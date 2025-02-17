using DisfigureModApi.Modules;
using DisfigurwModApi;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DisfigureModApi.UpgradeCreationTools
{
    public class NewUpgradePath : upgradepathspanel
    {
    }

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

    public class UpgradeRegistry
    {
        public static Dictionary<NewUpgrade, bool> newUpgrades = new Dictionary<NewUpgrade, bool>();

        public static void RegisterUpgrade(NewUpgrade upgrade)
        {
            ModApi.Log.LogMessage("Registering upgrade: " + upgrade.GetType().Name);
            newUpgrades.Add(upgrade, false);
        }

        public static void Ini()
        {
            ModApi.Log.LogMessage("UpgradeRegistry loading.....");
            newUpgrades.Clear();
        }
    }

    public class NewUpgrade : Upgrade
    {
        public bool isInitial = false;
    }

    public static class UpgradeUtils
    {
        public static void AssignInitialUpgrade(this PlayerStats playerStats, GameObject upgrade, List<GameObject> otherUpgrades)
        {
            Upgrade neWupgrade = upgrade.GetComponent<Upgrade>();
            GameObject newUpgradePathPanel = GameObject.Instantiate(neWupgrade.upgradePathsPanel);
            upgradepathspanel newPathPanel = newUpgradePathPanel.GetComponent<upgradepathspanel>();

            /* for (int i = 0; i < newUpgradePathPanel.transform.childCount; i++)
             {
                 int childname = i + 1;
                 GameObject child = newUpgradePathPanel.transform.FindChild(childname.ToString()).gameObject;
                 if (child != null)
                 {
                    child.GetComponent<Button>().enabled = true;
                 }
             }*/

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
            if (upgrade.statName == name)
            {
                return true;
            }
            if (upgrade.statName2 == name)
            {
                return true;
            }
            if (upgrade.statName3 == name)
            {
                return true;
            }
            if (upgrade.statName4 == name)
            {
                return true;
            }
            if (upgrade.statName5 == name)
            {
                return true;
            }
            return false;
        }

        public static void AddNewStat(this ModdedPlayerStats stats, string name, bool value)
        {
            stats.moddedStats.Add(new ModdedStatWrapper(name, value));
        }

        public static void AddNewStat(this ModdedPlayerStats stats, string name, int value)
        {
            stats.moddedStats.Add(new ModdedStatWrapper(name, value));
        }

        public static void AddNewStat(this ModdedPlayerStats stats, string name, float value)
        {
            stats.moddedStats.Add(new ModdedStatWrapper(name, value));
        }

        public static object GetStatByName(this ModdedPlayerStats stats, string name)
        {
            foreach (var stat in stats.moddedStats)
            {
                if (stat.GetStatName() == name)
                {
                    return stat.GetStatValue();
                }
            }
            return null;
        }

        public static ModdedStatWrapper GetStatWrapperByName(this ModdedPlayerStats stats, string name)
        {
            foreach (var stat in stats.moddedStats)
            {
                if (stat.GetStatName() == name)
                {
                    return stat;
                }
            }
            return null;
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

        public static GameObject BuildUpgrade(this PlayerStats pS, string name, string desc, UpgradeStatWrapper change1, GameObject unlock1 = null, GameObject unlock2 = null, DesclinesWrapper desclines = null)
        {
            GameObject newUpgrade = GameObject.Instantiate(pS.upgrades[0]);
            newUpgrade.name = "U." + name;

            Upgrade upgrade = newUpgrade.GetComponent<Upgrade>();
            upgrade.ClearStats();

            upgrade.upgradeName = name;
            upgrade.statdescription = desc;

            upgrade.statName = change1.name;
            upgrade.change = change1.value;

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

            ModApi.Log.LogMessage("Build Upgrade : | " + name + " |");

            return newUpgrade;
        }

        public static GameObject BuildUpgrade(this PlayerStats pS, string name, string desc, UpgradeStatWrapper change1, UpgradeStatWrapper change2, GameObject unlock1 = null, GameObject unlock2 = null, DesclinesWrapper desclines = null)
        {
            GameObject newUpgrade = GameObject.Instantiate(pS.upgrades[0]);
            newUpgrade.name = "U." + name;

            Upgrade upgrade = newUpgrade.GetComponent<Upgrade>();
            upgrade.ClearStats();

            upgrade.upgradeName = name;
            upgrade.statdescription = desc;

            upgrade.statName = change1.name;
            upgrade.change = change1.value;

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

            if (change2 != null)
            {
                upgrade.statName2 = change2.name;
                upgrade.change2 = change2.value;
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

            ModApi.Log.LogMessage("Build Upgrade : | " + name + " |");

            return newUpgrade;
        }

        public static GameObject BuildUpgrade(this PlayerStats pS, string name, string desc, UpgradeStatWrapper change1, UpgradeStatWrapper change2, UpgradeStatWrapper change3, GameObject unlock1 = null, GameObject unlock2 = null, DesclinesWrapper desclines = null)
        {
            GameObject newUpgrade = GameObject.Instantiate(pS.upgrades[0]);
            newUpgrade.name = "U." + name;

            Upgrade upgrade = newUpgrade.GetComponent<Upgrade>();
            upgrade.ClearStats();

            upgrade.upgradeName = name;
            upgrade.statdescription = desc;

            upgrade.statName = change1.name;
            upgrade.change = change1.value;

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
            if (change2 != null)
            {
                upgrade.statName2 = change2.name;
                upgrade.change2 = change2.value;
            }

            if (change3 != null)
            {
                upgrade.statName3 = change3.name;
                upgrade.change3 = change3.value;
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

            ModApi.Log.LogMessage("Build Upgrade : | " + name + " |");

            return newUpgrade;
        }

        public static GameObject BuildUpgrade(this PlayerStats pS, string name, string desc, UpgradeStatWrapper change1, UpgradeStatWrapper change2, UpgradeStatWrapper change3, UpgradeStatWrapper change4, GameObject unlock1 = null, GameObject unlock2 = null, DesclinesWrapper desclines = null)
        {
            GameObject newUpgrade = GameObject.Instantiate(pS.upgrades[0]);
            newUpgrade.name = "U." + name;

            Upgrade upgrade = newUpgrade.GetComponent<Upgrade>();
            upgrade.ClearStats();

            upgrade.upgradeName = name;
            upgrade.statdescription = desc;

            upgrade.statName = change1.name;
            upgrade.change = change1.value;

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
            if (change2 != null)
            {
                upgrade.statName2 = change2.name;
                upgrade.change2 = change2.value;
            }

            if (change3 != null)
            {
                upgrade.statName3 = change3.name;
                upgrade.change3 = change3.value;
            }

            if (change4 != null)
            {
                upgrade.statName4 = change4.name;
                upgrade.change4 = change4.value;
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

            ModApi.Log.LogMessage("Build Upgrade : | " + name + " |");

            return newUpgrade;
        }

        public static GameObject BuildUpgrade(this PlayerStats pS, string name, string desc, UpgradeStatWrapper change1, UpgradeStatWrapper change2, UpgradeStatWrapper change3, UpgradeStatWrapper change4, UpgradeStatWrapper change5, GameObject unlock1 = null, GameObject unlock2 = null, DesclinesWrapper desclines = null)
        {
            GameObject newUpgrade = GameObject.Instantiate(pS.upgrades[0]);
            newUpgrade.name = "U." + name;

            Upgrade upgrade = newUpgrade.GetComponent<Upgrade>();
            upgrade.ClearStats();

            upgrade.upgradeName = name;
            upgrade.statdescription = desc;

            upgrade.statName = change1.name;
            upgrade.change = change1.value;

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
            if (change2 != null)
            {
                upgrade.statName2 = change2.name;
                upgrade.change2 = change2.value;
            }

            if (change3 != null)
            {
                upgrade.statName3 = change3.name;
                upgrade.change3 = change3.value;
            }

            if (change4 != null)
            {
                upgrade.statName4 = change4.name;
                upgrade.change4 = change4.value;
            }

            if (change5 != null)
            {
                upgrade.statName5 = change5.name;
                upgrade.change5 = change5.value;
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

            ModApi.Log.LogMessage("Build Upgrade : | " + name + " |");

            return newUpgrade;
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

            if (moddedStats.TryGetStat(upgrade.statName, out ModdedStatWrapper statWrapper))
            {
                statWrapper.SetStatValue(upgrade.change);
                if(upgrade.statName != "")
                {
                    ModApi.Log.LogMessage("Setting stat: " + upgrade.statName + " By " + upgrade.change);
                }
            }
            if (moddedStats.TryGetStat(upgrade.statName2, out statWrapper))
            {
                statWrapper.SetStatValue(upgrade.change2);
                if (upgrade.statName2 != "")
                {
                    ModApi.Log.LogMessage("Setting stat: " + upgrade.statName2 + " By " + upgrade.change2);
                }
            }
            if (moddedStats.TryGetStat(upgrade.statName3, out statWrapper))
            {
                statWrapper.SetStatValue(upgrade.change3);
                if (upgrade.statName3 != "")
                {
                    ModApi.Log.LogMessage("Setting stat: " + upgrade.statName3 + " By " + upgrade.change3);
                }
            }
            if (moddedStats.TryGetStat(upgrade.statName4, out statWrapper))
            {
                statWrapper.SetStatValue(upgrade.change4);
                if (upgrade.statName4 != "")
                {
                    ModApi.Log.LogMessage("Setting stat: " + upgrade.statName4 + " By " + upgrade.change4);
                }
            }
            if (moddedStats.TryGetStat(upgrade.statName5, out statWrapper))
            {
                statWrapper.SetStatValue(upgrade.change5);
                if (upgrade.statName5 != "")
                {
                    ModApi.Log.LogMessage("Setting stat: " + upgrade.statName5 + " By " + upgrade.change5);
                }
            }
        }

        private static bool TryGetStat(this ModdedPlayerStats allStat, string name, out ModdedStatWrapper statWrapper)
        {
            foreach (var stat in allStat.moddedStats)
            {
                if (stat.GetStatName() == name)
                {
                    statWrapper = stat;
                    return true;
                }
            }

            statWrapper = null;
            return false;
        }
    }
}