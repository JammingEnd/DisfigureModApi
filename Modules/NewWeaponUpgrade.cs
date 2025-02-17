using DisfigureModApi.UpgradeCreationTools;
using DisfigurwModApi;
using DisfigurwModApi.WeaponCreationTools;
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
            NewTreeDef.Add(refname, keepPistolUpgrades);
        }   

    }

    public class NewWeaponUpgrade : weaponupgrade
    {
        public string ownerWeaponReference { get; private set; }

        public NewWeaponUpgrade(string name, DesclinesWrapper description, string ownerWeaponRefernce)
        {
            this.upgradeName = name;
            this.desclines[0] = description.UpperLine;
            this.desclines[1] = description.LowerLine;
            this.ownerWeaponReference = ownerWeaponRefernce;
        }
    }

    public static class NewWeaponUpgradeUtils
    {
        public static GameObject AddNewWeaponUpgradeTreesToPlayer(this weaponupgradescreen instance, bool keepPistolUpgrades)
        {
            GameObject newTreeInstance = GameObject.Instantiate(instance.weaponUpgradesList[0], instance.weaponUpgradesList[0].transform.parent);
            newTreeInstance.name = WeaponUtils.GetActiveWeapon().weaponReference + "WeaponUpgrades";

            if (keepPistolUpgrades)
            {
                ModApi.Log.LogMessage("Keeping pistol upgrades for modded weapon");
                instance.weaponUpgradesList.Add(newTreeInstance);
                return newTreeInstance;
            }


            int currentIndex = 0;
            foreach (var weaponUpgrade in NewWeaponUpgradeRegistry.NewWeaponUpgrades)
            {
              
                if (currentIndex >= 8)
                {
                    break;
                }


                if (weaponUpgrade.ownerWeaponReference == WeaponUtils.GetActiveWeapon().weaponReference)
                {
                    if (newTreeInstance.transform.GetChild(currentIndex).TryGetComponent(out weaponupgrade wU))
                    {
                        wU.upgradeName = weaponUpgrade.name;
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

                    }
                }

                currentIndex++;
            }

            ModApi.Log.LogMessage("Added" + newTreeInstance.name + " to player weapon upgrades");
            instance.weaponUpgradesList.Add(newTreeInstance);   
            return newTreeInstance;
        }
    }
}