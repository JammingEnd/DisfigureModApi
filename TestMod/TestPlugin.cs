using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using DisfigureModApi.Modules;
using DisfigureModApi.UpgradeCreationTools;
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

        RegisterTestPerks();
    }

    private static void RegisterTestPerks()
    {
        NewWeaponUpgradeRegistry.RegisterNewWeaponUpgrade(new NewWeaponUpgrade(
            "Test Damage",
            new DesclinesWrapper { UpperLine = "+25 damage", LowerLine = "TestWeapon perk" },
            "TestWeapon")
        {
            statName = "damage",
            change = 25f
        });

        NewWeaponUpgradeRegistry.RegisterNewWeaponUpgrade(new NewWeaponUpgrade(
            "Test Fire Rate",
            new DesclinesWrapper { UpperLine = "+20% fire rate", LowerLine = "TestWeapon perk" },
            "TestWeapon")
        {
            statName = "firerate",
            change = 0.2f
        });

        NewWeaponUpgradeRegistry.RegisterNewWeaponUpgrade(new NewWeaponUpgrade(
            "Test Clip Size",
            new DesclinesWrapper { UpperLine = "+5 clip size", LowerLine = "TestWeapon perk" },
            "TestWeapon")
        {
            statName = "clipsize",
            change = 5f
        });

        NewWeaponUpgradeRegistry.RegisterNewWeaponUpgrade(new NewWeaponUpgrade(
            "Test Reload",
            new DesclinesWrapper { UpperLine = "+25% reload speed", LowerLine = "TestWeapon perk" },
            "TestWeapon")
        {
            statName = "reloadspeed",
            change = 0.25f
        });

        NewWeaponUpgradeRegistry.RegisterNewWeaponUpgrade(new NewWeaponUpgrade(
            "Test Accuracy",
            new DesclinesWrapper { UpperLine = "+15% accuracy", LowerLine = "TestWeapon perk" },
            "TestWeapon")
        {
            statName = "accuracy",
            change = 0.15f
        });

        NewWeaponUpgradeRegistry.RegisterNewWeaponUpgrade(new NewWeaponUpgrade(
            "Test Spread",
            new DesclinesWrapper { UpperLine = "-10% spread", LowerLine = "TestWeapon perk" },
            "TestWeapon")
        {
            statName = "spread",
            change = -0.1f
        });

        NewWeaponUpgradeRegistry.RegisterNewWeaponUpgrade(new NewWeaponUpgrade(
            "Test Bullet Speed",
            new DesclinesWrapper { UpperLine = "+30% bullet speed", LowerLine = "TestWeapon perk" },
            "TestWeapon")
        {
            statName = "bulletspeed",
            change = 0.3f
        });

        NewWeaponUpgradeRegistry.RegisterNewWeaponUpgrade(new NewWeaponUpgrade(
            "Test Bullet Size",
            new DesclinesWrapper { UpperLine = "+20% bullet size", LowerLine = "TestWeapon perk" },
            "TestWeapon")
        {
            statName = "bulletsize",
            change = 0.2f
        });

        NewWeaponUpgradeRegistry.RegisterNewWeaponUpgrade(new NewWeaponUpgrade(
            "Test Piercing",
            new DesclinesWrapper { UpperLine = "+1 piercing", LowerLine = "TestWeapon perk" },
            "TestWeapon")
        {
            statName = "piercing",
            change = 1f
        });
    }
}
