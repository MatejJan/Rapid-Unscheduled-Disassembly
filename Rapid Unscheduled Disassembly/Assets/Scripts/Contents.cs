using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    [Serializable]
    public class Contents
    {
        [SerializeField] private List<Material> _materials = new();

        public IEnumerable<Material> materials => _materials;
        public IEnumerable<Material> gasses => materials.Where(material => material.state == Material.State.Gas);
        public IEnumerable<Material> liquids => materials.Where(material => material.state == Material.State.Liquid);

        public void Initialize()
        {
            foreach (Material material in _materials)
            {
                material.Update();
            }
        }

        public void AddMaterial(Material material)
        {
            int existingIndex = _materials.FindIndex(existingMaterial => existingMaterial.type == material.type);

            if (existingIndex < 0)
            {
                _materials.Add(material);

                return;
            }

            Material existingMaterial = _materials[existingIndex];

            float totalMass = material.mass + existingMaterial.mass;
            float ratio = material.mass / totalMass;

            existingMaterial.temperature = Mathf.Lerp(existingMaterial.temperature, material.temperature, ratio);
            existingMaterial.mass = totalMass;
        }

        public Material RemoveMaterial(MaterialType type, float? mass = null)
        {
            int existingIndex = _materials.FindIndex(existingMaterial => existingMaterial.type == type);
            Material existingMaterial = _materials[existingIndex];

            Material removedMaterial = new()
            {
                type = existingMaterial.type,
                mass = Mathf.Min(mass ?? existingMaterial.mass, existingMaterial.mass),
                temperature = existingMaterial.temperature,
                pressure = existingMaterial.pressure
            };

            existingMaterial.mass -= removedMaterial.mass;

            return removedMaterial;
        }

        public void Update()
        {
            for (int i = 0; i < _materials.Count; i++)
            {
                if (_materials[i].mass == 0)
                {
                    _materials.RemoveAt(i);
                    i--;
                }
                else
                {
                    _materials[i].Update();
                }
            }
        }
    }
}
