using System;
using System.Linq;
using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    [Serializable]
    public class Tank : Structure, IContainer
    {
        public float volume;
        public float maximumPressure;

        private float _emptyMass;

        protected override void Awake()
        {
            base.Awake();

            _emptyMass = rigidbody.mass;

            SphereCollider sphereCollider = GetComponent<SphereCollider>();

            if (sphereCollider != null)
            {
                volume = 4f / 3f * Mathf.PI * Mathf.Pow(sphereCollider.radius, 3);
            }

            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

            if (capsuleCollider != null)
            {
                volume = 4f / 3f * Mathf.PI * Mathf.Pow(capsuleCollider.radius, 3) + Mathf.PI * Mathf.Pow(capsuleCollider.radius, 2) * (capsuleCollider.height - capsuleCollider.radius * 2);
            }

            UpdateContents();
        }

        protected override void FixedUpdate()
        {
            UpdateContents();

            UpdateAvailableVolume();
            contents.Update();

            rigidbody.mass = _emptyMass + contents.materials.Sum(material => material.mass);
        }

        [field: SerializeField] public Contents contents { get; private set; } = new();

        public float GetPressureAtPoint(Vector3 point, out float liquidPressure)
        {
            float tankHeight = Mathf.Pow(volume, 1 / 3f);
            float tankBottom = transform.position.y - tankHeight;

            float liquidVolume = 0;
            float liquidDensity = 0;

            foreach (Material material in contents.liquids)
            {
                liquidVolume += material.volume;
                liquidDensity += material.mass;
            }

            if (liquidVolume > 0)
            {
                liquidDensity /= liquidVolume;
            }

            float liquidHeight = Mathf.Pow(liquidVolume, 1 / 3f);
            float liquidTop = tankBottom + liquidHeight;
            float heightInLiquid = Math.Max(0, liquidTop - point.y);
            liquidPressure = liquidDensity * -Physics.gravity.y * heightInLiquid;

            float gasPressure = contents.gasses.Sum(material => material.pressure);

            return gasPressure + liquidPressure;
        }

        private void UpdateContents()
        {
            UpdateAvailableVolume();
            contents.Update();
        }

        private void UpdateAvailableVolume()
        {
            float liquidsVolume = contents.liquids.Sum(material => material.volume);
            float gassesVolume = Mathf.Max(0.1f, volume - liquidsVolume);

            foreach (Material material in contents.gasses)
            {
                material.availableVolume = gassesVolume;
            }

            foreach (Material material in contents.liquids)
            {
                material.availableVolume = volume - liquidsVolume + material.volume;
            }
        }
    }
}
