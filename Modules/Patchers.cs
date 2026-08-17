using DisfigureModApi.Modules;
using DisfigureModApi.UImanipulation;
using DisfigureModApi.UpgradeCreationTools;
using DisfigureModApi.Util;
using DisfigureModApi.WeaponCreationTools;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DisfigureModApi
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
        /// <summary>
        /// Applies a modded weapon's persisted unlock state to its button
        /// (mirrors the game reading FBPP "<c>weaponnameUnlocked</c>").
        /// </summary>
        public static void Postfix(weaponselect __instance)
        {
            NewWeapon weapon = NewWeaponInitiator.GetWeapon(__instance.weaponname);
            if (weapon == null)
            {
                return;
            }

            __instance.unlockedString = weapon.UnlockKey;
            __instance.weaponIsUnlocked = weapon.IsUnlocked;
            __instance.selectedColor = Color.red;
        }
    }

    /// <summary>
    /// Persists the selected modded weapon using the same FBPP key the game uses
    /// ("selectedWeapon"), so the choice travels from the home screen to the match scene.
    /// </summary>
    [HarmonyPatch(typeof(weaponselect), "selectWeapon")]
    public class WeaponSelectPatchOnSelect
    {
        public static void Postfix(weaponselect __instance)
        {
            NewWeapon weapon = NewWeaponInitiator.GetWeapon(__instance.weaponname);
            if (weapon == null)
            {
                return;
            }

            ModApi.Log.LogMessage("Weapon selected: " + __instance.weaponname);
            weapon.Select();
            __instance.dIH?.showChosenWeapon(__instance.weaponname);
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

    /// <summary>
    /// Intercepts <see cref="ButtonControl.OnSelect"/> for the API's "More >>" clone so it
    /// clears the weapon slots instead of running the vanilla back-button behaviour.
    /// </summary>
    [HarmonyPatch(typeof(ButtonControl), "OnSelect")]
    public class MoreButtonOnSelectPatch
    {
        public static bool Prefix(ButtonControl __instance)
        {
            if (__instance.gameObject.name != "MoreButton")
            {
                return true;
            }

            UIinteractor.ClearGunButtons();
            return false; // skip the vanilla OnSelect body
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
            NewWeapon activeWeapon = WeaponUtils.GetActiveWeapon();
            if (activeWeapon == null)
            {
                return;
            }

            GameObject instanceHeldWeapon = WeaponUtils.SetHeldWeapon(pool, wM, activeWeapon);
            if (instanceHeldWeapon == null)
            {
                return;
            }

            instanceHeldWeapon.transform.position = stats.gameObject.transform.position;
            instanceHeldWeapon.transform.rotation = stats.gameObject.transform.rotation;

            if (instanceHeldWeapon.transform.childCount >= 2)
            {
                Transform child1 = instanceHeldWeapon.transform.GetChild(1);
                if (child1.childCount >= 2)
                {
                    stats.windUpActivateParticle = child1.GetChild(0).gameObject;
                    stats.windUpActivateParticle.SetActive(false);

                    stats.windUpReadyFlashParticle = child1.GetChild(1).gameObject;
                    stats.windUpReadyFlashParticle.SetActive(false);
                }
            }

            instanceHeldWeapon.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(ObjectPool), "Awake")]
    public class ObjectPoolPatchAwake
    {
        public static void Postfix(ObjectPool __instance)
        {
            __instance.pS.gameObject.AddComponent<ModdedPlayerStats>();
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