namespace DisfigureModApi.Util
{
    /// <summary>
    /// Index into <see cref="displayimagehandler.weaponDisplays"/> — the preview sprite
    /// shown for a weapon on the home screen. NOTE: this is the <b>preview</b> index,
    /// not the held-model index. The held model index is
    /// <see cref="WeaponCreationTools.NewWeapon.modelIndex"/> (into
    /// <see cref="ObjectPool.weaponsModelList"/>).
    /// </summary>
    public enum WeaponId
    {
        Pistol = 0,
        Shotgun = 1,
        Sniper = 2,
        Knife = 3,
        DoubleKatana = 4,
        GreatSword = 5,
        Scythe = 7,
        LeverAction = 6,
        Famas = 8,
        Minigun = 9,
        Revolver = 10,
        Saw = 11,
        Railgun = 12,
        AkimboSmg = 13,
        Halberd = 14,
        PulseRifle = 15,
    }
}
