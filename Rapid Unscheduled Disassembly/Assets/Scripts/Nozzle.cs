using System.Collections.Generic;
using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class Nozzle : Structure, IContainer
    {
        public float throatDiameter;
        public float exitDiameter;

        private readonly Dictionary<float, float> _exitMachNumbers = new();

        private Vent _vent;

        public float throatArea => Mathf.PI * Mathf.Pow(throatDiameter / 2f, 2);
        public float exitArea => Mathf.PI * Mathf.Pow(exitDiameter / 2f, 2);

        private void Start()
        {
            _vent = transform.Find("Vent").GetComponent<Vent>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            foreach (Material material in contents.materials)
            {
                if (material.mass == 0) continue;

                float specificHeatRatio = material.type.specificHeatRatio;
                float exitMachNumber = GetExitMachNumber(specificHeatRatio);

                // Calculate exit pressure and temperature.
                // pe / pt = [1 + Me^2 * (gam-1)/2]^-[gam/(gam-1)]
                // Te / Tt = [1 + Me^2 * (gam-1)/2]^-1
                float exitPressure = material.pressure * Mathf.Pow(1 + Mathf.Pow(exitMachNumber, 2) * (specificHeatRatio - 1) / 2, -specificHeatRatio / (specificHeatRatio - 1));
                float exitTemperature = material.temperature * Mathf.Pow(1 + Mathf.Pow(exitMachNumber, 2) * (specificHeatRatio - 1) / 2, -1);

                float R = Physicsf.molarGasConstant / material.type.molarMass;

                // Calculate exit speed.
                // Me * sqrt (gam * R * Te)
                float exitSpeed = exitMachNumber * Mathf.Sqrt(specificHeatRatio * R * exitTemperature);

                // Calculate mass flow rate.
                // mdot = (A* * pt/sqrt[Tt]) * sqrt(gam/R) * [(gam + 1)/2]^-[(gam + 1)/(gam - 1)/2]
                float massFlowRate = throatArea * material.pressure / Mathf.Sqrt(material.temperature) * Mathf.Sqrt(specificHeatRatio / R) * Mathf.Pow((specificHeatRatio + 1) / 2, -(specificHeatRatio + 1) / (specificHeatRatio - 1) / 2);

                // Vent the material.
                contents.RemoveMaterial(material.type, massFlowRate * Time.fixedDeltaTime);
                _vent.SetVentParameters(material, exitSpeed, massFlowRate);

                // Calculate thrust.
                // F = m dot * Ve + (pe - p0) * Ae
                float forceAmount = massFlowRate * exitSpeed + (exitPressure - Physicsf.atmosphericPressure) * exitArea;
                Vector3 direction = (transform.up * 10 + Random.insideUnitSphere).normalized;
                AddForce(forceAmount * direction, transform.position);
            }

            contents.Update();
        }

        public Contents contents { get; } = new();

        public float GetPressureAtPoint(Vector3 point, out float liquidPressure)
        {
            liquidPressure = 0;

            return 0;
        }

        private float GetExitMachNumber(float specificHeatRatio)
        {
            if (_exitMachNumbers.ContainsKey(specificHeatRatio)) return _exitMachNumbers[specificHeatRatio];

            float arat = exitArea / throatArea;
            float gamma = specificHeatRatio;
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

            _exitMachNumbers[specificHeatRatio] = m0;

            return m0;
        }
    }
}
