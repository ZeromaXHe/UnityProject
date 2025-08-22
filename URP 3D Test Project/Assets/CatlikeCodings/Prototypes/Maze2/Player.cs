using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 16:07:47
    public class Player : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float movementSpeed = 4f, rotationSpeed = 180f, mouseSensitivity = 5f;
        [SerializeField] private float startingVerticalEyeAngle = 10f;
        private CharacterController _characterController;
        private Transform _eye;
        private Vector2 _eyeAngles;
        private Camera _eyeCamera;
        private FieldOfView _vision;

        public FieldOfView Vision => _vision;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _eye = transform.GetChild(0);
            _eyeCamera = _eye.GetComponent<Camera>();
            _vision.Range = 1000f;
        }

        public void StartNewGame(Vector3 position)
        {
            _eyeAngles.x = Random.Range(0f, 360f);
            _eyeAngles.y = startingVerticalEyeAngle;
            _characterController.enabled = false;
            transform.localPosition = position;
            _characterController.enabled = true;
        }

        public Vector3 Move()
        {
            UpdateEyeAngles();
            UpdatePosition();
            return transform.localPosition;
        }

        private void UpdatePosition()
        {
            var movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            var sqrMagnitude = movement.sqrMagnitude;
            if (sqrMagnitude > 1f)
            {
                movement /= Mathf.Sqrt(sqrMagnitude);
            }

            movement *= movementSpeed;

            var forward = new Vector2(
                Mathf.Sin(_eyeAngles.x * Mathf.Deg2Rad),
                Mathf.Cos(_eyeAngles.x * Mathf.Deg2Rad)
            );
            var right = new Vector2(forward.y, -forward.x);

            movement = right * movement.x + forward * movement.y;
            _characterController.SimpleMove(new Vector3(movement.x, 0f, movement.y));
        }

        private void UpdateEyeAngles()
        {
            var rotationDelta = rotationSpeed * Time.deltaTime;
            _eyeAngles.x += rotationDelta * Input.GetAxis("Horizontal View");
            _eyeAngles.y -= rotationDelta * Input.GetAxis("Vertical View");
            if (mouseSensitivity > 0f)
            {
                var mouseDelta = rotationDelta * mouseSensitivity;
                _eyeAngles.x += mouseDelta * Input.GetAxis("Mouse X");
                _eyeAngles.y -= mouseDelta * Input.GetAxis("Mouse Y");
            }

            if (_eyeAngles.x > 360f)
            {
                _eyeAngles.x -= 360f;
            }
            else if (_eyeAngles.x < 0f)
            {
                _eyeAngles.x += 360f;
            }

            _eyeAngles.y = Mathf.Clamp(_eyeAngles.y, -45f, 45f);
            var rotation = _eye.localRotation = Quaternion.Euler(_eyeAngles.y, _eyeAngles.x, 0f);
            var viewFactorY = Mathf.Tan(_eyeCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var viewFactorX = viewFactorY * _eyeCamera.aspect;
            var y = _eyeAngles.y < 0f ? viewFactorY : -viewFactorY;
            Vector3 leftLine = rotation * new Vector3(-viewFactorX, y, 1f),
                rightLine = rotation * new Vector3(viewFactorX, y, 1f);
            _vision.LeftLine = float2(leftLine.x, leftLine.z);
            _vision.RightLine = float2(rightLine.x, rightLine.z);
        }
    }
}