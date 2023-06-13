using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class Tank : MonoBehaviour
    {
        public float volume;
        public float maximumPressure;

        public Material contents;
        public float thrustToWeightRatio;

        private float _emptyMass;
        private float _thrust;

        private Rigidbody _rigidbody;

        public void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _emptyMass = _rigidbody.mass;

            SphereCollider sphereCollider = GetComponent<SphereCollider>();

            if (sphereCollider is not null)
            {
                volume = 4f / 3f * Mathf.PI * Mathf.Pow(sphereCollider.radius, 3);
            }

            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

            if (capsuleCollider is not null)
            {
                volume = 4f / 3f * Mathf.PI * Mathf.Pow(capsuleCollider.radius, 3) + Mathf.PI * Mathf.Pow(capsuleCollider.radius, 2) * (capsuleCollider.height - capsuleCollider.radius * 2);
            }

            UpdatePressure();
        }

        public void FixedUpdate()
        {
            UpdatePressure();

            _rigidbody.mass = _emptyMass + contents.mass;

            // Add some wind.
            float windAmount = Random.value * 0.01f;
            Vector3 windDirection = Vector3.left;
            Vector3 position = transform.position + Random.insideUnitSphere;

            _rigidbody.AddForceAtPosition(windAmount * windDirection, position);

            // Display thrust to weight ratio.
            thrustToWeightRatio = _thrust / (_rigidbody.mass * Physics.gravity.magnitude);
            _thrust = 0;
        }

        public void UpdatePressure()
        {
            // Update pressure.
            // P = (nRT) / V
            float n = contents.amountOfSubstance;
            float R = Physicsf.molarGasConstant;
            float T = contents.temperature;
            float V = volume;

            contents.pressure = n * R * T / V;
        }

        public void AddForce(Vector3 force)
        {
            _rigidbody.AddForce(force);

            _thrust += force.magnitude;
        }
    }
}
