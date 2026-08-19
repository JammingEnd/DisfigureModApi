using UnityEngine;

namespace DisfigureModApi.Modules
{
    public class ModdedPlayerStats : MonoBehaviour
    {
        public bool initialized = false;
        public Dictionary<string, ModdedStatWrapper> moddedStats = new();
    }

    public class ModdedStatWrapper
    {
        object _statValue;
        float _floatValueMultiplier = 1;

        public ModdedStatWrapper(int value)
        {
            this._statValue = value;
        }

        public ModdedStatWrapper(float value)
        {
            this._statValue = value;
        }

        public ModdedStatWrapper(bool value)
        {
            this._statValue = value;
        }

        public bool GetStatValueBool()
        {
            if (_statValue is not bool value)
                return false;
            return value;
        }
        public int GetStatValueInt()
        {
            if (_statValue is not int value)
                return 0;
            return value;
        }
        public float GetStatValueFloat()
        {
            if (_statValue is not float value)
                return 0f;
            return value * _floatValueMultiplier;
        }

        public void SetStatValue(bool value) { _statValue = value; }
        public void SetStatValue(int value) { _statValue = (int)_statValue + value; }
        public void SetStatValue(float value) { _statValue = (float)_statValue + value; }
        public void AddStatValueModifier(float value) { 
            _floatValueMultiplier += value;
        }


        public object GetStat()
        {
            return _statValue;
        }   
        public float GetMultiplier()
        {
            return _floatValueMultiplier;
        }
    }
}