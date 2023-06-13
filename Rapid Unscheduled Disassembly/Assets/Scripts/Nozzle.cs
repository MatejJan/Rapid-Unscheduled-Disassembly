using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class Nozzle : MonoBehaviour
    {
        public const float particleRateFactor = 5e5f;
        public const float particleSpeedFactor = 1e-2f;

        public float throatDiameter;
        public float exitDiameter;

        public float exitMachNumber;

        public Tank parent;

        public float exitSpeed;
        public float ventedMass;

        private ParticleSystem _particleSystem;

        public float throatArea => Mathf.PI * Mathf.Pow(throatDiameter / 2f, 2);
        public float exitArea => Mathf.PI * Mathf.Pow(exitDiameter / 2f, 2);

        private void Start()
        {
            parent = transform.parent.parent.GetComponent<Tank>();

            _particleSystem = GetComponent<ParticleSystem>();

            CalculateExitMachNumber();
        }

        private void Update()
        {
            ParticleSystem.MainModule main = _particleSystem.main;
            main.startSpeed = exitSpeed * particleSpeedFactor;

            ParticleSystem.EmissionModule emission = _particleSystem.emission;
            emission.rateOverTime = ventedMass * particleRateFactor;
        }

        private void FixedUpdate()
        {
            float specificHeatRatio = parent.contents.specificHeatRatio;

            // Calculate exit pressure and temperature.
            // pe / pt = [1 + Me^2 * (gam-1)/2]^-[gam/(gam-1)]
            // Te / Tt = [1 + Me^2 * (gam-1)/2]^-1
            float exitPressure = parent.contents.pressure * Mathf.Pow(1 + Mathf.Pow(exitMachNumber, 2) * (specificHeatRatio - 1) / 2, -specificHeatRatio / (specificHeatRatio - 1));
            float exitTemperature = parent.contents.temperature * Mathf.Pow(1 + Mathf.Pow(exitMachNumber, 2) * (specificHeatRatio - 1) / 2, -1);

            float R = Physicsf.molarGasConstant / parent.contents.molarMass;

            // Calculate exit speed.
            // Me * sqrt (gam * R * Te)
            exitSpeed = exitMachNumber * Mathf.Sqrt(specificHeatRatio * R * exitTemperature);

            // Calculate mass flow rate.
            // mdot = (A* * pt/sqrt[Tt]) * sqrt(gam/R) * [(gam + 1)/2]^-[(gam + 1)/(gam - 1)/2]
            float massFlowRate = throatArea * parent.contents.pressure / Mathf.Sqrt(parent.contents.temperature) * Mathf.Sqrt(specificHeatRatio / R) * Mathf.Pow((specificHeatRatio + 1) / 2, -(specificHeatRatio + 1) / (specificHeatRatio - 1) / 2);

            // Vent the material.
            ventedMass = Mathf.Min(parent.contents.mass, massFlowRate * Time.fixedDeltaTime);
            parent.contents.mass -= ventedMass;

            // Calculate thrust.
            // F = m dot * Ve + (pe - p0) * Ae
            float forceAmount = massFlowRate * exitSpeed + (exitPressure - Physicsf.atmosphericPressure) * exitArea;

            int n = 1;

            for (int i = 0; i < n; i++)
            {
                Vector3 direction = (-transform.forward * 5 + Random.insideUnitSphere).normalized;

                parent.AddForce(forceAmount / n * direction);
            }
        }

        private void CalculateExitMachNumber()
        {
            float arat = exitArea / throatArea;
            float gamma = parent.contents.specificHeatRatio;
            float f1 = (gamma + 1) / (2 * (gamma - 1));
            float a0 = 2;
            float m0 = 2.2f;
            float m1 = m0 + 0.05f;

            while (Mathf.Abs(arat - a0) > 0.0001f)
            {
                float fac = 1 + 0.5f * (gamma - 1) * m1 * m1;
                float a1 = 1 / (m1 * Mathf.Pow(fac, -1 * f1) * Mathf.Pow((gamma + 1) / 2, f1));
                float am = (a1 - a0) / (m1 - m0);
                a0 = a1;
                m0 = m1;
                m1 = m0 + (arat - a0) / am;
            }

            exitMachNumber = m0;
        }
    }
}
