using UnityEngine;

public class MaskMovement : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

     [SerializeField] private float smoothTime = 0.2f;
    private float velocityX;

    void Update()
    {
        if(!playerTransform) return;

        Vector3 pos = transform.position;

        pos.x = Mathf.SmoothDamp( pos.x, playerTransform.position.x, ref velocityX, smoothTime );

        transform.position = pos;
    }
}
