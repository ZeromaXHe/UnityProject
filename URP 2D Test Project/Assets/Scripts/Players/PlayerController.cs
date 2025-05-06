using UnityEngine;
using UnityEngine.InputSystem;

namespace Players
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerInputControl _inputControl;
        private Rigidbody2D _rb;
        private PhysicsCheck _physicsCheck;
        public Vector2 inputDirection;
        [Header("基本参数")]
        public float speed;
        public float jumpForce;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _physicsCheck = GetComponent<PhysicsCheck>();
            _inputControl = new PlayerInputControl();
            _inputControl.Gameplay.Jump.started += Jump;
        }

        private void OnEnable()
        {
            _inputControl.Enable();
        }

        private void OnDisable()
        {
            _inputControl.Disable();
        }

        private void Update()
        {
            inputDirection = _inputControl.Gameplay.Move.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void Move()
        {
            _rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime, _rb.velocity.y);
            var faceDir = (int)transform.localScale.x;
            if (inputDirection.x > 0)
                faceDir = 1;
            if (inputDirection.x < 0)
                faceDir = -1;
            // 人物翻转
            transform.localScale = new Vector3(faceDir, 1, 1);
        }

        private void Jump(InputAction.CallbackContext obj)
        {
            // Debug.Log("JUMP!");
            if (_physicsCheck.isGround)
                _rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        }
    }
}