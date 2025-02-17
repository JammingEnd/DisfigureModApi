using DisfigureModApi.Modules;
using DisfigureModApi.UpgradeCreationTools;
using DisfigurwModApi.Util;
using DisfigurwModApi.WeaponCreationTools;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DisfigurwModApi
{
    public class Patcher
    {
        public static Harmony? harmony;

        public static void Ini()
        {
            harmony = new Harmony("com.disfigure.testmod");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(weaponselect), "Start")]
    public class WeaponSelectPatchStart
    {
        public static void Postfix(weaponselect __instance)
        {
            __instance.selectedColor = Color.red;
            if (__instance.weaponIsUnlocked == false)
            {
            }
        }
    }

    /// <summary>
    /// Makes a weapon value true when selected
    /// </summary>
    [HarmonyPatch(typeof(weaponselect), "selectWeapon")]
    public class WeaponSelectPatchOnSelect
    {
        public static void Postfix(weaponselect __instance)
        {
            ModApi.Log.LogMessage("Weapon selected: " + __instance.weaponname);
            // Reset all weapons to false
            foreach (var item in NewWeaponInitiator.newWeapons)
            {
                NewWeaponInitiator.newWeapons[item.Key] = false;
            }

            // Set the selected weapon to true
            // for all (currently) buttons
            if (__instance.gameObject.name == "GunButton (27)")
            {
                foreach (var item in NewWeaponInitiator.newWeapons)
                {
                    if (item.Key.weaponReference == __instance.weaponname)
                    {
                        NewWeaponInitiator.newWeapons[item.Key] = true;
                        NewWeaponInitiator.CurrentWeapon = item.Key.HeldWeapon;
                    }
                }
            }
            if (__instance.gameObject.name == "GunButton (28)")
            {
                foreach (var item in NewWeaponInitiator.newWeapons)
                {
                    if (item.Key.weaponReference == __instance.weaponname)
                    {
                        NewWeaponInitiator.newWeapons[item.Key] = true;
                        NewWeaponInitiator.CurrentWeapon = item.Key.HeldWeapon;
                    }
                }
            }

            if (__instance.gameObject.name == "GunButton (31)")
            {
                foreach (var item in NewWeaponInitiator.newWeapons)
                {
                    if (item.Key.weaponReference == __instance.weaponname)
                    {
                        NewWeaponInitiator.newWeapons[item.Key] = true;
                        NewWeaponInitiator.CurrentWeapon = item.Key.HeldWeapon;
                    }
                }
            }
            if (__instance.gameObject.name == "GunButton (32)")
            {
                foreach (var item in NewWeaponInitiator.newWeapons)
                {
                    if (item.Key.weaponReference == __instance.weaponname)
                    {
                        NewWeaponInitiator.newWeapons[item.Key] = true;
                        NewWeaponInitiator.CurrentWeapon = item.Key.HeldWeapon;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(StartMenu), "PlayGame")]
    public class StartMenuPatch
    {
        public static void Prefix()
        {
            ModApi.Log.LogMessage("PlayGame called");
        }
    }

    [HarmonyPatch(typeof(ObjectPool), "Start")]
    public class OnGameStartPatch
    {
        /// <summary>
        /// Activates when you load into the Map, used for setting up weapons and player stats
        /// </summary>
        public static event Action<PlayerStats, WeaponManager> OnGameStartForWeapons;

        public static event Action<PlayerStats, WeaponManager> OnGameStartForUpgrades;

        public static void Postfix(ObjectPool __instance)
        {
            PlayerStats player = __instance.pS;
            WeaponManager weaponManager = __instance.wM;
            OnGameStartForUpgrades?.Invoke(player, weaponManager);

            if (WeaponUtils.GetActiveWeapon() == null)
            {
                return;
            }

            SetupReferences(__instance, player, weaponManager);

            if (player != null)
            {
                OnGameStartForWeapons?.Invoke(player, weaponManager);
            }
        }

        private static void SetupReferences(ObjectPool pool, PlayerStats stats, WeaponManager wM)
        {
            GameObject instanceHeldWeapon = WeaponUtils.GetActiveWeapon().weaponPrefab;
            instanceHeldWeapon.transform.position = stats.gameObject.transform.position;
            instanceHeldWeapon.transform.rotation = stats.gameObject.transform.rotation;
            instanceHeldWeapon.transform.SetParent(wM.weaponModels.transform);

            stats.windUpActivateParticle = instanceHeldWeapon.transform.GetChild(1).GetChild(0).gameObject;
            stats.windUpActivateParticle.SetActive(false);

            stats.windUpReadyFlashParticle = instanceHeldWeapon.transform.GetChild(1).GetChild(1).gameObject;
            stats.windUpReadyFlashParticle.SetActive(false);

            //GameObject.Instantiate(WeaponUtils.SetHeldWeapon(pool, NewWeaponInitiator.CurrentWeapon), wM.weaponModels.transform.GetChild(0));

            instanceHeldWeapon.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(ObjectPool), "Awake")]
    public class ObjectPoolPatchAwake
    {
        public static void Postfix(ObjectPool __instance)
        {
            __instance.pS.gameObject.AddComponent<ModdedPlayerStats>();

            if (WeaponUtils.GetActiveWeapon() == null)
            {
                return;
            }

            NewWeapon activeWeapon = WeaponUtils.GetActiveWeapon();
            WeaponId id = activeWeapon.HeldWeapon;
            ModApi.Log.LogMessage($"Active weapon: {id}");
            WeaponUtils.GetActiveWeapon().weaponPrefab = GameObject.Instantiate(__instance.weaponsModelList[id.FromPreviewIdToModelId()], Vector3.zero, Quaternion.Euler(Vector3.zero));
        }
    }

    [HarmonyPatch(typeof(Upgrade), "UpgradeStat")]
    public class UpgradePatchUpgradeStat
    {
        public static void Prefix(Upgrade __instance)
        {
            ModApi.Log.LogMessage($"Upgrading " +
                $"{__instance.statName} by {__instance.change}" +
                $" || " +
                $"{__instance.statName2} by {__instance.change2}" +
                $" || " +
                $"{__instance.statName3} by {__instance.change3}" +
                " || " +
                $"{__instance.statName4} by {__instance.change4}" +
                $" || " +
                $"{__instance.statName5} by {__instance.change5}"
                );

            __instance.SetCustomStats();
        }
    }

    [HarmonyPatch(typeof(upgradepath), "OnPointerEnter")]
    public class UpgradePathPatchOnPointerEnter
    {
        public static void Postfix(upgradepath __instance, ref PointerEventData eventData)
        {
            if (__instance.pS.IsUpgradePresent(__instance.upgradeName))
            {
                __instance.ScaleUpOverTime(0.5f);

                GameObject textObject = __instance.gameObject.transform.parent.parent.parent.GetChild(2).gameObject;
                ModApi.Log.LogMessage("Text object: " + textObject.name);
                Text description = textObject.transform.GetChild(0).GetComponent<Text>();

                description.text = __instance.statdescription + "\n" + "\n";

                foreach (var item in __instance.desclines)
                {
                    description.text += item + "\n";
                }

                Text title = description.transform.GetChild(0).GetComponent<Text>();
                title.text = __instance.upgradeName;

                ModApi.Log.LogMessage("Showing upgrade");
                return;
            }
        }
    }

    [HarmonyPatch(typeof(upgradepath), "OnPointerExit")]
    public class UpgradePathPatchOnPointerExit
    {
        public static void Postfix(upgradepath __instance, ref PointerEventData eventData)
        {
            if (__instance.pS.IsUpgradePresent(__instance.upgradeName))
            {
                GameObject textObject = __instance.gameObject.transform.parent.parent.parent.GetChild(2).gameObject;
                Text description = textObject.transform.GetChild(0).GetComponent<Text>();

                description.text = __instance.pS.selectedUpgrade.statdescription + "\n" + "\n";
                foreach (var item in __instance.pS.selectedUpgrade.desclines)
                {
                    description.text += item + "\n";
                }

                Text title = description.transform.GetChild(0).GetComponent<Text>();
                title.text = __instance.pS.selectedUpgrade.getName();
                return;
            }
        }
    }

    [HarmonyPatch(typeof(weaponupgradescreen), "Awake")] 
    public class WeaponUpgradeScreenAwake
    {
        public static void Postfix(weaponupgradescreen __instance)
        {
            __instance.AddNewWeaponUpgradeTreesToPlayer(true);
        }
    }

    [HarmonyPatch(typeof(weaponupgradescreen), "OnEnable")]
    public class WeaponUpgradeScreenOnUpgrade
    {
        public static void Postfix(weaponupgradescreen __instance)
        {
            ModApi.Log.LogMessage(WeaponUtils.GetActiveWeapon().weaponName);
            GameObject currentUpgrades = __instance.weaponUpgradesList[__instance.weaponUpgradesList.Count - 1];
            if (WeaponUtils.GetActiveWeapon() != null)
            {
           

                __instance.temp = currentUpgrades;
                for (int i = 0; i < 8; i++)
                {
                    GameObject singleUpgrade = GameObject.Instantiate(currentUpgrades.transform.GetChild(i).gameObject);
                    ModApi.Log.LogMessage("Spawning upgrade: " + singleUpgrade.name);
                    //singleUpgrade.transform.localPosition = __instance.transformPositions[i].position;
                    //singleUpgrade.transform.parent = __instance.transformPositions[i];
                    __instance.chosenList.Add(singleUpgrade);

                }
            }
        }

        private static void Try1(weaponupgradescreen __instance)
        {
            GameObject upgrades = __instance.weaponUpgradesList[__instance.weaponUpgradesList.Count - 1];
            __instance.temp = upgrades;
            for (int i = 0; i < 8; i++)
            {
                GameObject singleUpgrade = upgrades.transform.GetChild(i).gameObject;
                singleUpgrade.transform.localPosition = __instance.transformPositions[i].position;
                singleUpgrade.transform.parent = __instance.transformPositions[i];
                __instance.chosenList.Add(singleUpgrade);
            }
        }
    }
}