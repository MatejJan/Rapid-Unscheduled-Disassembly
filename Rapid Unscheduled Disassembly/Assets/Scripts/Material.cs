using System;

namespace RapidUnscheduledDisassembly
{
    [Serializable]
    public class Material
    {
        public enum State
        {
            Gas,
            Liquid
        }

        public MaterialType type;
        public float mass;
        public float temperature;
        public float availableVolume;

        public float volume;
        public float pressure;

        public State state;
        public float boilingPoint;
        public float amountOfSubstance;

        public void Update()
        {
            // Calculate amount of substance.
            amountOfSubstance = mass / type.molarMass;

            // Calculate state.
            boilingPoint = type.boilingPoint.GetTemperature(pressure);
            state = temperature < boilingPoint ? State.Liquid : State.Gas;

            // Calculate volume and pressure.
            switch (state)
            {
                case State.Gas:
                    volume = availableVolume;
                    pressure = amountOfSubstance * Physicsf.molarGasConstant * temperature / volume;

                    break;

                case State.Liquid:
                    volume = mass / type.liquidDensity;
                    pressure = 0;

                    break;
            }
        }
    }
}
