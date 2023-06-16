using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class FlowButton : MonoBehaviour
    {
        public UnityEngine.Material onMaterial;
        public UnityEngine.Material offMaterial;
        public Opening opening;

        private bool _isOn;
        private Camera _camera;
        private float _onDiameter;
        private Renderer _renderer;

        private void Start()
        {
            _camera = Camera.main;
            _renderer = GetComponent<Renderer>();

            _onDiameter = opening.diameter;
            opening.diameter = 0;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) // Check if the left mouse button is clicked
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

                if (!Physics.Raycast(ray, out RaycastHit hit)) return;

                if (hit.collider.gameObject == gameObject)
                {
                    _isOn = !_isOn;

                    _renderer.material = _isOn ? onMaterial : offMaterial;

                    opening.diameter = _isOn ? _onDiameter : 0;
                }
            }
        }
    }
}
