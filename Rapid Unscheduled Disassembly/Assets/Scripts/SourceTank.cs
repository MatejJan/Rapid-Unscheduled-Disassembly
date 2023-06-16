using System;
using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    [Serializable]
    public class SourceTank : MonoBehaviour, IContainer
    {
        public float pressure;

        private void Awake()
        {
            UpdateContents();
        }

        private void FixedUpdate()
        {
            UpdateContents();
        }

        [field: SerializeField] public Contents contents { get; private set; } = new();

        public float GetPressureAtPoint(Vector3 point, out float liquidPressure)
        {
            liquidPressure = 0;

            return pressure;
        }

        private void UpdateContents()
        {
            UpdateAvailableVolume();
            contents.Update();
        }

        private void UpdateAvailableVolume()
        {
            foreach (Material material in contents.materials)
            {
                material.mass = 1000;
            }

            foreach (Material material in contents.gasses)
            {
                material.availableVolume = material.mass / material.type.molarMass * Physicsf.molarGasConstant * material.temperature / pressure;
            }
        }
    }
}
