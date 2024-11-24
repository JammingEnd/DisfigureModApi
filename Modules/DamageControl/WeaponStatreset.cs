using DisfigurwModApi.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace DisfigureModApi.Modules.DamageControl
{
    public static class WeaponStatreset
    {
        public static void ResetPistolStats(this weaponselect weaponselect)
        {
            weaponselect.transform.GetChild(7).GetComponent<Text>().text = "Pistol";
            weaponselect.transform.GetChild(3).GetComponent<Text>().text = "A basic pistol";

            weaponstatsliders slider = weaponselect.transform.GetChild(10).GetComponent<weaponstatsliders>();
            slider.projectile.damage = 10;
            slider.projectile.fireRate = 0.3f;
            slider.projectile.projSpeed = 50;
            slider.projectile.changeSize(5);

        }
    }
   
}
