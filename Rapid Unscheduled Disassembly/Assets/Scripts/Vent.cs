using System.Collections.Generic;
using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class Vent : MonoBehaviour
    {
        public GameObject ventedMaterialPrefab;

        private readonly Dictionary<MaterialType, VentedMaterial> _ventedMaterials = new();

        private GameObject ground;
        private ParticleSystem _gasShapeParticleSystem;

        private void Awake()
        {
            _gasShapeParticleSystem = GetComponent<ParticleSystem>();
            _gasShapeParticleSystem.Stop();
        }

        private void Start()
        {
            ground = GameObject.Find("Ground");
        }

        public void SetVentParameters(Material material, float speed, float rate)
        {
            VentedMaterial ventedMaterial;

            if (!_ventedMaterials.ContainsKey(material.type))
            {
                GameObject ventedMaterialGameObject = Instantiate(ventedMaterialPrefab, transform);
                ventedMaterialGameObject.name = material.type.name;

                ventedMaterial = ventedMaterialGameObject.GetComponent<VentedMaterial>();
                ventedMaterial.Initialize(material.type, _gasShapeParticleSystem.shape, ground);

                _ventedMaterials[material.type] = ventedMaterial;
            }
            else
            {
                ventedMaterial = _ventedMaterials[material.type];
            }

            ventedMaterial.SetVentParameters(material.state, speed, rate);
        }
    }
}
