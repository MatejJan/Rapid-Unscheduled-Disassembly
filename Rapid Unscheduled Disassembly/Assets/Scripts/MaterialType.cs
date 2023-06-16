using System;
using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    [CreateAssetMenu]
    public class MaterialType : ScriptableObject
    {
        public float molarMass;
        public float specificHeatRatio;
        public StatePoint boilingPoint;
        public float liquidDensity;

        public float particleRateFactor = 1;
        public float particleSpeedFactor = 1;
        public Color particleColor = Color.white;
        public UnityEngine.Material particleMaterial;

        [Serializable]
        public class StatePoint
        {
            public float lowPressure;
            public float lowTemperature;
            public float highPressure;
            public float highTemperature;

            public float GetPressure(float temperature)
            {
                float dP = highPressure - lowPressure;
                float dT = highTemperature - lowTemperature;
                float k = dP / dT;
                float n = highPressure - k * highTemperature;

                return k * temperature + n;
            }

            public float GetTemperature(float pressure)
            {
                float dP = highPressure - lowPressure;
                float dT = highTemperature - lowTemperature;
                float k = dT / dP;
                float n = highTemperature - k * highPressure;

                return k * pressure + n;
            }
        }
    }
}
