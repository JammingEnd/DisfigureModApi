using DisfigureModApi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DisfigureModApi.WeaponCreationTools
{
    /// <summary>
    /// A modded weapon. Mirrors how the game itself identifies weapons:
    /// the identity is a <b>string reference</b> (the same value used for
    /// <see cref="weaponselect.weaponname"/> on the menu button, the FBPP unlock key
    /// <c>"&lt;reference&gt;Unlocked"</c>, and the value of the FBPP
    /// <c>"selectedWeapon"</c> key that carries the choice across scenes).
    /// </summary>
    public class NewWeapon
    {
        /// <summary>FBPP key that persists the active weapon across scenes.</summary>
        public const string SelectedWeaponKey = "selectedWeapon";

        /// <summary>
        /// Unique identity string, e.g. <c>"pistol"</c>. Used as the value of
        /// <see cref="weaponselect.weaponname"/>, the base of the unlock key
        /// (<c>weaponReference + "Unlocked"</c>), and the value written to
        /// <see cref="SelectedWeaponKey"/>.
        /// </summary>
        public string weaponReference;

        /// <summary>Display name shown on the weapon button.</summary>
        public string weaponName;

        /// <summary>
        /// Index into <see cref="ObjectPool.weaponsModelList"/> — the held model that
        /// appears in the player's hands. Matches <see cref="WeaponManager.currentWeapon"/>.
        /// </summary>
        public int modelIndex;

        /// <summary>Index into <see cref="displayimagehandler.weaponDisplays"/> — the preview sprite.</summary>
        public WeaponId previewIndex;

        /// <summary>Whether this weapon is melee (changes held-model setup / stats panel).</summary>
        public bool isMelee;

        /// <summary>Stats used to build the preview display on the home screen.</summary>
        public WeaponPreviewStats previewStats;

        /// <summary>
        /// Called by the API when the home screen shows this weapon's display
        /// (<see cref="displayimagehandler.showChosenWeapon(string)"/>). Override to build a
        /// custom preview; the default builds one from <see cref="previewStats"/>.
        /// </summary>
        public virtual void BuildWeapon(displayimagehandler instance)
        {
            instance.SetWeaponPreviewStats(previewStats, previewIndex, isMelee);
        }

        /// <summary>FBPP unlock key for this weapon: <c>"&lt;weaponReference&gt;Unlocked"</c>.</summary>
        public string UnlockKey => weaponReference + "Unlocked";

        /// <summary>
        /// Reads the unlock state from FBPP (falls back to <c>false</c> if FBPP is not
        /// initialized yet, e.g. during plugin load).
        /// </summary>
        public bool IsUnlocked
        {
            get
            {
                try { return FBPP.GetBool(UnlockKey, false); }
                catch (Exception) { return false; }
            }
        }

        /// <summary>Whether this weapon is the currently selected one (FBPP "selectedWeapon").</summary>
        public bool IsSelected
        {
            get
            {
                try { return FBPP.GetString(SelectedWeaponKey, "") == weaponReference; }
                catch (Exception) { return false; }
            }
        }

        /// <summary>Persist the unlock state (FBPP bool "<c>&lt;weaponReference&gt;Unlocked</c>").</summary>
        public void Unlock()
        {
            try { FBPP.SetBool(UnlockKey, true); }
            catch (Exception) { }
        }

        /// <summary>Persist this weapon as the active selection (FBPP string "selectedWeapon").</summary>
        public void Select()
        {
            try { FBPP.SetString(SelectedWeaponKey, weaponReference); }
            catch (Exception) { }
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

    public static class NewWeaponInitiator
    {
        /// <summary>All registered modded weapons.</summary>
        public static readonly List<NewWeapon> newWeapons = new();

        /// <summary>Registers a new weapon.</summary>
        public static void AddWeapon(NewWeapon weapon)
        {
            if (weapon == null || string.IsNullOrEmpty(weapon.weaponReference))
            {
                ModApi.Log.LogWarning("AddWeapon: weapon or weaponReference is null/empty, ignored.");
                return;
            }
            if (newWeapons.Any(w => w.weaponReference == weapon.weaponReference))
            {
                ModApi.Log.LogWarning($"AddWeapon: weapon reference '{weapon.weaponReference}' already registered, ignored.");
                return;
            }
            newWeapons.Add(weapon);
        }

        /// <summary>Finds a registered weapon by its reference string.</summary>
        public static NewWeapon GetWeapon(string weaponReference)
        {
            return newWeapons.FirstOrDefault(w => w.weaponReference == weaponReference);
        }

        /// <summary>
        /// Returns the weapon currently persisted in FBPP "selectedWeapon", or null if
        /// the saved selection is not one of the registered modded weapons.
        /// </summary>
        public static NewWeapon GetActiveWeapon()
        {
            string reference = "";
            try { reference = FBPP.GetString(NewWeapon.SelectedWeaponKey, ""); }
            catch (Exception) { return null; }
            return GetWeapon(reference);
        }
    }

    public static class WeaponUtils
    {
        public static bool isActiveWeapon(string weaponRef)
        {
            return NewWeaponInitiator.GetActiveWeapon()?.weaponReference == weaponRef;
        }

        /// <summary>Returns the currently selected modded weapon, or null (vanilla selection).</summary>
        public static NewWeapon GetActiveWeapon()
        {
            return NewWeaponInitiator.GetActiveWeapon();
        }

        /// <summary>
        /// Instantiates the held model for a weapon from
        /// <see cref="ObjectPool.weaponsModelList"/> and parents it under
        /// <see cref="WeaponManager.weaponModels"/>, then sets
        /// <see cref="ObjectPool.selectedWeaponModel"/> and
        /// <see cref="WeaponManager.currentWeapon"/>/<see cref="WeaponManager.weaponName"/>.
        /// </summary>
        public static GameObject SetHeldWeapon(ObjectPool pool, WeaponManager wM, NewWeapon weapon)
        {
            if (pool == null || wM == null || weapon == null)
            {
                ModApi.Log.LogWarning("SetHeldWeapon: pool/wM/weapon is null.");
                return null;
            }
            if (weapon.modelIndex < 0 || weapon.modelIndex >= pool.weaponsModelList.Count)
            {
                ModApi.Log.LogWarning($"SetHeldWeapon: modelIndex {weapon.modelIndex} out of range for weaponsModelList (count {pool.weaponsModelList.Count}).");
                return null;
            }

            GameObject held = GameObject.Instantiate(pool.weaponsModelList[weapon.modelIndex], Vector3.zero, Quaternion.Euler(Vector3.zero));
            held.name = weapon.weaponReference;
            held.transform.SetParent(wM.weaponModels.transform);
            held.SetActive(false);

            pool.selectedWeaponModel = held;
            wM.currentWeapon = weapon.modelIndex;
            wM.weaponName = weapon.weaponReference;
            return held;
        }
    }
}
