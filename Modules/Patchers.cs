
using DisfigureTestMod.Util;
using DisfigureTestMod.Weapons;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DisfigureTestMod
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
        public static event Action<PlayerStats, WeaponManager> OnGameStart;
        public static void Postfix(ObjectPool __instance)
        {
            PlayerStats player = __instance.pS;
            WeaponManager weaponManager = __instance.wM;
            if (player != null)
            {
                ModApi.Log.LogMessage("PlayerStats found");
                player.circleVisionLength *= 3;

                OnGameStart?.Invoke(player, weaponManager);
                

                ModApi.Log.LogMessage(weaponManager);
            }
            else
            {
                ModApi.Log.LogMessage("PlayerStats not found");
            }
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
                $"{__instance.statName3} by {__instance.change3}"
                );
        }
    }
}