using System.Linq;
using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class Opening : MonoBehaviour
    {
        public MaterialType air;

        public float diameter;

        public Structure parentStructure;

        public Opening connectedOpening;

        public float externalPressure;
        public float internalPressure;

        public IContainer parentContainer;

        private Vent _vent;

        public float area => Mathf.PI * Mathf.Pow(diameter / 2f, 2);

        private void Start()
        {
            Transform structureTransform = transform.parent.parent;
            parentStructure = structureTransform.GetComponent<Structure>();
            parentContainer = structureTransform.GetComponent<IContainer>();

            _vent = transform.Find("Vent").GetComponent<Vent>();
        }

        private void FixedUpdate()
        {
            internalPressure = parentContainer.GetPressureAtPoint(transform.position, out float liquidPressure);

            if (connectedOpening is null)
            {
                externalPressure = Physicsf.atmosphericPressure;
            }
            else
            {
                externalPressure = connectedOpening.parentContainer.GetPressureAtPoint(transform.position, out _);
            }

            float pressure = internalPressure - externalPressure;

            if (pressure < 0)
            {
                // If there is a connection, the other side will push material in.
                if (connectedOpening is not null) return;

                // Suck air into the hole.

                // Calculate entrance velocity.
                // v = √(2P / ρ)
                float density = Physicsf.airDensityAtSeaLevel;
                float enterSpeed = Mathf.Sqrt(2 * -pressure / density);

                // Calculate mass flow rate.
                // Q = ρ * v * A
                float massFlowRate = density * enterSpeed * area;

                // Absorb the air.
                Material absorbedAir = new()
                {
                    type = air,
                    mass = massFlowRate * Time.fixedDeltaTime,
                    temperature = 300
                };

                parentContainer.contents.AddMaterial(absorbedAir);

                return;
            }

            float minimumArea = area;

            if (connectedOpening is not null && connectedOpening.area < area)
            {
                minimumArea = connectedOpening.area;
            }

            float totalVolume = parentContainer.contents.materials.Sum(material => material.volume);
            float totalLiquidVolume = parentContainer.contents.liquids.Sum(material => material.volume);

            foreach (Material material in parentContainer.contents.materials)
            {
                if (material.mass == 0) continue;

                if (liquidPressure > 0 && material.state != Material.State.Liquid) continue;

                // Calculate exit velocity.
                // v = √(2P / ρ)
                float density = material.mass / material.volume;
                float exitSpeed = Mathf.Sqrt(2 * pressure / density);

                // Calculate mass flow rate.
                // Q = ρ * v * A
                float massFlowRate = density * exitSpeed * minimumArea;

                // Weight it by volume % in the container.
                if (liquidPressure > 0)
                {
                    massFlowRate *= material.volume / totalLiquidVolume;
                }
                else
                {
                    massFlowRate *= material.volume / totalVolume;
                }

                // Remove the material.
                float removedMass = Mathf.Min(material.mass, massFlowRate * Time.fixedDeltaTime);
                Material removedMaterial = parentContainer.contents.RemoveMaterial(material.type, removedMass);

                if (connectedOpening is null)
                {
                    // Vent the material.
                    _vent.SetVentParameters(material, exitSpeed, massFlowRate);

                    // Create the force.
                    // F = (m * v) / Δt
                    float forceAmount = removedMass * exitSpeed / Time.fixedDeltaTime;

                    Vector3 direction = (transform.up + Random.insideUnitSphere).normalized;

                    if (parentStructure != null)
                    {
                        parentStructure.AddForce(forceAmount * direction, transform.position);
                    }
                }
                else
                {
                    connectedOpening.parentContainer.contents.AddMaterial(removedMaterial);
                }
            }
        }
    }
}
