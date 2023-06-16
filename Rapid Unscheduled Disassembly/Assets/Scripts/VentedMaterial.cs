using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class VentedMaterial : MonoBehaviour
    {
        private bool _wasSet;
        private MaterialType _materialType;
        private ParticleSystem _gasParticleSystem;
        private ParticleSystem _liquidParticleSystem;

        private void Awake()
        {
            _liquidParticleSystem = transform.Find("Liquid").GetComponent<ParticleSystem>();
            _gasParticleSystem = transform.Find("Gas").GetComponent<ParticleSystem>();
        }

        private void FixedUpdate()
        {
            if (_wasSet)
            {
                _wasSet = false;
            }
            else
            {
                DisableParticleSystem(_gasParticleSystem);
                DisableParticleSystem(_liquidParticleSystem);
            }
        }

        public void Initialize(MaterialType materialType, ParticleSystem.ShapeModule gasParticleSystemShape, GameObject ground)
        {
            _materialType = materialType;

            ParticleSystem.ShapeModule ownGasParticleSystemShape = _gasParticleSystem.shape;
            ownGasParticleSystemShape.angle = gasParticleSystemShape.angle;
            ownGasParticleSystemShape.radius = gasParticleSystemShape.radius;

            ParticleSystem[] particleSystems =
            {
                _gasParticleSystem,
                _liquidParticleSystem
            };

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                ParticleSystem.MainModule main = particleSystem.main;
                main.startColor = _materialType.particleColor;

                ParticleSystem.CollisionModule collission = particleSystem.collision;
                collission.AddPlane(ground.transform);

                Renderer particleRenderer = particleSystem.GetComponent<Renderer>();
                particleRenderer.material = _materialType.particleMaterial;
            }
        }

        public void SetVentParameters(Material.State state, float speed, float rate)
        {
            _wasSet = true;

            // Enable the current particle system.
            ParticleSystem currentParticleSystem = state == Material.State.Gas ? _gasParticleSystem : _liquidParticleSystem;
            if (!currentParticleSystem.isPlaying) currentParticleSystem.Play();

            ParticleSystem.MainModule main = currentParticleSystem.main;
            main.startSpeed = speed * _materialType.particleSpeedFactor;

            ParticleSystem.EmissionModule emission = currentParticleSystem.emission;
            emission.rateOverTime = Mathf.Min(rate * _materialType.particleRateFactor, main.maxParticles);

            // Disable the other particle system.
            ParticleSystem otherParticleSystem = state == Material.State.Liquid ? _gasParticleSystem : _liquidParticleSystem;
            DisableParticleSystem(otherParticleSystem);
        }

        private void DisableParticleSystem(ParticleSystem particleSystem)
        {
            // Disable the other particle system.
            if (!particleSystem.isPlaying) return;

            ParticleSystem.EmissionModule otherEmission = particleSystem.emission;
            otherEmission.rateOverTime = 0;

            if (particleSystem.IsAlive())
            {
                particleSystem.Stop();
            }
        }
    }
}
