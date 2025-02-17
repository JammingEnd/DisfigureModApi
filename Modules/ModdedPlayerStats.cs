using UnityEngine;

namespace DisfigureModApi.Modules
{
    public class ModdedPlayerStats : MonoBehaviour
    {
        public List<ModdedStatWrapper> moddedStats = new List<ModdedStatWrapper>();
    }

    public class ModdedStatWrapper
    {
        string statName;
        
        object _statValue;
        float _floatValueMultiplier = 1;

        public ModdedStatWrapper(string statName, int value)
        {
            this.statName = statName;
            this._statValue = value;
        }

        public ModdedStatWrapper(string statName, float value)
        {
            this.statName = statName;
            this._statValue = value;
        }

        public ModdedStatWrapper(string statName, bool value)
        {
            this.statName = statName;
            this._statValue = value;
        }

        public object GetStatValue()
        {
            return _statValue;
        }

        public string GetStatName()
        {
            return statName;
        }

        public void SetStatValue(bool value) { _statValue = value; }
        public void SetStatValue(int value) { _statValue = (int)_statValue + value; }
        public void SetStatValue(float value) { 
            float oldvalue = _floatValueMultiplier;
            _floatValueMultiplier += value;
            _statValue = ((float)_statValue / oldvalue) * _floatValueMultiplier;

        }


        public KeyValuePair<string, object> GetStat()
        {
            return new KeyValuePair<string, object>(statName, _statValue);
        }   
    }
}