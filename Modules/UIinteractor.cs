using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;
using DisfigureModApi.Util;
using DisfigureModApi.WeaponCreationTools;
using System.Collections.Generic;

namespace DisfigureModApi.UImanipulation
{
    public class UIinteractor
    {
        public static string currentButtonName = "";

        public static void IniUIInteractor()
        {
            currentButtonName = "";
        }

        [HarmonyPatch(typeof(weaponselect), "OnPointerEnter")]
        public class UIinteractorOnPointerEnter
        {
            public static void Postfix(weaponselect __instance)
            {
                currentButtonName = __instance.gameObject.name;
            }
        }

        /// <summary>
        /// Assigns each registered modded weapon to a free weapon slot on the home screen.
        /// A free slot is a <see cref="weaponselect"/> button whose reference is not yet
        /// a vanilla weapon. (A "clear vanilla buttons" menu action is planned so any slot
        /// can be reused for modded weapons.)
        /// </summary>
        [HarmonyPatch(typeof(StartMenu), "OnEnable")]
        public class UIinteractorStart
        {
            public static void Postfix(StartMenu __instance)
            {
                if (NewWeaponInitiator.newWeapons.Count == 0)
                {
                    return;
                }

                // Track which slots already hold a weapon so we only fill free ones.
                foreach (Transform child in __instance.gameObject.transform)
                {
                    if (!child.gameObject.IsAvaibleButton())
                    {
                        continue;
                    }

                    weaponselect wpS = child.GetComponent<weaponselect>();
                    if (wpS == null)
                    {
                        continue;
                    }

                    if (NewWeaponInitiator.GetWeapon(wpS.weaponname) != null)
                    {
                        // Already assigned to a modded weapon; keep it.
                        continue;
                    }

                    Text textComp = child.GetChild(0).GetComponent<Text>();
                    if (textComp == null || textComp.text != "COMING SOON")
                    {
                        continue;
                    }

                    NewWeapon weapon = NewWeaponInitiator.newWeapons.Find(w => !IsAssigned(w));
                    if (weapon == null)
                    {
                        return;
                    }

                    ModApi.Log.LogMessage("Assigning weapon: " + weapon.weaponName + " to slot " + child.name);
                    textComp.text = weapon.weaponName;
                    wpS.weaponname = weapon.weaponReference;
                    wpS.unlockedString = weapon.UnlockKey;
                    wpS.weaponIsUnlocked = weapon.IsUnlocked;
                    wpS.enabled = true;
                    child.GetComponent<Button>().enabled = true;
                    assignedWeapons.Add(weapon);
                }
            }

            private static readonly List<NewWeapon> assignedWeapons = new();

            public static void ResetAssignments()
            {
                assignedWeapons.Clear();
            }

            private static bool IsAssigned(NewWeapon weapon)
            {
                return assignedWeapons.Contains(weapon);
            }
        }

        [HarmonyPatch(typeof(StartMenu), "OnDisable")]
        public class UIinteractorStartDisable
        {
            public static void Postfix(StartMenu __instance)
            {
                // Reset assignment bookkeeping so the menu can be rebuilt next time.
                UIinteractorStart.ResetAssignments();
            }
        }

        /// <summary>
        /// Builds the home-screen preview for a modded weapon when its display is shown
        /// (the game calls <see cref="displayimagehandler.showChosenWeapon(string)"/> with the
        /// selected weapon's name).
        /// </summary>
        [HarmonyPatch(typeof(displayimagehandler), "showChosenWeapon")]
        public class UIinteractorWeaponDisplayShow
        {
            public static void Postfix(displayimagehandler __instance, string weaponname)
            {
                NewWeapon weapon = NewWeaponInitiator.GetWeapon(weaponname);
                if (weapon == null)
                {
                    return;
                }

                weapon.BuildWeapon(__instance);
            }
        }
    }
}
