using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using TooMuchScrap;
using UnityEngine.UIElements.UIR;

namespace TooMuchScrap
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class TooMuchScrap : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "abu";
        public const string PluginName = "TooMuchScrap";
        public const string PluginVersion = "1.2.0";

        public static ConfigEntry<float> MergeDistance { get; private set; } = null!;
        public static ConfigEntry<float> MaxMergeValue { get; private set; } = null!;
        public static ConfigEntry<string> MergeableItems { get; private set; } = null!;
        public static ConfigEntry<string> PrefixChar { get; private set; } = null!;
        public static ConfigEntry<bool> CompanyOnly { get; private set; } = null!;
        public static ConfigEntry<bool> AutoMerge { get; private set; } = null!;

        private static HashSet<string>? _mergeableItemsCache = null;

        public static TooMuchScrap Instance = null!;

        private void Awake()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            

            MergeDistance = Config.Bind("General", "MergeDistance", 3F, "Maximum distance at which scrap will merge.");
            MaxMergeValue = Config.Bind("General", "MaxMergeValue", 200f, "Maximum merged scrap value.");
            MergeableItems = Config.Bind("General", "MergeableItems",
                "HeartContainer,SeveredHandLOD0,SeveredFootLOD0,SeveredThighLOD0,Bone,RibcageBone,Ear,Tongue",
                "Comma-separated list of item names that can be merged.");
            PrefixChar = Config.Bind("General", "PrefixChar", "/", "Character that prefixes chat commands.");
            CompanyOnly = Config.Bind("General", "CompanyOnly", true, "Only allow merging of scrap at the company building.");
            AutoMerge = Config.Bind("General", "AutoMerge", false, "Automatically merge scrap when going in orbit.");

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
            Instance.Config.Reload();
            ClearCache(); // Clear cached parsed data
        }
    }
}

[HarmonyPatch(typeof(HUDManager))]
internal class MergeCommandPatch
{
    [HarmonyPatch("AddTextToChatOnServer")]
    [HarmonyPrefix]

    public static bool AddTextToChatOnServer_Prefix(HUDManager __instance, ref string chatMessage)
    {
        var match = System.Text.RegularExpressions.Regex.Match(chatMessage, $@"^{TooMuchScrap.TooMuchScrap.PrefixChar.Value}merge(?:\s+(-?\d+))?$");
        if (match.Success)
        {
            TooMuchScrap.TooMuchScrap.ReloadConfig();
            int destroyedCount = MergeClass.Merge(out string? error);
            if (destroyedCount >= 0)
            {
                __instance.AddTextToChatOnServer($"[TMS] Total scrap removed: {destroyedCount}");
                return false;
            }
            else
            {
                // Show error message in chat
                __instance.AddTextToChatOnServer($"[TMS] {error}");
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(StartOfRound))]
internal class AutoMergePatch
{
    [HarmonyPatch("SetShipReadyToLand")]
    [HarmonyPostfix]
    public static void SetShipReadyToLand_Postfix(StartOfRound __instance)
    {
        TooMuchScrap.TooMuchScrap.ReloadConfig();
        if (TooMuchScrap.TooMuchScrap.AutoMerge.Value)
        {
            int destroyedCount = MergeClass.Merge(out string? error);
            if (destroyedCount > 0)
            {
                HUDManager.Instance.AddTextToChatOnServer($"[TMS] Auto-merge complete. Total scrap removed: {destroyedCount}");
            }
        }
    }
}