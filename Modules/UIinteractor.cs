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

        private static GameObject moreButtonClone;

        private static readonly Vector3 MoreButtonPosition = new Vector3(-59.9823f, -65, -53.6812f);

        private static readonly List<NewWeapon> assignedWeapons = new();

        public static void IniUIInteractor()
        {
            currentButtonName = "";
        }

        public static void ResetAssignments()
        {
            assignedWeapons.Clear();
        }

        /// <summary>Destroys the clone and drops its reference so it is recreated fresh next time.</summary>
        public static void ResetMoreButton()
        {
            if (moreButtonClone != null)
            {
                GameObject.Destroy(moreButtonClone);
            }
            moreButtonClone = null;
        }

        private static bool IsAssigned(NewWeapon weapon)
        {
            return assignedWeapons.Contains(weapon);
        }

        /// <summary>
        /// Assigns unassigned registered modded weapons to free gun-button slots under
        /// <paramref name="panel"/>. A slot is free when it has a <see cref="weaponselect"/>
        /// that is not yet a modded weapon and whose label is "COMING SOON".
        /// </summary>
        public static void AssignWeaponsToFreeSlots(Transform panel)
        {
            if (NewWeaponInitiator.newWeapons.Count == 0)
            {
                return;
            }

            foreach (GameObject button in panel.FindGunButtons())
            {
                weaponselect wpS = button.GetComponent<weaponselect>();
                if (wpS == null)
                {
                    continue;
                }

                if (NewWeaponInitiator.GetWeapon(wpS.weaponname) != null)
                {
                    // Already assigned to a modded weapon; keep it.
                    continue;
                }

                Text textComp = button.transform.GetChild(0).GetComponent<Text>();
                if (textComp == null || textComp.text != "COMING SOON")
                {
                    continue;
                }

                NewWeapon weapon = NewWeaponInitiator.newWeapons.Find(w => !IsAssigned(w));
                if (weapon == null)
                {
                    return;
                }

                ModApi.Log.LogMessage("Assigning weapon: " + weapon.weaponName + " to slot " + button.name);
                textComp.text = weapon.weaponName;
                wpS.weaponname = weapon.weaponReference;
                wpS.unlockedString = weapon.UnlockKey;
                wpS.weaponIsUnlocked = weapon.IsUnlocked;
                wpS.selectedColor = Color.red;
                wpS.enabled = true;
                button.GetComponent<Button>().enabled = true;
                assignedWeapons.Add(weapon);
            }
        }

        /// <summary>
        /// Creates the "More >>" clone of the back button under Canvas/Start on first use.
        /// No-op if the clone already exists or no active back button is found.
        /// </summary>
        public static void EnsureMoreButtonCreated()
        {
            if (moreButtonClone != null)
            {
                return;
            }

            BackButton source = null;
            foreach (BackButton bb in UnityEngine.Object.FindObjectsOfType<BackButton>())
            {
                if (bb.gameObject.name == "back" && bb.gameObject.activeInHierarchy)
                {
                    source = bb;
                    break;
                }
            }
            if (source == null)
            {
                ModApi.Log.LogMessage("MoreButton: no active 'back' button found to clone.");
                return;
            }

            moreButtonClone = GameObject.Instantiate(source.gameObject, source.transform.parent);
            moreButtonClone.name = "MoreButton";
            moreButtonClone.transform.position = MoreButtonPosition;

            BackButton cloneBack = moreButtonClone.GetComponent<BackButton>();
            if (cloneBack != null)
            {
                cloneBack.enabled = false;
            }
            ButtonControl cloneControl = moreButtonClone.GetComponent<ButtonControl>();
            if (cloneControl != null)
            {
                cloneControl.enabled = false;
            }

            SetMoreButtonLabel(moreButtonClone);

            Button cloneButton = moreButtonClone.GetComponent<Button>();
            if (cloneButton != null)
            {
                cloneButton.onClick.RemoveAllListeners();
                cloneButton.onClick.AddListener(ClearGunButtons);
            }
            ModApi.Log.LogMessage("MoreButton created at " + moreButtonClone.transform.position);
        }

        /// <summary>
        /// Sets the back button's label child to "More >>".
        /// </summary>
        private static void SetMoreButtonLabel(GameObject clone)
        {
            Text label = null;
            foreach (Text text in clone.GetComponentsInChildren<Text>(true))
            {
                if (!string.IsNullOrEmpty(text.text))
                {
                    label = text;
                    break;
                }
            }
            if (label != null)
            {
                label.text = "More >>";
            }
            else
            {
                ModApi.Log.LogMessage("MoreButton: no Text child found to label.");
            }
        }

        /// <summary>
        /// Captures the selectedColor of the first GunButton that is not holding a modded
        /// weapon — the "vanilla" default — so cleared slots can be restored to it
        /// (weaponselect.Start won't re-fire). Skips slots already stamped by the
        /// assignment logic (those carry the modded red color).
        /// </summary>
        private static Color SnapshotDefaultButtonColor(Transform panel)
        {
            foreach (GameObject button in panel.FindGunButtons())
            {
                weaponselect wpS = button.GetComponent<weaponselect>();
                if (wpS == null)
                {
                    continue;
                }
                if (NewWeaponInitiator.GetWeapon(wpS.weaponname) != null)
                {
                    // Holds a modded weapon; its color is the modded red, not vanilla.
                    continue;
                }
                return wpS.selectedColor;
            }
            return Color.white;
        }

        /// <summary>
        /// Clears every active GunButton under the More button's parent panel back to a free
        /// slot ("COMING SOON", no weapon), then reassigns registered modded weapons.
        /// No-op when no modded weapons are registered.
        /// </summary>
        public static void ClearGunButtons()
        {
            if (NewWeaponInitiator.newWeapons.Count == 0 || moreButtonClone == null)
            {
                return;
            }

            Transform panel = moreButtonClone.transform.parent;
            if (panel == null)
            {
                return;
            }

            Color defaultColor = SnapshotDefaultButtonColor(panel);

            foreach (GameObject button in panel.FindGunButtons())
            {
                weaponselect wpS = button.GetComponent<weaponselect>();
                if (wpS == null)
                {
                    continue;
                }

                Text textComp = button.transform.GetChild(0).GetComponent<Text>();
                if (textComp != null)
                {
                    textComp.text = "COMING SOON";
                }

                wpS.weaponname = "";
                wpS.unlockedString = "";
                wpS.weaponIsUnlocked = false;
                wpS.selectedColor = defaultColor;
                wpS.enabled = true;
                button.GetComponent<Button>().enabled = true;
            }

            ModApi.Log.LogMessage("MoreButton: cleared all gun buttons.");
            ResetAssignments();
            AssignWeaponsToFreeSlots(panel);
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
                UIinteractor.EnsureMoreButtonCreated();
                UIinteractor.AssignWeaponsToFreeSlots(__instance.gameObject.transform);
            }
        }

        [HarmonyPatch(typeof(StartMenu), "OnDisable")]
        public class UIinteractorStartDisable
        {
            public static void Postfix(StartMenu __instance)
            {
                // Reset assignment bookkeeping so the menu can be rebuilt next time.
                UIinteractor.ResetAssignments();
                UIinteractor.ResetMoreButton();
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