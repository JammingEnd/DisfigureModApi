using DisfigureModApi.Modules;
using DisfigureModApi;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DisfigureModApi.UpgradeCreationTools
{
    public class UpgradeStatWrapper
    {
        public string name;
        public float value;
    }

    public class DesclinesWrapper
    {
        public string UpperLine;
        public string LowerLine;
    }

    public class NewUpgrade : Upgrade
    {
        public bool isInitial = false;
        /// <summary>
        /// Content mods override this to build their upgrade tree into the player.
        /// Called by the API's <c>PlayerStats.Start</c> patch once per match.
        /// </summary>
        public virtual void BuildUpgradeTree(PlayerStats pS) { }
    }

    public class UpgradeRegistry
    {
        public static Dictionary<NewUpgrade, bool> newUpgrades = new Dictionary<NewUpgrade, bool>();
        public static Dictionary<string, object> registeredStats = new Dictionary<string, object>();

        public static void RegisterUpgrade(NewUpgrade upgrade)
        {
            ModApi.Log.LogMessage("Registering upgrade: " + upgrade.GetType().Name);
            newUpgrades.Add(upgrade, false);
        }

        public static void RegisterStat(string name, float value)
        {
            registeredStats[name] = value;
        }

        public static void RegisterStat(string name, int value)
        {
            registeredStats[name] = value;
        }

        public static void RegisterStat(string name, bool value)
        {
            registeredStats[name] = value;
        }

        /// <summary>
        /// Applies all registered content to a player at match start: seeds the
        /// registered base stats into <see cref="ModdedPlayerStats"/> and builds every
        /// registered upgrade tree via <see cref="NewUpgrade.BuildUpgradeTree"/>.
        /// Guarded per player instance so it only runs once per match.
        /// </summary>
        public static void ApplyToPlayer(PlayerStats pS)
        {
            if (pS == null)
            {
                return;
            }

            ModdedPlayerStats modded = pS.gameObject.GetComponent<ModdedPlayerStats>();
            if (modded == null)
            {
                modded = pS.gameObject.AddComponent<ModdedPlayerStats>();
            }
            if (modded.initialized)
            {
                return;
            }
            modded.initialized = true;

            foreach (var kv in registeredStats)
            {
                switch (kv.Value)
                {
                    case float f:
                        modded.moddedStats[kv.Key] = new ModdedStatWrapper(f);
                        break;
                    case int i:
                        modded.moddedStats[kv.Key] = new ModdedStatWrapper(i);
                        break;
                    case bool b:
                        modded.moddedStats[kv.Key] = new ModdedStatWrapper(b);
                        break;
                }
            }

            foreach (var kv in newUpgrades)
            {
                kv.Key.BuildUpgradeTree(pS);
            }
        }

        public static void Ini()
        {
            ModApi.Log.LogMessage("UpgradeRegistry loading.....");
            newUpgrades.Clear();
            registeredStats.Clear();
        }
    }

    public static class UpgradeUtils
    {
        public static void AssignInitialUpgrade(this PlayerStats playerStats, GameObject upgrade, List<GameObject> otherUpgrades)
        {
            Upgrade neWupgrade = upgrade.GetComponent<Upgrade>();
            GameObject newUpgradePathPanel = GameObject.Instantiate(neWupgrade.upgradePathsPanel);
            upgradepathspanel newPathPanel = newUpgradePathPanel.GetComponent<upgradepathspanel>();

            newPathPanel.upgradesList[0] = upgrade;
            for (int i = 1; i < 6 + 1; i++)
            {
                ModApi.Log.LogMessage("Assigning upgrade: " + otherUpgrades[i - 1].name);
                otherUpgrades[i - 1].GetComponent<Upgrade>().upgradePathsPanel = newUpgradePathPanel;
                newPathPanel.upgradesList[i] = otherUpgrades[i - 1];
            }

            newUpgradePathPanel.gameObject.name = "P." + neWupgrade.upgradeName;
            neWupgrade.upgradePathsPanel = newUpgradePathPanel;

            playerStats.unlockedUpgrades.Add(upgrade);
            playerStats.upgrades.Add(upgrade);
        }

        public static bool HasChosenStat(this Upgrade upgrade, string name)
        {
            foreach (var statName in new[] { upgrade.statName, upgrade.statName2, upgrade.statName3, upgrade.statName4, upgrade.statName5 })
            {
                if (statName == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static void AddNewStat(this ModdedPlayerStats stats, string name, bool value)
        {
            stats.moddedStats[name] = new ModdedStatWrapper(value);
        }

        public static void AddNewStat(this ModdedPlayerStats stats, string name, int value)
        {
            stats.moddedStats[name] = new ModdedStatWrapper(value);
        }

        public static void AddNewStat(this ModdedPlayerStats stats, string name, float value)
        {
            stats.moddedStats[name] = new ModdedStatWrapper(value);
        }

        private static Upgrade ClearStats(this Upgrade upgrade)
        {
            upgrade.statName = "";
            upgrade.change = 0;
            upgrade.statName2 = "";
            upgrade.change2 = 0;
            upgrade.statName3 = "";
            upgrade.change3 = 0;
            upgrade.statName4 = "";
            upgrade.change4 = 0;
            upgrade.statName5 = "";
            upgrade.change5 = 0;

            upgrade.unlocks.Clear();

            for (int i = 0; i < upgrade.desclines.Length; i++)
            {
                upgrade.desclines[i] = "";
            }

            upgrade.statdescription = "";

            return upgrade;
        }

        public static GameObject BuildUpgrade(this PlayerStats pS, string name, string desc, UpgradeStatWrapper change1, GameObject unlock1 = null, GameObject unlock2 = null, DesclinesWrapper desclines = null, String spriteUpgradeName = "", Color? spriteColor = null)
        {
            return BuildUpgradeCore(pS, name, desc, unlock1, unlock2, desclines, spriteUpgradeName, spriteColor, change1);
        }

        public static GameObject BuildUpgrade(this PlayerStats pS, string name, string desc, UpgradeStatWrapper change1, UpgradeStatWrapper change2, GameObject unlock1 = null, GameObject unlock2 = null, DesclinesWrapper desclines = null, String spriteUpgradeName = "", Color? spriteColor = null)
        {
            return BuildUpgradeCore(pS, name, desc, unlock1, unlock2, desclines, spriteUpgradeName, spriteColor, change1, change2);
        }

        public static GameObject BuildUpgrade(this PlayerStats pS, string name, string desc, UpgradeStatWrapper change1, UpgradeStatWrapper change2, UpgradeStatWrapper change3, GameObject unlock1 = null, GameObject unlock2 = null, DesclinesWrapper desclines = null, String spriteUpgradeName = "", Color? spriteColor = null)
        {
            return BuildUpgradeCore(pS, name, desc, unlock1, unlock2, desclines, spriteUpgradeName, spriteColor, change1, change2, change3);
        }

        public static GameObject BuildUpgrade(this PlayerStats pS, string name, string desc, UpgradeStatWrapper change1, UpgradeStatWrapper change2, UpgradeStatWrapper change3, UpgradeStatWrapper change4, GameObject unlock1 = null, GameObject unlock2 = null, DesclinesWrapper desclines = null, String spriteUpgradeName = "", Color? spriteColor = null)
        {
            return BuildUpgradeCore(pS, name, desc, unlock1, unlock2, desclines, spriteUpgradeName, spriteColor, change1, change2, change3, change4);
        }

        public static GameObject BuildUpgrade(this PlayerStats pS, string name, string desc, UpgradeStatWrapper change1, UpgradeStatWrapper change2, UpgradeStatWrapper change3, UpgradeStatWrapper change4, UpgradeStatWrapper change5, GameObject unlock1 = null, GameObject unlock2 = null, DesclinesWrapper desclines = null, String spriteUpgradeName = "", Color? spriteColor = null)
        {
            return BuildUpgradeCore(pS, name, desc, unlock1, unlock2, desclines, spriteUpgradeName, spriteColor, change1, change2, change3, change4, change5);
        }

        private static GameObject BuildUpgradeCore(PlayerStats pS, string name, string desc, GameObject unlock1, GameObject unlock2, DesclinesWrapper desclines, String spriteUpgradeName, Color? spriteColor, params UpgradeStatWrapper[] changes)
        {
            GameObject newUpgrade = GameObject.Instantiate(pS.upgrades[0]);
            newUpgrade.name = "U." + name;

            Upgrade upgrade = newUpgrade.GetComponent<Upgrade>();
            upgrade.ClearStats();

            upgrade.upgradeName = name;
            upgrade.statdescription = desc;

            ApplyStat(upgrade, changes, 0);
            ApplyStat(upgrade, changes, 1);
            ApplyStat(upgrade, changes, 2);
            ApplyStat(upgrade, changes, 3);
            ApplyStat(upgrade, changes, 4);

            if (desclines != null)
            {
                if (desclines.UpperLine != null)
                {
                    upgrade.desclines[0] = desclines.UpperLine;
                }
                if (desclines.LowerLine != null)
                {
                    upgrade.desclines[1] = desclines.LowerLine;
                }
            }
            else
            {
                upgrade.desclines[0] = " ";
                upgrade.desclines[1] = " ";
            }
            if (unlock1 != null)
            {
                upgrade.unlocks.Add(unlock1);
                pS.unlockedUpgrades.Add(unlock1);
            }
            if (unlock2 != null)
            {
                upgrade.unlocks.Add(unlock2);
                pS.unlockedUpgrades.Add(unlock2);
            }

            if (!string.IsNullOrEmpty(spriteUpgradeName) || spriteColor.HasValue)
            {
                List<GameObject> allUpgrades = CollectUpgradePool(pS);
                GameObject source = null;

                if (!string.IsNullOrEmpty(spriteUpgradeName))
                {
                    foreach (GameObject candidate in allUpgrades)
                    {
                        if (candidate != null && candidate.name.StartsWith(spriteUpgradeName, StringComparison.Ordinal))
                        {
                            source = candidate;
                            break;
                        }
                    }
                    if (source == null)
                    {
                        ModApi.Log.LogWarning("Upgrade not found in pool: " + spriteUpgradeName);
                    }
                }
                else
                {
                    source = allUpgrades.Count > 0
                        ? allUpgrades[UnityEngine.Random.Range(0, allUpgrades.Count)]
                        : null;
                }

                Image sourceImage = source != null ? source.GetComponent<Image>() : null;
                Image targetImage = upgrade.GetComponent<Image>();
                if (sourceImage != null && targetImage != null)
                {
                    targetImage.sprite = sourceImage.sprite;
                    if (spriteColor.HasValue)
                    {
                        targetImage.color = spriteColor.Value;
                    }
                }
            }

            ModApi.Log.LogMessage("Build Upgrade : | " + name + " |");

            return newUpgrade;
        }

        /// <summary>
        /// Recursively collects every upgrade GameObject reachable from the starting pool,
        /// following each upgrade's <c>unlocks</c> chain.
        /// </summary>
        private static List<GameObject> CollectUpgradePool(PlayerStats pS)
        {
            List<GameObject> pool = new List<GameObject>();
            if (pS.upgrades != null)
            {
                foreach (GameObject start in pS.upgrades)
                {
                    CollectUpgradeRecursive(start, pool);
                }
            }
            return pool;
        }

        private static void CollectUpgradeRecursive(GameObject current, List<GameObject> pool)
        {
            if (current == null || pool.Contains(current))
            {
                return;
            }

            pool.Add(current);

            Upgrade upgrade = current.GetComponent<Upgrade>();
            if (upgrade == null || upgrade.unlocks == null)
            {
                return;
            }

            foreach (GameObject next in upgrade.unlocks)
            {
                CollectUpgradeRecursive(next, pool);
            }
        }

        private static void ApplyStat(Upgrade upgrade, UpgradeStatWrapper[] changes, int index)
        {
            if (index >= changes.Length || changes[index] == null)
            {
                return;
            }

            switch (index)
            {
                case 0:
                    upgrade.statName = changes[index].name;
                    upgrade.change = changes[index].value;
                    break;
                case 1:
                    upgrade.statName2 = changes[index].name;
                    upgrade.change2 = changes[index].value;
                    break;
                case 2:
                    upgrade.statName3 = changes[index].name;
                    upgrade.change3 = changes[index].value;
                    break;
                case 3:
                    upgrade.statName4 = changes[index].name;
                    upgrade.change4 = changes[index].value;
                    break;
                case 4:
                    upgrade.statName5 = changes[index].name;
                    upgrade.change5 = changes[index].value;
                    break;
            }
        }

        public static bool IsUpgradePresent(this PlayerStats stats, string name)
        {
            foreach (var upgrade in stats.unlockedUpgrades)
            {
                if (upgrade.name.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }

        public static void SetCustomStats(this Upgrade upgrade)
        {
            PlayerStats stats = upgrade.pS;
            ModdedPlayerStats moddedStats = stats.gameObject.GetComponent<ModdedPlayerStats>();

            ApplyStatToModded(moddedStats, upgrade.statName, upgrade.change);
            ApplyStatToModded(moddedStats, upgrade.statName2, upgrade.change2);
            ApplyStatToModded(moddedStats, upgrade.statName3, upgrade.change3);
            ApplyStatToModded(moddedStats, upgrade.statName4, upgrade.change4);
            ApplyStatToModded(moddedStats, upgrade.statName5, upgrade.change5);
        }

        private static void ApplyStatToModded(ModdedPlayerStats moddedStats, string name, float change)
        {
            if (moddedStats.moddedStats.TryGetValue(name, out ModdedStatWrapper statWrapper))
            {
                statWrapper.SetStatValue(change);
                if (name != "")
                {
                    ModApi.Log.LogMessage("Setting stat: " + name + " By " + change);
                }
            }
        }
    }
}