using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class Structure : MonoBehaviour
    {
        public float thrustToWeightRatio;
        private float _thrust;

        protected new Rigidbody rigidbody;

        protected virtual void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void FixedUpdate()
        {
            // Add some wind.
            float windAmount = Random.value * 0.01f;
            Vector3 windDirection = Vector3.left;
            Vector3 position = transform.position + Random.insideUnitSphere;

            rigidbody.AddForceAtPosition(windAmount * windDirection, position);

            // Display thrust to weight ratio.
            thrustToWeightRatio = _thrust / (rigidbody.mass * Physics.gravity.magnitude);
            _thrust = 0;
        }

        public void AddForce(Vector3 force, Vector3 position)
        {
            rigidbody.AddForceAtPosition(force, position);
            //rigidbody.AddForce(force);

            _thrust += force.magnitude;
        }
    }
}
