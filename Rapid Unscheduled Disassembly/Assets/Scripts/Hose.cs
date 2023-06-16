using System;
using System.Collections.Generic;
using UnityEngine;

namespace RapidUnscheduledDisassembly
{
    public class Hose : MonoBehaviour
    {
        public GameObject pointPrefab;
        public GameObject startConnection;
        public GameObject endConnection;
        public bool dynamicLength;
        public float lengthChangeSpeed;

        public float width = 0.05f;
        public float dampen = 1;
        public float stiffen;
        public float pullForceFactor = 10;
        public float separationLength = 0.1f;

        public float totalLength;
        private readonly List<Rigidbody> _pointRigidBodies = new();

        private readonly List<Transform> _pointTransforms = new();
        private float _remainingSeparationLength;

        private LineRenderer _lineRenderer;
        private Opening _endOpening;

        private Opening _startOpening;

        private Vector3[] _linePositions;

        private void Awake()
        {
            Vector3 endPosition = endConnection.transform.position;
            Vector3 difference = startConnection.transform.position - endPosition;
            totalLength = 2 * difference.magnitude;

            int pointsCount = Mathf.FloorToInt(totalLength / separationLength) + 1;
            _remainingSeparationLength = totalLength - (pointsCount - 1) * separationLength;

            Vector3 direction = difference.normalized;

            _linePositions = new Vector3[pointsCount * 2];

            for (int i = 0; i < pointsCount; i++)
            {
                GameObject point = Instantiate(pointPrefab, transform);
                _linePositions[i] = endPosition + i * separationLength / 2 * direction;
                point.transform.position = _linePositions[i];

                _pointTransforms.Add(point.transform);
                _pointRigidBodies.Add(point.GetComponent<Rigidbody>());
            }

            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
            _lineRenderer.positionCount = pointsCount;

            _lineRenderer.SetPositions(_linePositions);

            _startOpening = startConnection.GetComponent<Opening>();
            _endOpening = startConnection.GetComponent<Opening>();
        }

        private void Update()
        {
            bool haveStartConnection = startConnection != null;
            bool haveEndConnection = endConnection != null;

            if (_lineRenderer.positionCount != _pointTransforms.Count)
            {
                _lineRenderer.positionCount = _pointTransforms.Count;

                if (_linePositions.Length < _pointTransforms.Count)
                {
                    _linePositions = new Vector3[_pointTransforms.Count * 2];
                }
            }

            for (int i = 0; i < _pointTransforms.Count; i++)
            {
                _linePositions[i] = _pointTransforms[i].position;
            }

            if (haveEndConnection)
            {
                _linePositions[0] = endConnection.transform.position;
            }

            if (haveStartConnection)
            {
                _linePositions[_pointTransforms.Count - 1] = startConnection.transform.position;
            }

            _lineRenderer.SetPositions(_linePositions);

            if (dynamicLength)
            {
                totalLength = Mathf.Max(0, totalLength + lengthChangeSpeed);

                if (haveStartConnection && haveEndConnection)
                {
                    Vector3 endPosition = endConnection.transform.position;
                    Vector3 startPosition = startConnection.transform.position;
                    Vector3 difference = startPosition - endPosition;
                    totalLength = 2 * difference.magnitude;
                }

                int pointsCount = Math.Max(2, Mathf.FloorToInt(totalLength / separationLength) + 1);
                _remainingSeparationLength = totalLength - (pointsCount - 1) * separationLength;

                while (pointsCount > _pointTransforms.Count)
                {
                    // Add a new point.
                    GameObject point = Instantiate(pointPrefab, transform);
                    point.transform.position = startConnection.transform.position;

                    _pointTransforms.Add(point.transform);
                    _pointRigidBodies.Add(point.GetComponent<Rigidbody>());
                }

                while (pointsCount < _pointTransforms.Count)
                {
                    // remove a point.
                    Destroy(_pointTransforms[^1].gameObject);
                    _pointTransforms.RemoveAt(_pointTransforms.Count - 1);
                    _pointRigidBodies.RemoveAt(_pointRigidBodies.Count - 1);
                }
            }
        }

        private void FixedUpdate()
        {
            bool haveStartConnection = startConnection != null;
            bool haveEndConnection = endConnection != null;

            int n = _pointTransforms.Count;
            int m = n / 4;

            for (int i = 0; i < n - 1; i++)
            {
                ConstrainPair(i, i + 1);
            }

            if (haveEndConnection) Stiffen(0, 1, -endConnection.transform.up, 0.25f);
            if (haveStartConnection) Stiffen(n - 1, n - 2, -startConnection.transform.up, 0.25f);

            for (int i = 1; i < m; i++)
            {
                float factor = (1 - (float)i / m) * stiffen;
                if (haveEndConnection) Stiffen(i, 1, factor);
                if (haveStartConnection) Stiffen(n - 1 - i, -1, factor);
            }

            foreach (Rigidbody pointRigidbody in _pointRigidBodies)
            {
                pointRigidbody.angularVelocity = Vector3.zero;
                pointRigidbody.rotation = Quaternion.identity;
            }

            if (haveEndConnection)
            {
                _pointTransforms[0].position = endConnection.transform.position;
                _pointRigidBodies[0].velocity = Vector3.zero;
            }

            if (haveStartConnection)
            {
                _pointTransforms[^1].position = startConnection.transform.position;
                _pointRigidBodies[^1].velocity = Vector3.zero;
            }
        }

        private void ConstrainPair(int index1, int index2)
        {
            // Maintain separation.
            float targetDistance = index2 == _pointTransforms.Count - 1 ? _remainingSeparationLength : separationLength;
            Vector3 difference = _pointTransforms[index1].position - _pointTransforms[index2].position;
            float correctionDistance = (targetDistance - difference.magnitude) / 2 * dampen;
            Vector3 direction = difference.normalized;

            _pointTransforms[index1].position += direction * correctionDistance;
            _pointTransforms[index2].position -= direction * correctionDistance;

            // Correct velocities.
            float correctionSpeed = correctionDistance / Time.fixedDeltaTime;
            AddForce(index1, direction * correctionSpeed);
            AddForce(index2, direction * -correctionSpeed);
        }

        private void AddForce(int index, Vector3 force)
        {
            _pointRigidBodies[index].AddForce(force, ForceMode.VelocityChange);

            if (index != 0 && index != _pointRigidBodies.Count - 1) return;
            Opening opening = index == 0 ? _endOpening : _startOpening;

            if (opening is null) return;

            opening.parentStructure.AddForce(force * pullForceFactor, transform.position);
        }

        private void Stiffen(int parentIndex, int indexSign, float factor)
        {
            int childIndex = parentIndex + indexSign;
            int grandparentIndex = parentIndex - indexSign;

            Stiffen(parentIndex, childIndex, _pointTransforms[parentIndex].position - _pointTransforms[grandparentIndex].position, factor);
        }

        private void Stiffen(int parentIndex, int childIndex, Vector3 direction, float factor)
        {
            float targetDistance = parentIndex == _pointTransforms.Count - 1 ? _remainingSeparationLength : separationLength;
            Vector3 targetPosition = _pointTransforms[parentIndex].position + direction * targetDistance;
            Vector3 correctionDifference = targetPosition - _pointTransforms[childIndex].position;
            float correctionDistance = correctionDifference.magnitude * factor;
            Vector3 correctionDirection = correctionDifference.normalized;

            _pointTransforms[childIndex].position += correctionDirection * correctionDistance;

            // Correct velocities.
            float correctionSpeed = correctionDistance / Time.fixedDeltaTime;
            AddForce(childIndex, correctionDirection * correctionSpeed);
        }
    }
}
