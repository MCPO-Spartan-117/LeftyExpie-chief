using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Logging;
using CUCoreLib.Helpers;
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
        public const string ModVersion = "1.0.3";

        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new(ModGUID);
        public static Plugin Instance { get; private set; } = null!;

        public static Sprite[] HandSwapSprites { get; private set; } = null!;

        void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            HandSwapSprites = [
                AssetLoader.LoadSpriteFromPluginFolder(this, "Images/handswitchR.png"),
                AssetLoader.LoadSpriteFromPluginFolder(this, "Images/handswitchL.png")
            ];

            _harmony.PatchAll();
            Logger.LogInfo($"Plugin {ModName} is loaded!");
        }

        void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            Instance = null!;
        }
    }
}
