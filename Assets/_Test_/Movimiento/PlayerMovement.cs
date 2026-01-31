using UnityEngine;

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

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
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
            verticalVelocity = -1f; // Fuerza m�nima para mantener el contacto
        }
        else
        {
            verticalVelocity -= 9.81f * Time.deltaTime; // Gravedad normal si cae
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
}