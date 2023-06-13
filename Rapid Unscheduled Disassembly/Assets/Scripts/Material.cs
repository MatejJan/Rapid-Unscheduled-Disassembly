using System;

namespace RapidUnscheduledDisassembly
{
    [Serializable]
    public class Material
    {
        public float mass;
        public float molarMass;
        public float pressure;
        public float temperature;
        public float specificHeatRatio;

        public float amountOfSubstance => mass / molarMass;
    }
}
