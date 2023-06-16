using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class DragAndDropControls : MonoBehaviour
    {
        private bool _isDragging;
        private Camera _camera;

        private Transform _draggedTransform;
        private Vector3 _offset;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (_draggedTransform is not null)
            {
                Vector3 mousePosition = GetMouseWorldPosition();
                _draggedTransform.position = new Vector3(mousePosition.x + _offset.x, mousePosition.y + _offset.y, _draggedTransform.position.z);
            }
        }

        public void StartDrag(Transform draggedTransform)
        {
            _draggedTransform = draggedTransform;
            _offset = draggedTransform.position - GetMouseWorldPosition();
        }

        public void EndDrag()
        {
            _draggedTransform = null;
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = -_camera.transform.position.z;

            return _camera.ScreenToWorldPoint(mousePosition);
        }
    }
}
