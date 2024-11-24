using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisfigureTestMod.Weapons
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
