using System.Collections.Generic;
using System.Linq;
using ChatCommandAPI;
using GameNetcodeStuff;
using UnityEngine;
using ScrapMerging;
using BepInEx.Configuration;

namespace ScrapMerging
{
    // Inherit from Command from the ChatCommandAPI
    public class MergeCommand : Command
    {
        // Sets the command name that users will type in chat (e.g., /merge)
        public override string Name => "merge";

        // Sets the description for the /help command
        public override string Description => "Immediately merges all eligible scrap in the ship without needing to drop them.";

        // The logic that runs when the command is invoked
        public override bool Invoke(string[] args, Dictionary<string, string> kwargs, out string? error)
        {
            error = null;

            // Chat commands that modify game state should typically only run on the host/server.
            if (!GameNetworkManager.Instance.localPlayerController.IsHost)
            {
                error = "Only the host can execute the /merge command to ensure proper networking.";
                return false;
            }

            // Clear cache to get fresh settings from config
            ScrapMerging.ReloadConfig();
            
            // Get the set of mergeable items from config
            HashSet<string> mergeableItems = ScrapMerging.GetMergeableItems();

            // Find all GrabbableObjects currently loaded in the game
            GrabbableObject[] allObjects = Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None);

            // Filter for scrap items that are inside the ship
            GrabbableObject[] scrapInShip = allObjects
                .Where(x => x != null && x.itemProperties.isScrap && x.isInShipRoom)
                .ToArray();

            // Keep track of processed/destroyed objects so we don't double-count or reprocess them.
            HashSet<int> processed = new HashSet<int>();

            // Iterate over all scrap and call the existing MergeScrap logic.
            // Using a processed set ensures deterministic behavior regardless of iteration order.
            foreach (GrabbableObject __instance in scrapInShip)
            {
                if (__instance is null)
                    continue;

                int id = __instance.GetInstanceID();
                if (processed.Contains(id))
                    continue;

                string itemName = __instance.name.Replace("(Clone)", "");
                
                // Check if this item is in the mergeable list
                if (!__instance.itemProperties.isScrap
                     || !__instance.isInShipRoom 
                     || !mergeableItems.Contains(itemName))
                {
                    continue;
                }

                GrabbableObject[] mergeCandidates = Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None)
                    .Where(x => x != null
                             && x.isInShipRoom 
                             && x.itemProperties.isScrap 
                             && x.name == __instance.name)
                    .ToArray();
                if (mergeCandidates.Length < 2) { continue; }

                GrabbableObject[] mergableObjects = mergeCandidates.Where(
                    x => Vector3.Distance(x.transform.position, __instance.transform.position) < ScrapMerging.MergeDistance.Value
                ).ToArray();
                if (mergableObjects.Length < 2) { continue; }

                // Sort by value (optional, ensures predictable merges)
                List<GrabbableObject> toMerge = mergableObjects.OrderBy(x => x.scrapValue).ToList();

                int totalValue = __instance.scrapValue;
                List<GrabbableObject> merged = new List<GrabbableObject> { __instance };

                foreach (var obj in toMerge)
                {
                    if (obj is null)
                        continue;

                    int objId = obj.GetInstanceID();
                    if (objId == id)
                        continue;

                    if (processed.Contains(objId))
                        continue;

                    if (totalValue + obj.scrapValue > ScrapMerging.MaxMerge.Value)
                        continue; // skip merging this one if it would exceed the limit

                    totalValue += obj.scrapValue;
                    merged.Add(obj);
                }

                __instance.SetScrapValue(totalValue);

                // Destroy only those that were merged and mark them processed
                foreach (var obj in merged.Where(x => x != __instance))
                {
                    if (obj is null) continue;
                    processed.Add(obj.GetInstanceID());
                    Object.Destroy(obj.gameObject);
                }

                // Mark the survivor as processed so it won't be source for another merge pass
                processed.Add(id);
            }

            ChatCommandAPI.ChatCommandAPI.Print("Scrap merged.");

            return true;
        }
    }
}