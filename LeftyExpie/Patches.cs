using System;
using BepInEx;
using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Plugin = LeftyExpie.Plugin;
using CUCoreLib.Registries;
using UnityEngine.UI;
using TMPro;

namespace LeftyExpie
{
    internal class Patches
    {
        [HarmonyPatch(typeof(PlayerCamera), "SwitchHands")]
        public static class PlayerCameraSwitchHandsPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(PlayerCamera __instance)
            {
                __instance.PlayUISound(PlayerCamera.UISoundType.Click, 1f);
                __instance.body.handSlot = ((__instance.body.handSlot == 0) ? 1 : 0);
                __instance.handSwapImage.sprite = Plugin.HandSwapSprites[__instance.body.handSlot];
                if (__instance.body.handSlot == 0)
                {
                    __instance.DoAlert(LocaleRegistry.Get("other", "leftyexpie.handswitchwarning", "<color=\"orange\">WARNING\n</color>You have less dexterity in your right hand"), false);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerCamera), "Start")]
        public static class PlayerCameraStartPatch
        {
            [HarmonyPrefix]
            private static void Prefix (PlayerCamera __instance)
            {
                __instance.SwitchHands();
            }

            [HarmonyPostfix]
            private static void Postfix(PlayerCamera __instance)
            {
                UILocalizer text1 = __instance.radialMenu.transform.Find("Text (TMP)").GetComponent<UILocalizer>();
                TextMeshProUGUI text2 = __instance.radialMenu.transform.Find("Text (TMP) (1)").GetComponent<TextMeshProUGUI>();

                text1.key = "secondaryhand";
                text2.text = Locale.GetOther("mainhand");
            }
        }

        [HarmonyPatch(typeof(WoundView), "Start")]
        public static class WoundViewStartPatch
        {
            [HarmonyPostfix]
            private static void Postfix(WoundView __instance)
            {
                UITooltip uitooltip = __instance.transform.Find("HandButton").GetComponent<UITooltip>();
                uitooltip.skipLocale = true;
                uitooltip.tipDesc = LocaleRegistry.Get("other", "handswitchdsc", "Swap your main hand between the Left/Right\nUse this to dig with your second arm if the main one is broken\nYour right arm is slightly weaker than the left");
            }
        }

        [HarmonyPatch(typeof(Body), "Attack")]
        public static class BodyAttackPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(Body __instance, ref AttackInfo atk, ref int slot, ref bool __result)
            {
                if (__instance.conscious && __instance.attackCooldown <= 0f)
                {
                    __instance.liquidDrinkTime = 0f;
                    slot = __instance.handSlot;
                    atk.damage *= WorldGeneration.GetRunSettingFloat("attackdamage");
                    atk.structuralDamage *= WorldGeneration.GetRunSettingFloat("attackdamage");
                    if ((__instance.isRight && __instance.targetLookPos.x < __instance.transform.position.x) || (!__instance.isRight && __instance.targetLookPos.x > __instance.transform.position.x))
                    {
                        __instance.SwitchDir();
                    }
                    Vector2 vector = (__instance.targetLookPos - __instance.limbs[1].transform.position).normalized;
                    if (atk.physicalSwing)
                    {
                        foreach (Limb limb in __instance.slots[slot].useLimbs)
                        {
                            if (limb.broken || limb.dislocated)
                            {
                                limb.pain += 2f * limb.brokenPainMultiplier;
                            }
                        }
                        __instance.temperature += 0.125f * atk.cooldown;
                        if (__instance.limbs[0].broken)
                        {
                            __instance.limbs[0].pain += 2f * __instance.limbs[0].brokenPainMultiplier;
                        }
                        if (__instance.limbs[1].broken || __instance.limbs[1].dislocated)
                        {
                            __instance.limbs[1].pain += 2f * __instance.limbs[1].brokenPainMultiplier;
                        }
                        float d = __instance.isRight ? 1f : -1f;
                        __instance.armOffset = Vector2.SignedAngle(__instance.limbs[1].transform.right * d, vector);
                        __instance.visualBodyOffset += vector * (atk.rotateAmount * 0.03f);
                        if (!__instance.standing)
                        {
                            for (int j = 3; j < 10; j++)
                            {
                                __instance.limbs[j].rb.AddForce(vector * 800f);
                                __instance.limbs[1].rb.AddForce(-vector * 800f);
                            }
                        }
                    }
                    __instance.attackRot -= atk.rotateAmount * (__instance.isRight ? 1f : -1f);
                    if (atk.doAttackAnim)
                    {
                        __instance.armsAnimator.Play("ArmsSwing", -1, 0f);
                    }
                    __instance.stamina -= atk.staminaUse;
                    float num = 1f;
                    if (atk.physicalSwing)
                    {
                        num = __instance.slots[slot].armPowerMult;
                        if (slot == 0)
                        {
                            num *= 0.75f;
                        }
                        num *= 1f + __instance.skills.STRFrom10 * 0.0334f;
                        __instance.attackCooldown = atk.cooldown / (__instance.consciousness * 0.01f) * (1f + __instance.overEncumberance) / (1f + __instance.stimulantMultiplier * 0.66f);
                        __instance.TryExertSound(atk.cooldown * 1.15f, 0.35f);
                    }
                    else
                    {
                        __instance.attackCooldown = atk.cooldown;
                    }
                    if (atk.unarmed)
                    {
                        num *= __instance.clawDamageCurve.Evaluate(__instance.clawHealth);
                    }
                    RaycastHit2D[] array = Physics2D.RaycastAll(__instance.limbs[1].transform.position, vector, atk.distance);
                    Sound.Play(atk.swingSounds[Random.Range(0, atk.swingSounds.Length)], __instance.transform.position, false, true, __instance.transform, atk.volume, 1f, false, false);
                    if (atk.attackAnim)
                    {
                        GameObject gameObject = Object.Instantiate<GameObject>(atk.attackAnim);
                        gameObject.transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(__instance.isRight ? Vector3.right : Vector3.left, vector));
                        gameObject.transform.localScale = new Vector3(__instance.isRight ? 1f : -1f, 1f, 1f);
                        gameObject.transform.position = __instance.limbs[1].transform.position;
                        gameObject.transform.SetParent(__instance.transform);
                        Object.Destroy(gameObject, 5f);
                    }
                    bool flag = false;
                    foreach (RaycastHit2D raycastHit2D in array)
                    {
                        if (raycastHit2D.transform != __instance.transform)
                        {
                            if (raycastHit2D.transform.CompareTag("BlockGround"))
                            {
                                WorldGeneration.world.DamageBlock(raycastHit2D.point + vector * 0.05f, atk.structuralDamage * num, true, atk.metalMoreDamage);
                                WorldGeneration.CreateDamageNumber(raycastHit2D.point, (int)(atk.structuralDamage * num));
                                WorldGeneration.world.CreateHitFlash(PlayerCamera.main.defaultHoverSquareSprite, WorldGeneration.world.BlockToWorldPos(WorldGeneration.world.WorldToBlockPos(raycastHit2D.point + vector * 0.05f)), Quaternion.identity, Color.gray, null);
                                __instance.CreateCloudSmall(raycastHit2D.point, new Vector2?(raycastHit2D.normal * 4f));
                                flag = true;
                                if (!atk.piercing)
                                {
                                    break;
                                }
                            }
                            BuildingEntity buildingEntity;
                            if (raycastHit2D.transform.TryGetComponent<BuildingEntity>(out buildingEntity) && !buildingEntity.cantHit)
                            {
                                if (raycastHit2D.rigidbody)
                                {
                                    raycastHit2D.rigidbody.AddForceAtPosition(vector * num * atk.knockBack, raycastHit2D.point, ForceMode2D.Impulse);
                                }
                                buildingEntity.health -= (buildingEntity.animal ? atk.damage : atk.structuralDamage) * num * ((atk.metalMoreDamage && buildingEntity.metallic) ? 10f : 1f);
                                WorldGeneration.CreateDamageNumber(raycastHit2D.point, (int)((buildingEntity.animal ? atk.damage : atk.structuralDamage) * num));
                                SpriteRenderer spriteRenderer;
                                if (buildingEntity.TryGetComponent<SpriteRenderer>(out spriteRenderer))
                                {
                                    WorldGeneration.world.CreateHitFlash(spriteRenderer.sprite, buildingEntity.transform.position, buildingEntity.transform.rotation, Color.red, buildingEntity.transform);
                                }
                                Sound.Play(buildingEntity.hitSound, raycastHit2D.point, false, true, null, 1f, 1f, false, false);
                                __instance.CreateCloudSmall(raycastHit2D.point, new Vector2?(raycastHit2D.normal * 4f));
                                if (atk.unarmed)
                                {
                                    SawbladeScript sawbladeScript;
                                    if (raycastHit2D.transform.TryGetComponent<SawbladeScript>(out sawbladeScript))
                                    {
                                        __instance.Ragdoll();
                                    }
                                    CoilScript coilScript;
                                    if (raycastHit2D.transform.TryGetComponent<CoilScript>(out coilScript))
                                    {
                                        coilScript.Shock(__instance.limbs[0]);
                                    }
                                }
                                if (buildingEntity.animal)
                                {
                                    raycastHit2D.transform.gameObject.SendMessage("AnimalHit", atk.damage * num);
                                    __instance.attackCooldown *= 3.5f * atk.attackCooldownMult;
                                    PlayerCamera.main.lastAttackCool = __instance.attackCooldown;
                                }
                                else
                                {
                                    raycastHit2D.transform.gameObject.SendMessage("BuildingHit", atk, SendMessageOptions.DontRequireReceiver);
                                }
                                flag = true;
                                if (!atk.piercing)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    if (flag)
                    {
                        if (__instance.standing)
                        {
                            __instance.rb.AddForce(-vector * atk.knockBack * num, ForceMode2D.Impulse);
                        }
                        else
                        {
                            __instance.limbs[1].rb.AddForce(-vector * atk.knockBack * num, ForceMode2D.Impulse);
                        }
                        if (atk.unarmed)
                        {
                            __instance.clawHealth -= 0.3f;
                            if (__instance.clawHealth < 20f && Random.value < 0.1f)
                            {
                                __instance.slots[slot].limb.skinHealth -= 3f;
                                __instance.slots[slot].limb.muscleHealth -= 2f;
                                __instance.slots[slot].limb.pain += 12f;
                                __instance.slots[slot].limb.bleedAmount += Random.Range(0.35f, 0.85f);
                            }
                        }
                        if (atk.physicalSwing)
                        {
                            __instance.dirtyness += atk.cooldown * 1f;
                            __instance.skills.AddExp(0, atk.damage / 300f);
                        }
                        __result = true;

                        return false;
                    }
                }
                __result = false;

                return false;
            }
        }
    }
}
