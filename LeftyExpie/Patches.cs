using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.Emit.OpCodes;
using BepInEx;
using BepInEx.Logging;
using CUCoreLib.Helpers;
using HarmonyLib;
using static HarmonyLib.CodeInstruction;
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
                HandednessStatus status = __instance.body.GetStatus<HandednessStatus>();
                switch (status.Handedness)
                {
                    case 0:
                        return true;
                    case 1:
                        __instance.PlayUISound(PlayerCamera.UISoundType.Click, 1f);
                        __instance.body.handSlot = ((__instance.body.handSlot == 0) ? 1 : 0);
                        __instance.handSwapImage.sprite = Plugin.LeftHandSwapSprites[__instance.body.handSlot];
                        if (__instance.body.handSlot == 0)
                        {
                            __instance.DoAlert(LocaleRegistry.Get("other", "leftyexpie.tooltip.handswitchwarning", "<color=\"orange\">WARNING\n</color>You have less dexterity in your right hand"), false);
                        }
                        return false;
                    case 2:
                        __instance.PlayUISound(PlayerCamera.UISoundType.Click, 1f);
                        __instance.body.handSlot = ((__instance.body.handSlot == 0) ? 1 : 0);
                        __instance.handSwapImage.sprite = Plugin.AmbiHandSwapSprites[__instance.body.handSlot];
                        return false;
                    default:
                        return true;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerCamera), "Start")]
        public static class PlayerCameraStartPatch
        {
            [HarmonyPostfix]
            private static void Postfix(PlayerCamera __instance)
            {
                HandednessStatus status = __instance.body.GetStatus<HandednessStatus>();
                if (status.NewRun)
                {
                    return;
                }
                if (status.Handedness == 1)
                {
                    __instance.SwitchHands();
                }
                switch (status.Handedness)
                {
                    case 0:
                    case 2:
                    default:
                        break;
                    case 1:
                        UILocalizer text1 = __instance.radialMenu.transform.Find("Text (TMP)").GetComponent<UILocalizer>();
                        TextMeshProUGUI text2 = __instance.radialMenu.transform.Find("Text (TMP) (1)").GetComponent<TextMeshProUGUI>();

                        text1.key = "secondaryhand";
                        text2.text = Locale.GetOther("mainhand");
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerCamera), "Update")]
        public static class PlayerCameraUpdatePatch
        {
            [HarmonyPostfix]
            private static void Postfix(PlayerCamera __instance)
            {
                HandednessStatus status = __instance.body.GetStatus<HandednessStatus>();
                if (status.NewRun)
                {
                    if (status.Handedness == -1) return;
                    else
                    {
                        if (status.Handedness == 1)
                        {
                            __instance.SwitchHands();
                            status.NewRun = false;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(WoundView), "Start")]
        public static class WoundViewStartPatch
        {
            [HarmonyPostfix]
            private static void Postfix(WoundView __instance)
            {
                HandednessStatus status = __instance.body.GetStatus<HandednessStatus>();
                UITooltip uitooltip = __instance.transform.Find("HandButton").GetComponent<UITooltip>();
                uitooltip.skipLocale = true;
                switch (status.Handedness)
                {
                    case 0:
                        uitooltip.tipDesc = Locale.GetOther("handswitchdsc");
                        break;
                    case 1:
                        uitooltip.tipDesc = LocaleRegistry.Get("other", "leftyexpie.tooltip.lefthandswitchdsc", "Swap your main hand between the Left/Right\nUse this to dig with your second arm if the main one is broken\nYour right arm is slightly weaker than the left");
                        break;
                    case 2:
                        uitooltip.tipDesc = LocaleRegistry.Get("other", "leftyexpie.tooltip.ambihandswitchdsc", "Swap your main hand between the Right/Left\nUse this to dig with your second arm if the main one is broken\nBeing ambidextrous, you can use both hands equally well");
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(Body), nameof(Body.Attack), argumentTypes: [typeof(AttackInfo), typeof(int)])]
        public static class BodyAttackPatch {
            internal static ManualLogSource Logger = Plugin.Logger;
            private static void handness_switch(Body body, ref int slot, ref float attackpower) {
                HandednessStatus status = body.GetStatus<HandednessStatus>();
                switch (status.Handedness) {
                    case 0:
                        if (slot == 1) {
                            attackpower *= 0.75f;
                        }
                    break;
                    case 1:
                        if (slot == 0) {
                            attackpower *= 0.75f;
                        }
                    break;
                    case 2:
                        attackpower *= 0.9f;
                    break;
                }
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                int startidx = -1;
                int endidx = -1;
                uint stage = 0;
                for (var i = 0; i < codes.Count; i++) {
                    switch(stage) {
                        case 0:
                            if(codes[i].opcode == Ldarg_2) {
                                startidx = i;
                            } else if(codes[i].operand?.GetType() == typeof(float) && (float)codes[i].operand == 0.75f) {
                                Logger.LogDebug("Found start " + startidx);
                                stage++;
                            }
                        break;
                        case 1:
                            if(codes[i].opcode == Stloc_1) {
                                endidx = i;
                                Logger.LogDebug("Found end " + i);
                                stage++;
                            }
                        break;
                        default:
                            i = codes.Count;
                        break;
                    }
                }

                if (endidx != -1) {
                    List<CodeInstruction> callfunct = new List<CodeInstruction> {
                        new(Ldarg_0),
                        new(Ldarga_S, 2),
                        new(Ldloca_S, 1),
                        CodeInstruction.Call(typeof(BodyAttackPatch), nameof(BodyAttackPatch.handness_switch))
                    };

                    codes.RemoveRange(startidx, endidx - startidx + 1);
                    codes.InsertRange(startidx, callfunct);
                } else {
                    Logger.LogError("Not patching");
                }

                return (IEnumerable<CodeInstruction>)codes;
            }
        }
    }
}
