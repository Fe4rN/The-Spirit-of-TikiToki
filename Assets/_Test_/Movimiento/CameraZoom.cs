using Unity.Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private float zoomSpeed;
    private float minY = 5f;
    private float maxY = 10f;
    private float minZ = -5f;
    private float maxZ = -10f;

    private CinemachineFollow cameraFollow;

    void Start()
    {
        cameraFollow = transform.GetComponent<CinemachineFollow>();
    }

    void Update()
    {
        Vector3 offset = cameraFollow.FollowOffset;

        // Acercar (zoom in)
        if (Input.GetKey(KeyCode.Q))
        {
            offset.y -= zoomSpeed * Time.deltaTime;
            offset.z += zoomSpeed * Time.deltaTime;
        }

        // Alejar (zoom out)
        if (Input.GetKey(KeyCode.E))
        {
            offset.y += zoomSpeed * Time.deltaTime;
            offset.z -= zoomSpeed * Time.deltaTime;
        }

        // Limitar el zoom
        offset.y = Mathf.Clamp(offset.y, minY, maxY);
        offset.z = Mathf.Clamp(offset.z, maxZ, minZ);

        cameraFollow.FollowOffset = offset;
    }
}
