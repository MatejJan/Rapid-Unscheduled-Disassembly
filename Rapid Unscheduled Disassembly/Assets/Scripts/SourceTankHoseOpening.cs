using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class SourceTankHoseOpening : MonoBehaviour
    {
        public float retractSpeed;
        public Hose hose;

        private bool _dragging;

        private Camera _camera;
        private DragAndDropControls _dragAndDropControls;
        private float _onDiameter;
        private Quaternion _restRotation;
        private Renderer _renderer;

        private Vector3 _restPosition;

        private void Awake()
        {
            _camera = Camera.main;
            _restPosition = transform.position;
            _restRotation = transform.rotation;
        }

        private void Start()
        {
            _dragAndDropControls = FindObjectOfType<DragAndDropControls>();
        }

        private void Update()
        {
            if (_dragging)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    _dragAndDropControls.EndDrag();
                    _dragging = false;
                    hose.endConnection = null;
                    hose.lengthChangeSpeed = -retractSpeed;
                }

                return;
            }

            if (Input.GetMouseButtonDown(0)) // Check if the left mouse button is clicked
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

                if (!Physics.Raycast(ray, out RaycastHit hit)) return;

                if (hit.collider.gameObject == gameObject)
                {
                    _dragAndDropControls.StartDrag(transform);
                    _dragging = true;
                }
            }
        }

        private void FixedUpdate()
        {
            if (hose.lengthChangeSpeed != 0 && hose.totalLength > 0)
            {
                transform.position = hose.transform.GetChild(0).position;
            }
            else
            {
                if (hose.endConnection != null) return;
                transform.position = _restPosition;
                hose.endConnection = gameObject;
                hose.lengthChangeSpeed = 0;
            }
        }
    }
}
