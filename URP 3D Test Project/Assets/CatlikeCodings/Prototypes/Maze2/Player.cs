using UnityEngine;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 16:07:47
    public class Player : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float movementSpeed = 4f, rotationSpeed = 180f, mouseSensitivity = 5f;
        [SerializeField] private float startingVerticalEyeAngle = 10f;
        private CharacterController characterController;
        private Transform eye;
        private Vector2 eyeAngles;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            eye = transform.GetChild(0);
        }

        public void StartNewGame(Vector3 position)
        {
            eyeAngles.x = Random.Range(0f, 360f);
            eyeAngles.y = startingVerticalEyeAngle;
            characterController.enabled = false;
            transform.localPosition = position;
            characterController.enabled = true;
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
                Mathf.Sin(eyeAngles.x * Mathf.Deg2Rad),
                Mathf.Cos(eyeAngles.x * Mathf.Deg2Rad)
            );
            var right = new Vector2(forward.y, -forward.x);

            movement = right * movement.x + forward * movement.y;
            characterController.SimpleMove(new Vector3(movement.x, 0f, movement.y));
        }

        private void UpdateEyeAngles()
        {
            var rotationDelta = rotationSpeed * Time.deltaTime;
            eyeAngles.x += rotationDelta * Input.GetAxis("Horizontal View");
            eyeAngles.y -= rotationDelta * Input.GetAxis("Vertical View");
            if (mouseSensitivity > 0f)
            {
                var mouseDelta = rotationDelta * mouseSensitivity;
                eyeAngles.x += mouseDelta * Input.GetAxis("Mouse X");
                eyeAngles.y -= mouseDelta * Input.GetAxis("Mouse Y");
            }

            if (eyeAngles.x > 360f)
            {
                eyeAngles.x -= 360f;
            }
            else if (eyeAngles.x < 0f)
            {
                eyeAngles.x += 360f;
            }

            eyeAngles.y = Mathf.Clamp(eyeAngles.y, -45f, 45f);
            eye.localRotation = Quaternion.Euler(eyeAngles.y, eyeAngles.x, 0f);
        }
    }
}