using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using DisfigureModApi.Modules;
using DisfigureModApi.UpgradeCreationTools;
using DisfigurwModApi.UImanipulation;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace DisfigurwModApi
{
    [BepInPlugin("com.disfigure.modapi", "DisfigureModApi", "1.0.0")]
    public class ModApi : BasePlugin
    {
        internal static ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;
            Log.LogMessage("ModApi loading.....");

            Patcher.Ini();
            UpgradeRegistry.Ini();
            UIinteractor.IniUIInteractor();
            ClassInjector.RegisterTypeInIl2Cpp<ModdedPlayerStats>();
        }
    }
}