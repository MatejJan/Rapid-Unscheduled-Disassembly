using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class TrackingCamera : MonoBehaviour
    {
        public enum Type
        {
            Pan,
            Follow
        }

        public Transform trackedTransform;
        public Type type;
        private Vector3 followOffset;

        private void Start()
        {
            followOffset = transform.position - trackedTransform.position;
        }

        private void Update()
        {
            switch (type)
            {
                case Type.Pan:
                    transform.LookAt(trackedTransform);

                    break;

                case Type.Follow:
                    transform.position = trackedTransform.position + followOffset;

                    break;
            }
        }
    }
}
