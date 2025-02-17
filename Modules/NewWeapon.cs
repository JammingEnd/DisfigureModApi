using DisfigurwModApi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DisfigurwModApi.WeaponCreationTools
{
    /// <summary>
    /// Base weapon class
    /// </summary>
    public class NewWeapon
    {
        /// <summary>
        /// The display name of the weapon
        /// </summary>
        public string weaponName;

        /// <summary>
        ///  The reference name of the weapon (used for identification)
        /// </summary>
        public string weaponReference;

        /// <summary>
        /// this boolean is used for checking if the weapon assigned to a button
        /// </summary>
        public bool IsGenereated = false;

        /// <summary>
        /// What will the weapon, the player is holding, look like
        /// </summary>
        public WeaponId HeldWeapon;
        public GameObject weaponPrefab;
        public virtual void BuildWeapon(displayimagehandler instance, string name)
        {

        }

       
    }
    public class WeaponUtils
    {
        public static bool isActiveWeapon(string weaponRef)
        {
            bool canperform = false;
            foreach (var weapon in NewWeaponInitiator.newWeapons)
            {
                if (weapon.Value && weapon.Key.weaponReference == weaponRef)
                {
                    canperform = true;
                }
            }
            return canperform;
        }

        /// <summary>
        /// Return the active weapon. !!! only use this in methods that operate when youre in the main game, outside it, it will return null
        /// </summary>
        /// <returns> The active weapon </returns>
        public static NewWeapon GetActiveWeapon()
        {
            foreach (var weapon in NewWeaponInitiator.newWeapons)
            {
                if (weapon.Value)
                {
                    return weapon.Key;
                }
            }
            return null;
        }

        public static GameObject SetHeldWeapon(ObjectPool pool, WeaponId id)
        {
            return pool.weaponsModelList[(int)id];
        }
    }

    public class WeaponPreviewStats
    {
        public string WeaponName;
        public string WeaponDescription;
        public float WeaponDamage;
        public float WeaponFireRate;
        public float BulletSpeed;
        public float BulletSize;

    }

    public class  NewWeaponInitiator
    {
        /// <summary>
        /// A list of all the new weapons, the bool is used for checking if the weapon is selected
        /// </summary>
        public static Dictionary<NewWeapon, bool> newWeapons = new();

        /// <summary>
        /// registers a new weapon
        /// </summary>
        /// <param name="weapon"></param>
        public static void AddWeapon(NewWeapon weapon)
        {
            newWeapons.Add(weapon, false);
        }

        public static WeaponId CurrentWeapon { get; set;}
    }
}
