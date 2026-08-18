using DisfigureModApi.UpgradeCreationTools;
using DisfigureModApi;
using DisfigureModApi.WeaponCreationTools;
using UnityEngine;

namespace DisfigureModApi.Modules
{
    public class NewWeaponUpgradeRegistry
    {
        public static List<NewWeaponUpgrade> NewWeaponUpgrades = new List<NewWeaponUpgrade>();
        public static Dictionary<string, bool> NewTreeDef = new Dictionary<string, bool>();

        public static void RegisterNewWeaponUpgrade(NewWeaponUpgrade newWeaponUpgrade)
        {
            NewWeaponUpgrades.Add(newWeaponUpgrade);
        }

        public static void RegisterNewWeaponUpgradeTree(string refname, bool keepPistolUpgrades = false)
        {
            NewTreeDef[refname] = keepPistolUpgrades;
        }

        /// <summary>
        /// Whether a modded weapon's tree should inherit the pistol's perk children
        /// (false = clean, only that weapon's own registered perks).
        /// </summary>
        public static bool KeepsPistolUpgrades(string weaponRef)
        {
            return NewTreeDef.TryGetValue(weaponRef, out bool keep) && keep;
        }
    }

    public class NewWeaponUpgrade : weaponupgrade
    {
        public string ownerWeaponReference { get; private set; }

        public NewWeaponUpgrade(string name, DesclinesWrapper description, string ownerWeaponRefernce)
        {
            // desclines is a [SerializeField] field: null when created via new in IL2CPP.
            this.desclines = new string[2];
            this.upgradeName = name;
            this.desclines[0] = description.UpperLine;
            this.desclines[1] = description.LowerLine;
            this.ownerWeaponReference = ownerWeaponRefernce;
        }
    }

    public static class NewWeaponUpgradeUtils
    {
        private const int MaxUpgradeSlots = 8;

        /// <summary>
        /// Finds the modded weapon's upgrade-tree GameObject already added to the screen's
        /// <c>weaponUpgradesList</c> (named <c>"&lt;ref&gt;WeaponUpgrades"</c>), or null.
        /// </summary>
        public static GameObject GetTreeForWeapon(weaponupgradescreen instance, string weaponRef)
        {
            if (instance == null || instance.weaponUpgradesList == null)
            {
                return null;
            }
            string targetName = weaponRef + "WeaponUpgrades";
            foreach (GameObject tree in instance.weaponUpgradesList)
            {
                if (tree != null && tree.name == targetName)
                {
                    return tree;
                }
            }
            return null;
        }

        /// <summary>
        /// Creates (or reuses) the upgrade tree for the active modded weapon and adds it to
        /// the screen's <c>weaponUpgradesList</c>. Clones the pistol tree as the layout
        /// template; when <paramref name="keepPistolUpgrades"/> is false, blanks every slot
        /// and fills them with the weapon's registered perks only.
        /// </summary>
        public static GameObject AddNewWeaponUpgradeTreesToPlayer(this weaponupgradescreen instance, bool keepPistolUpgrades)
        {
            NewWeapon active = WeaponUtils.GetActiveWeapon();
            if (instance == null || instance.weaponUpgradesList == null || instance.weaponUpgradesList.Count == 0)
            {
                ModApi.Log.LogWarning("AddNewWeaponUpgradeTreesToPlayer: upgrade screen not ready.");
                return null;
            }
            if (active == null)
            {
                ModApi.Log.LogWarning("AddNewWeaponUpgradeTreesToPlayer: no active modded weapon.");
                return null;
            }

            string weaponRef = active.weaponReference;

            GameObject existing = GetTreeForWeapon(instance, weaponRef);
            if (existing != null)
            {
                return existing;
            }

            GameObject template = instance.weaponUpgradesList[0];
            GameObject newTreeInstance = GameObject.Instantiate(template, template.transform.parent);
            newTreeInstance.name = weaponRef + "WeaponUpgrades";

            if (keepPistolUpgrades)
            {
                ModApi.Log.LogMessage("Keeping pistol upgrades for modded weapon " + weaponRef);
            }
            else
            {
                ResetTreeToModded(newTreeInstance);
                PopulateTree(newTreeInstance, weaponRef);
            }

            instance.weaponUpgradesList.Add(newTreeInstance);
            ModApi.Log.LogMessage("Added " + newTreeInstance.name + " to player weapon upgrades");
            return newTreeInstance;
        }

        /// <summary>
        /// Blanks every perk slot of a freshly cloned tree so no vanilla (pistol) perk
        /// content leaks into a modded weapon's tree.
        /// </summary>
        private static void ResetTreeToModded(GameObject tree)
        {
            for (int i = 0; i < tree.transform.childCount && i < MaxUpgradeSlots; i++)
            {
                if (!tree.transform.GetChild(i).TryGetComponent(out weaponupgrade wU))
                {
                    continue;
                }

                wU.upgradeName = "";
                wU.statdescription = "";
                wU.desclines = new string[wU.desclines == null ? 2 : wU.desclines.Length];
                wU.statName = "";
                wU.statName2 = "";
                wU.statName3 = "";
                wU.statName4 = "";
                wU.statName5 = "";
                wU.change = 0f;
                wU.change2 = 0f;
                wU.change3 = 0f;
                wU.change4 = 0f;
                wU.change5 = 0f;
            }
        }

        /// <summary>
        /// Overwrites the tree's perk slots with the registered perks owned by
        /// <paramref name="weaponRef"/>, in registration order (capped at 8 slots).
        /// </summary>
        private static void PopulateTree(GameObject tree, string weaponRef)
        {
            int slot = 0;
            foreach (var weaponUpgrade in NewWeaponUpgradeRegistry.NewWeaponUpgrades)
            {
                if (slot >= MaxUpgradeSlots)
                {
                    break;
                }
                if (weaponUpgrade.ownerWeaponReference != weaponRef)
                {
                    continue;
                }
                if (tree.transform.childCount <= slot)
                {
                    break;
                }
                if (!tree.transform.GetChild(slot).TryGetComponent(out weaponupgrade wU))
                {
                    slot++;
                    continue;
                }

                wU.upgradeName = weaponUpgrade.upgradeName;
                wU.desclines = weaponUpgrade.desclines;
                wU.statName = weaponUpgrade.statName;
                wU.statName2 = weaponUpgrade.statName2;
                wU.statName3 = weaponUpgrade.statName3;
                wU.statName4 = weaponUpgrade.statName4;
                wU.statName5 = weaponUpgrade.statName5;

                wU.change = weaponUpgrade.change;
                wU.change2 = weaponUpgrade.change2;
                wU.change3 = weaponUpgrade.change3;
                wU.change4 = weaponUpgrade.change4;
                wU.change5 = weaponUpgrade.change5;

                slot++;
            }
        }
    }
}