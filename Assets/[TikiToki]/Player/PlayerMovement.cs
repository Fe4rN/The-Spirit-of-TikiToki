using UnityEngine;

namespace TikiToki.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float speed = 6f;
        [SerializeField] private float rotationSpeed = 720f;

        private CharacterController controller;
        private Vector3 moveInput;

        private Vector3 windDirection;
        private float windStrength;
        private float verticalVelocity;

        private bool isStunned = false;
        private float stunTimer = 0f;

        [Header("Stun Effect Settings")]
        [SerializeField] private float tremoloIntensity = 0.5f;
        [SerializeField] private float tremoloFrequency = 8f;

        void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            // Update stun timer
            if (isStunned)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                {
                    isStunned = false;
                    stunTimer = 0f;
                }
            }

            // If stunned, movement is blocked
            if (isStunned)
            {
                // Only apply gravity
                if (controller.isGrounded)
                {
                    verticalVelocity = -1f;
                }
                else
                {
                    verticalVelocity -= 9.81f * Time.deltaTime;
                }

                // Apply tremor effect
                Vector3 tremolo = GetTremoloOffset();
                Vector3 gravityOnly = Vector3.up * verticalVelocity + tremolo;
                controller.Move(gravityOnly * Time.deltaTime);
                return;
            }

            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            moveInput = new Vector3(moveX, 0, moveZ).normalized;

            if (moveInput != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveInput);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (controller.isGrounded)
            {
                verticalVelocity = -1f; // Minimum force to keep contact
            }
            else
            {
                verticalVelocity -= 9.81f * Time.deltaTime; // Normal gravity when falling
            }

            float againstWindFactor = 1f;

            if (moveInput != Vector3.zero && windStrength > 0f)
            {
                float dot = Vector3.Dot(moveInput.normalized, windDirection);
                if (dot < 0)
                {
                    float resistance = Mathf.Abs(dot) * windStrength;
                    againstWindFactor -= resistance;
                }
            }

            Vector3 finalVelocity = moveInput * speed * Mathf.Clamp(againstWindFactor, 0.2f, 1f) + (Vector3.up * verticalVelocity);
            controller.Move(finalVelocity * Time.deltaTime);
        }

        public void SetWind(Vector3 direction, float strength)
        {
            windDirection = direction.normalized;
            windStrength = strength;
        }

        public void ClearWind()
        {
            windStrength = 0f;
        }

        public void ApplyStun(float duration)
        {
            isStunned = true;
            stunTimer = duration;
            Debug.Log($"Player stunned for {duration} seconds");
        }

        private Vector3 GetTremoloOffset()
        {
            float xTremolo = Mathf.Sin(Time.time * tremoloFrequency) * tremoloIntensity;
            float zTremolo = Mathf.Cos(Time.time * tremoloFrequency * 0.7f) * tremoloIntensity;
            return new Vector3(xTremolo, 0, zTremolo);
        }
    }
}
