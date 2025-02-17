using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;
using DisfigurwModApi.Util;
using DisfigurwModApi.WeaponCreationTools;

namespace DisfigurwModApi.UImanipulation
{
    public class UIinteractor
    {
        /// <summary>
        /// List of all the newly added weapons (from  the buttons)
        /// </summary>
        public static Dictionary<string[], bool> NewlyAddedWeaponsList = new();

        /// <summary>
        /// List of all the base-game weapon prefabs
        /// </summary>
        public static Dictionary<GameObject, string> weaponPrefabs = new();

        public static string currentButtonName = "";

        public static void IniUIInteractor()
        {
            //UIChanger.ChangeUI();
            NewlyAddedWeaponsList.Clear();
        }

        [HarmonyPatch(typeof(weaponselect), "OnPointerEnter")]
        public class UIinteractorOnPointerEnter
        {
            public static void Posfix(weaponselect __instance)
            {
                currentButtonName = __instance.gameObject.name;
            }
        }

        [HarmonyPatch(typeof(weaponselect), "Start")]
        public class UIinteractorOnEnable
        {
            public static void Postfix(weaponselect __instance)
            {
                if (!__instance.gameObject.IsAvaibleButton())
                {
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(StartMenu), "OnEnable")]
        public class UIinteractorStart
        {
            public static void Postfix(StartMenu __instance)
            {
                // Iterate through all the childs of the start menu
                for (int i = 0; i < __instance.gameObject.transform.childCount; i++)
                {
                    if (NewWeaponInitiator.newWeapons.ToList()[NewWeaponInitiator.newWeapons.Count - 1].Key.IsGenereated)
                    {
                        return;
                    }
                    // Get the current child for use in the loop
                    Transform currentChild = __instance.gameObject.transform.GetChild(i);
                    if (!currentChild.gameObject.IsAvaibleButton())
                    {
                        // If the current child is not a button, skip it
                        continue;
                    }
                    Text textComp;
                    if (currentChild.transform.GetChild(0).TryGetComponent(out Text result))
                    {
                        // If the current child has a text component, assign it to the textComp variable
                        if(result.gameObject.activeSelf == false)
                        {
                            continue;
                        }
                        textComp = result;
                        textComp.text = "COMING SOON";
                    }
                    else
                    {
                        continue;
                    }

                    // If the text of the button is a "COMING SOON" button
                    if (textComp.text == "COMING SOON")
                    {
                       

                        foreach (var weapon in NewWeaponInitiator.newWeapons)
                        {
                            if (weapon.Key.IsGenereated == false)
                            {
                                ModApi.Log.LogMessage("Generating Weapon: " + weapon.Key.weaponName);
                                textComp.text = weapon.Key.weaponName;
                                weapon.Key.IsGenereated = true;

                                weaponselect wpS = textComp.transform.parent.GetComponent<weaponselect>();
                                wpS.weaponname = weapon.Key.weaponReference;
                                wpS.enabled = true;
                                textComp.transform.parent.GetComponent<Button>().enabled = true;

                                break;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StartMenu), "OnDisable")]
        public class UIinteractorStartDisable
        {
            public static void Postfix(StartMenu __instance)
            {
                // Reset the generated weapons to false, otherwise the weapons will not be generated
                foreach (var weapon in NewWeaponInitiator.newWeapons)
                {
                    weapon.Key.IsGenereated = false;
                }
            }
        }

        [HarmonyPatch(typeof(displayimagehandler), "showChosenWeapon")]
        public class UIinteractorWeaponDisplayShow
        {
            public static void Prefix(displayimagehandler __instance, ref string weaponname)
            {
            }

            public static void Postfix(displayimagehandler __instance, ref string weaponname)
            {
                foreach (var weapon in NewWeaponInitiator.newWeapons)
                {
                    if (weaponname == weapon.Key.weaponReference)
                    {
                        weapon.Key.BuildWeapon(__instance, weaponname);
                    }
                }

                HashSet<GameObject> seen = new HashSet<GameObject>();
                List<GameObject> toDestroy = new List<GameObject>();

                for (int i = 0; i < __instance.gameObject.transform.childCount; i++)
                {
                    seen.Add(__instance.transform.GetChild(i).gameObject);
                }

                foreach (var obj in seen)
                {
                    if (obj == null) continue; // Skip null GameObjects

                    // Check if the object matches the string condition
                    if (obj.name.Contains(weaponname) || !seen.Add(obj))
                    {
                        // Either it's a duplicate or doesn't match the string condition
                        toDestroy.Add(obj);
                    }
                }

                string refname = weaponname;
                // Destroy all unwanted objects
                foreach (var obj in toDestroy)
                {
                    if (obj.gameObject.name.Contains(weaponname))
                    {
                        if (toDestroy.Where(x => x.name.Contains(refname)).Count() == 1)
                        {
                            continue;
                        }
                    }

                    seen.Remove(obj); // Remove from the list
                    GameObject.Destroy(obj); // Destroy the GameObject
                }
            }
        }
    }
}