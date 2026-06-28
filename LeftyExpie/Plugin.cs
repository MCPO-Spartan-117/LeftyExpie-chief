using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Logging;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace LeftyExpie
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("net.cucorelib", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "leftie.leftyexpie";
        public const string ModName = "LeftyExpie";
        public const string ModVersion = "2.0.0";

        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new(ModGUID);
        public static Plugin Instance { get; private set; } = null!;

        public static Sprite[] LeftHandSwapSprites { get; private set; } = null!;
        public static Sprite[] AmbiHandSwapSprites { get; private set; } = null!;
        public static Sprite[] HandMoodletSprites { get; private set; } = null!;

        void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            LeftHandSwapSprites = [
                AssetLoader.LoadSpriteFromPluginFolder(this, "Images/handswitchR.png"),
                AssetLoader.LoadSpriteFromPluginFolder(this, "Images/handswitchL.png")
            ];
            AmbiHandSwapSprites = [
                AssetLoader.LoadSpriteFromPluginFolder(this, "Images/handswitchR_A.png"),
                AssetLoader.LoadSpriteFromPluginFolder(this, "Images/handswitchL.png")
            ];
            HandMoodletSprites = [
                AssetLoader.LoadSpriteFromPluginFolder(this, "Images/handednessR.png"),
                AssetLoader.LoadSpriteFromPluginFolder(this, "Images/handednessL.png"),
                AssetLoader.LoadSpriteFromPluginFolder(this, "Images/handednessA.png"),
            ];

            _harmony.PatchAll();

            ModOptionsRegistry.Register(ModOptionDefinition.Float("leftyexpie.leftychance",
                LocaleRegistry.Get("other", "gamesetleftyexpie.leftychance", "Left handedness chance"),
                LocaleRegistry.Get("other", "gamesetleftyexpie.leftychancedsc", "Chance that the Experiment will be left handed when starting a new descent.\nIf they are not left handed, they will be right handed."),
                Setting.SettingCategory.Game,
                PlayerPrefs.GetFloat("LeftyExpie_LeftyChance", 0.1f),
                0.01f,
                1f,
                value =>
                {
                    PlayerPrefs.SetFloat("LeftyExpie_LeftyChance", value);
                    PlayerPrefs.Save();
                },
                value => Mathf.RoundToInt(value * 100f) + "%"

            ));
            ModOptionsRegistry.Register(ModOptionDefinition.Bool("leftyexpie.ambidextrous",
                LocaleRegistry.Get("other", "gamesetleftyexpie.ambidextrous", "Enable ambidextrous Experiments"),
                LocaleRegistry.Get("other", "gamesetleftyexpie.ambidextrousdsc", "If enabled, there is a 1% chance that the Experiment will be ambidextrous when starting a new descent.\nAmbidextrous Experiments can use both hands equally well, but are 10% weaker across both hands as a tradeoff."),
                Setting.SettingCategory.Game,
                PlayerPrefs.GetInt("LeftyExpie_Ambidextrous", 1) == 1,
                value =>
                {
                    PlayerPrefs.SetInt("LeftyExpie_Ambidextrous", value ? 1 : 0);
                    PlayerPrefs.Save();
                }
            ));
            ModOptionsRegistry.Register(ModOptionDefinition.Bool("leftyexpie.handednessmoodles",
                LocaleRegistry.Get("other", "gamesetleftyexpie.handednessmoodles", "Show handedness moodles"),
                LocaleRegistry.Get("other", "gamesetleftyexpie.handednessmoodlesdsc", "Display low importance moodles of the Experiment's handedness.\nYou can still identify handedness from the hand button in the Health Panel."),
                Setting.SettingCategory.Game,
                PlayerPrefs.GetInt("LeftyExpie_HandednessMoodles", 1) == 1,
                value =>
                {
                    PlayerPrefs.SetInt("LeftyExpie_HandednessMoodles", value ? 1 : 0);
                    PlayerPrefs.Save();
                }
            ));

            Logger.LogInfo($"Plugin {ModName} is loaded!");
        }

        void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            Instance = null!;
        }
    }
}
