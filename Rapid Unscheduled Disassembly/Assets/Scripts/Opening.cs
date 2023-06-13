using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class Opening : MonoBehaviour
    {
        public const float particleRateFactor = 5e5f;
        public const float particleSpeedFactor = 1e-2f;

        public float diameter;

        public Tank parent;

        public float exitSpeed;
        public float ventedMass;

        private ParticleSystem _particleSystem;

        public float area => Mathf.PI * Mathf.Pow(diameter / 2f, 2);

        private void Start()
        {
            parent = transform.parent.parent.GetComponent<Tank>();

            _particleSystem = GetComponent<ParticleSystem>();
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
            float pressure = parent.contents.pressure - Physicsf.atmosphericPressure;

            if (pressure < 0)
            {
                exitSpeed = 0;
                ventedMass = 0;

                return;
            }

            // Calculate exit velocity.
            // v = √(2P / ρ)
            float density = parent.contents.mass / parent.volume;
            exitSpeed = Mathf.Sqrt(2 * pressure / density);

            // Calculate flow rate.
            // Q = v * A
            float flowRate = exitSpeed * area;

            // Vent the material.
            float ventedVolume = flowRate * Time.fixedDeltaTime;
            ventedMass = ventedVolume * density;

            if (ventedMass > parent.contents.mass)
            {
                ventedMass = parent.contents.mass;
                ventedVolume = ventedMass / density;
            }

            parent.contents.mass -= ventedMass;

            // Create the force.
            // F = (m * v) / Δt
            float forceAmount = ventedMass * exitSpeed / Time.fixedDeltaTime;

            int n = 1;

            for (int i = 0; i < n; i++)
            {
                Vector3 direction = (-transform.forward + Random.insideUnitSphere).normalized;

                parent.AddForce(forceAmount / n * direction);
            }
        }
    }
}
