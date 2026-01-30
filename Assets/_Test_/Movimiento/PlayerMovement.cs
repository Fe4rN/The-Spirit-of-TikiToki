using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float speed = 6f;
    [SerializeField] private float rotationSpeed = 720f;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 moveInput;

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

        Vector3 finalVelocity = (moveInput * speed);
        controller.Move(finalVelocity * Time.deltaTime);
    }
}