using DisfigurwModApi.UImanipulation;
using DisfigurwModApi.WeaponCreationTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DisfigurwModApi.Util
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
        /// it Builds the preview of the weapon but also literally changes the weapon stats
        /// </summary>
        /// <param name="baseWeapon"> What is being edited </param>
        /// <param name="newName"> The new name shown under the image </param>
        /// <param name="newDesc"> The new description </param>
        /// <param name="damageSliderStat"> The Initial damage it will deal </param>
        /// <param name="fireRateStat"> The new fire rate (idk how the logic works yet) </param>
        /// <param name="bulletSpeedStat"> The new bullet speed (1 = 10 units per second, pistol damage is 50) </param>
        /// <param name="bulletSizeStat"> The starting bullet size </param>
        /// <param name="newSprite"> The new image(s) being displayed </param>
        /// <param name="isMelee"> Is it an melee weapon? default : False </param>
        /// <returns> An edit / new weapon. !!! these are used in the actual game too </returns>
        public static GameObject SetWeaponPreviewStats(this displayimagehandler instance, string name, string desc, float damage, float fireRate, float bulletSpeed, float BulletSize, WeaponId previewSprite, bool isMelee = false)
        {
            GameObject displayedObj = GameObject.Instantiate(instance.weaponDisplays[0], instance.gameObject.transform);
            displayedObj.transform.position = new Vector3(25.66f, -65, -15.5645f);
            displayedObj.BuildPreview(new WeaponPreviewStats() {
                WeaponName = name,
                WeaponDescription = desc,
                WeaponDamage = damage,
                WeaponFireRate = fireRate,
                BulletSpeed = bulletSpeed,
                BulletSize = BulletSize
            }, 
             previewSprite, 
             instance.weaponDisplays.ToList(),
             isMelee);
            if (instance.gameObject.transform.childCount > 1)
            {
                ModApi.Log.Equals("Destroying child");
                GameObject.Destroy(instance.gameObject.transform.GetChild(0).gameObject);
            }
            return displayedObj;
        }

        public static Transform GetChildTransformByName(this Transform transform, string name)
        {
            foreach (Transform child in transform)
            {
                if (child.name == name)
                {
                    return child;
                }
            }
            return null;
        }

        public static int FromPreviewIdToModelId(this WeaponId id)
        {
            switch (id)
            {
                case WeaponId.Pistol:
                    return 10; // Original id is 0 but the pistol DOESNT FKIN WORK
                case WeaponId.Shotgun:
                    return 1;
                case WeaponId.Sniper:
                    return 2;                 
                case WeaponId.Knife:
                    return 3;
                case WeaponId.DoubleKatana:
                    return 4;
                case WeaponId.GreatSword:
                    return 5;
                case WeaponId.Scythe:
                    return 6;
                case WeaponId.LeverAction:
                    return 7;
                case WeaponId.Famas:
                    return 8;
                case WeaponId.Minigun:
                    return 9;
                case WeaponId.Revolver:
                    return 10;
                case WeaponId.Saw:
                    return 11;
                case WeaponId.Railgun:
                    return 12;
                case WeaponId.AkimboSmg:
                    return 13;
                case WeaponId.Halberd:
                    return 14;
                case WeaponId.PulseRifle:
                    return 15;
                default:
                    return 0;
            }
        }


        public static bool IsAvaibleButton(this GameObject obj)
        {
            string name = obj.name;
            if(name.Contains("27"))
                return true;
            if (name.Contains("28"))
                return true;
            if (name.Contains("31"))
                return true;
            if (name.Contains("32"))
                return true;

            return false;
        }
    }
}
