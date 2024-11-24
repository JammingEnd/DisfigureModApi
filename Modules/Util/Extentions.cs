using DisfigurwModApi.UImanipulation;
using DisfigurwModApi.Weapons;
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
                GameObject.Destroy(instance.gameObject.transform.GetChild(0).gameObject);
            }
            return displayedObj;
        }


    }
}
