using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GameNetcodeStuff;
using UnityEngine;

namespace TooMuchScrap
{
    public static class MergeClass
    {
        public static int Merge(out string? error)
        {
            error = null;

            if (!IsHost(out error))
                return -1;

            if (TooMuchScrap.CompanyOnly.Value && !IsInCompanyBuilding(out error))
                return -1;
            HashSet<string> mergeableItems = TooMuchScrap.GetMergeableItems();

            GrabbableObject[] scrapInShip = GetScrapInShip();
            int destroyedCount = MergeAllScrap(scrapInShip, mergeableItems);
            return destroyedCount;
        }
        private static bool IsInCompanyBuilding(out string? error)
        {
            if (RoundManager.Instance.currentLevel.sceneName != "CompanyBuilding")
            {
                error = "The /merge command can only be used in the Company Building. (You can change this in the config)";
                return false;
            }
            error = null;
            return true;
        }

        private static bool IsHost(out string? error)
        {
            if (!GameNetworkManager.Instance.localPlayerController.IsHost)
            {
                error = "Only the host can execute the /merge command to ensure proper networking.";
                return false;
            }
            error = null;
            return true;
        }

        private static GrabbableObject[] GetScrapInShip()
        {
            return Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None)
                .Where(x => x != null && x.itemProperties.isScrap && x.isInShipRoom)
                .ToArray();
        }

        private static int MergeAllScrap(GrabbableObject[] scrapInShip, HashSet<string> mergeableItems)
        {
            HashSet<int> processed = new HashSet<int>();
            int destroyedCount = 0;

            foreach (GrabbableObject obj in scrapInShip)
            {
                if (obj == null || processed.Contains(obj.GetInstanceID()))
                    continue;

                string itemName = obj.name.Replace("(Clone)", "");
                if (!mergeableItems.Contains(itemName))
                    continue;

                int destroyed = MergeNearbyScrap(obj, mergeableItems, processed);
                destroyedCount += destroyed;
            }

            return destroyedCount;
        }

        private static int MergeNearbyScrap(GrabbableObject source, HashSet<string> mergeableItems, HashSet<int> processed)
        {
            int destroyedCount = 0;
            int sourceId = source.GetInstanceID();

            GrabbableObject[] mergeCandidates = FindMergeCandidates(source);
            if (mergeCandidates.Length < 2)
                return 0;

            GrabbableObject[] nearby = mergeCandidates
                .Where(x => Vector3.Distance(x.transform.position, source.transform.position) < TooMuchScrap.MergeDistance.Value)
                .ToArray();

            if (nearby.Length < 2)
                return 0;

            List<GrabbableObject> toMerge = nearby.OrderBy(x => x.scrapValue).ToList();
            int totalValue = source.scrapValue;
            List<GrabbableObject> merged = new List<GrabbableObject> { source };

            foreach (var obj in toMerge)
            {
                if (obj == null || processed.Contains(obj.GetInstanceID()) || obj.GetInstanceID() == sourceId)
                    continue;

                if (totalValue + obj.scrapValue > TooMuchScrap.MaxMergeValue.Value)
                    continue;

                totalValue += obj.scrapValue;
                merged.Add(obj);
            }

            source.SetScrapValue(totalValue);

            foreach (var obj in merged.Where(x => x != source))
            {
                if (obj == null) continue;
                processed.Add(obj.GetInstanceID());
                Object.Destroy(obj.gameObject);
                destroyedCount++;
            }

            processed.Add(sourceId);
            return destroyedCount;
        }

        private static GrabbableObject[] FindMergeCandidates(GrabbableObject source)
        {
            return Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None)
                .Where(x =>
                    x != null &&
                    x.isInShipRoom &&
                    x.itemProperties.isScrap &&
                    x.name == source.name &&
                    !x.isHeld)
                .ToArray();
        }
    }
}
