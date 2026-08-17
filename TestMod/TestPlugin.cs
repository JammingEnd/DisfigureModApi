using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using DisfigureModApi.Util;
using DisfigureModApi.WeaponCreationTools;

namespace TestMod;

[BepInPlugin("com.disfigure.testmod", "TestMod", "1.0.0")]
public class TestPlugin : BasePlugin
{
    internal static ManualLogSource Log;

    public override void Load()
    {
        Log = base.Log;
        Log.LogMessage("TestMod loading.....");

        RegisterTestContent();
    }

    private static void RegisterTestContent()
    {
        NewWeapon testWeapon = new NewWeapon
        {
            weaponName = "Test Weapon",
            weaponReference = "TestWeapon",
            modelIndex = 1, // held model = ObjectPool.weaponsModelList[1]
            previewIndex = WeaponId.Shotgun, // preview sprite = displayimagehandler.weaponDisplays[1]
            isMelee = false,
            previewStats = new WeaponPreviewStats
            {
                WeaponName = "Test Weapon",
                WeaponDescription = "A test weapon registered through the mod API.",
                WeaponDamage = 50f,
                WeaponFireRate = 1f,
                BulletSpeed = 10f,
                BulletSize = 1f
            }
        };

        testWeapon.Unlock(); // FBPP: SetBool("TestWeaponUnlocked", true)
        NewWeaponInitiator.AddWeapon(testWeapon);
        Log.LogMessage("Registered test weapon: " + testWeapon.weaponName);
    }
}
