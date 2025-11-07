using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TooMuchScrap
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class TooMuchScrap : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "valentinps";
        public const string PluginName = "TooMuchScrap";
        public const string PluginVersion = "1.0.2";

        public static ConfigEntry<float> MergeDistance { get; private set; } = null!;
        public static ConfigEntry<float> MaxMergeValue { get; private set; } = null!;
        public static ConfigEntry<string> MergeableItems { get; private set; } = null!;

        private static HashSet<string>? _mergeableItemsCache = null;

        private void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            MergeDistance = Config.Bind("General", "MergeDistance", 3F, "Maximum distance at which scrap will merge.");
            MaxMergeValue = Config.Bind("General", "MaxMergeValue", 200f, "Maximum merged scrap value.");
            MergeableItems = Config.Bind("General", "MergeableItems",
                "HeartContainer,SeveredHandLOD0,SeveredFootLOD0,SeveredThighLOD0,Bone,RibcageBone",
                "Comma-separated list of item names that can be merged.");

            _ = new MergeCommand();
        }

        public static HashSet<string> GetMergeableItems()
        {
            if (_mergeableItemsCache == null || string.IsNullOrWhiteSpace(MergeableItems?.Value))
            {
                _mergeableItemsCache = new HashSet<string>();
                if (!string.IsNullOrWhiteSpace(MergeableItems?.Value))
                {
                    foreach (string item in MergeableItems.Value.Split(','))
                    {
                        string trimmed = item.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed))
                        {
                            _mergeableItemsCache.Add(trimmed);
                        }
                    }
                }
            }
            return _mergeableItemsCache;
        }

        public static void ClearCache()
        {
            _mergeableItemsCache = null;
        }
        public static void ReloadConfig()
        {
            if (BepInEx.Bootstrap.Chainloader.ManagerObject == null)
                return;

            var pluginInstance = BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<TooMuchScrap>();
            if (pluginInstance == null)
                return;

            pluginInstance.Config.Reload(); // Re-read from disk
            ClearCache(); // Clear cached parsed data
        }
    }
}