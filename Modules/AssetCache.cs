using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DisfigureModApi.Modules
{
    /// <summary>
    /// I want to save stuff in the DontDestroyOnLoad object so it doesn't get deleted on scene change
    /// </summary>
    public class AssetCache : MonoBehaviour
    { 
        public bool UpgradesLoaded = false;
        public Dictionary<GameObject, List<GameObject>> CachedUpgrades { get; private set; } = new();
        public void InsetUpgrades(GameObject ini, List<GameObject> unlocks)
        {
            CachedUpgrades[ini] = unlocks;
        }
    }
}
