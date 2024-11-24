using DisfigurwModApi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DisfigurwModApi.Weapons
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
        public virtual void BuildWeapon(displayimagehandler instance, string weaponname)
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
    }
}
