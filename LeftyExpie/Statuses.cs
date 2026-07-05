using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using HarmonyLib;
using UnityEngine;

namespace LeftyExpie
{
    [StatusOptions(Key = "leftyexpie.handedness", SaveEnabled = true)]
    public sealed class HandednessStatus : BodyStatus
    {
        public bool NewRun { get; set; } = true;
        public int Handedness { get; set; } = -1;
    }

    [HarmonyPatch(typeof(Body), "Update")]
    public static class BodyUpdateStatusPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Body __instance)
        {
            HandednessStatus status = __instance.GetStatus<HandednessStatus>();
            if (status.Handedness == -1)
            {
                int random = UnityEngine.Random.Range(1, 101);
                int rightyChance = Mathf.RoundToInt((1f - PlayerPrefs.GetFloat("LeftyExpie_LeftyChance", 0.1f)) * 100f);
                if (random <= rightyChance)
                {
                    status.Handedness = 0; // Right handed
                }
                else
                {
                   status.Handedness = 1; // Left handed
                }
                int random2 = UnityEngine.Random.Range(0, 101);
                int ambiChance = Mathf.RoundToInt(PlayerPrefs.GetFloat("LeftyExpie_AmbidextrousChance", 0.01f) * 100f);
                if (random2 <= ambiChance)
                {
                    status.Handedness = 2; // Ambidextrous
                }
            }
        }

        [HarmonyPostfix]
        private static void Postfix(Body __instance)
        {
            if (PlayerPrefs.GetInt("LeftyExpie_HandednessMoodles", 1) == 1)
            {
                HandednessStatus status = __instance.GetStatus<HandednessStatus>();

                switch (status.Handedness)
                {
                    case 0:
                        MoodleRegistry.AddMoodle(
                            0,
                            Plugin.HandMoodletSprites[0],
                            LocaleRegistry.Get("other", "leftyexpie.righthandedname", "Right handed"),
                            LocaleRegistry.Get("other", "leftyexpie.righthandeddsc", "You are more dextrous with your right hand. Just how the coats like it."),
                            important: false
                        );
                        break;

                    case 1:
                        MoodleRegistry.AddMoodle(
                            0,
                            Plugin.HandMoodletSprites[1],
                            LocaleRegistry.Get("other", "leftyexpie.lefthandedname", "Left handed"),
                            LocaleRegistry.Get("other", "leftyexpie.lefthandeddsc", "You are more dextrous with your left hand. You weren't meant to be."),
                            important: false
                        );
                        break;

                    case 2:
                        MoodleRegistry.AddMoodle(
                            8,
                            Plugin.HandMoodletSprites[2],
                            LocaleRegistry.Get("other", "leftyexpie.ambihandedname", "Ambidextrous"),
                            LocaleRegistry.Get("other", "leftyexpie.ambihandeddsc", "You are equally dextrous with both hands. Aren't you so special?"),
                            important: false
                        );
                        break;
                }
            }
        }
    }
}
