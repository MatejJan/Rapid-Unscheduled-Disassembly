using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public interface IContainer
    {
        public Contents contents { get; }

        public float GetPressureAtPoint(Vector3 point, out float liquidPressure);
    }
}
