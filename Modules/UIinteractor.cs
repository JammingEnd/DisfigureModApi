using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;
using DisfigurwModApi.Util;
using DisfigurwModApi.Weapons;

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
                if (__instance.gameObject.name == "GunButton (31)")
                {
                    ModApi.Log.LogMessage("Magic Wand hovered");
                }
            }
        }

        [HarmonyPatch(typeof(Text), "OnEnable")]
        public class UIinteractorStart
        {
            public static void Postfix(Text __instance)
            {
                // Check if the button is the "COMING SOON" button, indicating an unused button
                if (__instance.text == "COMING SOON")
                {
                    ModApi.Log.LogMessage("Found unused button" + "Registered weapons total: " + NewlyAddedWeaponsList.Count);

                    foreach (var weapon in NewWeaponInitiator.newWeapons)
                    {
                        if (weapon.Key.IsGenereated == false)
                        {
                            ModApi.Log.LogMessage("Adding weapon: " + weapon.Key.weaponName);
                            __instance.text = weapon.Key.weaponName;
                            weapon.Key.IsGenereated = true;

                            weaponselect wpS = __instance.transform.parent.GetComponent<weaponselect>();
                            wpS.weaponname = weapon.Key.weaponReference;
                            wpS.enabled = true;
                            __instance.transform.parent.GetComponent<Button>().enabled = true;

                            break;
                        }
                    }
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
                ModApi.Log.LogMessage("Weapon selected: " + weaponname);
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
                    if(obj.gameObject.name.Contains(weaponname))
                    {
                       if(toDestroy.Where(x => x.name.Contains(refname)).Count() == 1)
                       {
                            continue;
                       }   
                    }

                    ModApi.Log.LogMessage("Destroying: " + obj.name);
                    seen.Remove(obj); // Remove from the list
                    GameObject.Destroy(obj); // Destroy the GameObject
                }
            }
        }
    }
}