using DisfigureModApi.UImanipulation;
using DisfigureModApi.WeaponCreationTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DisfigureModApi.Util
{
    public static class Extentions
    {
        public static List<T> ToList<T>(this Il2CppSystem.Collections.Generic.List<T> values)
        {
            List<T> list = new();
            foreach (var item in values)
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// Builds a preview display on the home screen for a weapon. Mirrors the game's flow:
        /// the display is shown by <see cref="displayimagehandler.showChosenWeapon(string)"/>
        /// and stat sliders are driven by <see cref="weaponstatsliders"/>.
        /// </summary>
        /// <returns>The built display GameObject (already parented to the displayimagehandler).</returns>
        public static GameObject SetWeaponPreviewStats(this displayimagehandler instance, WeaponPreviewStats stats, WeaponId previewSprite, bool isMelee = false)
        {
            instance.ClearDisplayChildren();

            GameObject displayedObj = GameObject.Instantiate(instance.weaponDisplays[0], instance.gameObject.transform);
            displayedObj.transform.position = new Vector3(25.66f, -65, -15.5645f);
            displayedObj.BuildPreview(stats,
             previewSprite,
             instance.weaponDisplays.ToList(),
             isMelee);
            return displayedObj;
        }

        /// <summary>
        /// Removes all children of the display handler before building a modded preview, so
        /// a stale vanilla display (or a previous modded one) never overlaps with the new one.
        /// </summary>
        public static void ClearDisplayChildren(this displayimagehandler instance)
        {
            Transform root = instance.gameObject.transform;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(root.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Destroys any modded preview display built by the API (clones named with
        /// "(clone of ..."), leaving vanilla display children untouched.
        /// </summary>
        public static void DestroyModdedPreview(this displayimagehandler instance)
        {
            Transform root = instance.gameObject.transform;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                GameObject child = root.GetChild(i).gameObject;
                if (child.name.Contains("(clone of"))
                {
                    GameObject.DestroyImmediate(child);
                }
            }
        }

        public static Transform GetChildTransformByName(this Transform transform, string name)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name == name)
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the active child GameObjects of <paramref name="parent"/> whose name starts
        /// with "GunButton" (e.g. "GunButton (27)"). Disabled/legacy buttons are excluded.
        /// </summary>
        public static List<GameObject> FindGunButtons(this Transform parent)
        {
            List<GameObject> buttons = new();
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.gameObject.name.StartsWith("GunButton") && child.gameObject.activeInHierarchy)
                {
                    buttons.Add(child.gameObject);
                }
            }
            return buttons;
        }
        
    }
}
