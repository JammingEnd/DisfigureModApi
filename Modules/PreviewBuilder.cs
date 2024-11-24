using DisfigureTestMod.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DisfigureTestMod.UImanipulation
{
    public static class PreviewBuilder
    {
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
        public static GameObject BuildPreview(this GameObject baseWeapon, string newName, string newDesc, float damageSliderStat, float fireRateStat, float bulletSpeedStat, float bulletSizeStat, WeaponId newSprite, List<GameObject> currentAvaibleClones, bool isMelee = false)
        {
            // This is where the preview will be built
            GameObject baseWeaponObject = baseWeapon;
            bulletSpeedStat *= 10;
            if (!isMelee)
            {
                baseWeaponObject.transform.GetChild(7).GetComponent<Text>().text = newName;
                baseWeaponObject.transform.GetChild(3).GetComponent<Text>().text = newDesc;
                weaponstatsliders slider = baseWeaponObject.transform.GetChild(10).GetComponent<weaponstatsliders>();
                slider.projectile.damage = damageSliderStat;
                slider.projectile.fireRate = fireRateStat;
                slider.projectile.projSpeed = bulletSpeedStat;
                slider.projectile.ogSize *= bulletSizeStat;
                slider.bulletDamageStat = damageSliderStat;
                slider.fireRateStat = fireRateStat;
                slider.bulletSpeedStat = bulletSpeedStat;
                slider.bulletSizeStat = bulletSizeStat;

                Image retrievedImg = currentAvaibleClones[(int)newSprite].transform.GetChild(6).GetComponent<Image>();
                ModApi.Log.LogMessage("Retrieved image: " + retrievedImg.sprite.name);

                Image imageBright = baseWeaponObject.transform.GetChild(6).GetComponent<Image>();
                imageBright.sprite = retrievedImg.sprite;
                imageBright.rectTransform.sizeDelta = retrievedImg.rectTransform.sizeDelta;
                imageBright.rectTransform.pivot = retrievedImg.rectTransform.pivot;

                Image imageDark = baseWeaponObject.transform.GetChild(5).GetComponent<Image>();
                imageDark.sprite = retrievedImg.sprite;
                imageDark.rectTransform.sizeDelta = retrievedImg.rectTransform.sizeDelta;
                imageDark.rectTransform.pivot = retrievedImg.rectTransform.pivot;

            }
            baseWeaponObject.name = newName + "(clone of " + baseWeapon.name + ")";
            return baseWeaponObject;
        }

        public static Image GetSprite(string weaponName)
        {
            return Resources.Load<GameObject>("Asset/Resource/" + weaponName).transform.GetChild(6).GetComponent<Image>();
        }   
    }
}
