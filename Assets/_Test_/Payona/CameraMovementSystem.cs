using UnityEngine;

public class CameraMovementSystem : MonoBehaviour
{

    [Header("Ajustes de Velocidad")]
    public float speed = 10f;
    public float rotationSpeed = 720f; // Grados por segundo

    private CharacterController _controller;
    private Vector3 _moveInput;
    private float _verticalVelocity; // Solo para mantenerlo pegado al suelo

    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. Obtener entrada (Input)
        // Usamos GetAxisRaw para que el inicio y parada sean instantáneos (sin suavizado de Unity)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        _moveInput = new Vector3(moveX, 0, moveZ).normalized;

        // 2. Rotación del personaje
        if (_moveInput != Vector3.zero)
        {
            // Calculamos hacia dónde debe mirar
            Quaternion targetRotation = Quaternion.LookRotation(_moveInput);
            // Giramos de forma fluida pero rápida
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 3. Gravedad constante
        // Aunque no haya salto, necesitamos una fuerza hacia abajo para que el 
        // CharacterController detecte que está "Grounded" y baje rampas o escalones.
        if (_controller.isGrounded)
        {
            _verticalVelocity = -1f; // Fuerza mínima para mantener el contacto
        }
        else
        {
            _verticalVelocity -= 9.81f * Time.deltaTime; // Gravedad normal si cae
        }

        // 4. Aplicar el movimiento final
        Vector3 finalVelocity = (_moveInput * speed) + (Vector3.up * _verticalVelocity);
        _controller.Move(finalVelocity * Time.deltaTime);
    }
}
