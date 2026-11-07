using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using MonoMod.RuntimeDetour;
using UnityEngine;
using Newtonsoft.Json.Linq;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

// Use the below namespaces if you want to use CUCoreLib features. You can remove them if you don't need them.
using CUCoreLib.ContentReload;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using CUCoreLib.Saving;


namespace ModNamespace
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("net.cucorelib", BepInDependency.DependencyFlags.HardDependency)] 
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.yourName.modName";
        public const string ModName = "My First Casualties: Unknown Mod"; // To change .dll name, change the name in vars.targets
        public const string ModVersion = "1.0.0";

        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new(ModGUID);
        public static Plugin Instance { get; private set; } = null!;

        public void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            _harmony.PatchAll();
            Logger.LogInfo($"Plugin {ModName} is loaded!");

            DoStuff();
        }

        private void DoStuff()
        {
            // Put anything you want to do on plugin load here. For example, you can register new content, add new data, or modify existing game behavior!
        }
    }
}
