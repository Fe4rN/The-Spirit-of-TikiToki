using UnityEngine;
using System.Collections;

public class Meteor : MonoBehaviour
{
    private float damageRadius;
    private bool hasImpacted = false;
    [SerializeField] private float destroyDelay = 2f;

    public void Initialize(float radius)
    {
        damageRadius = radius;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;

        // Verificar si colisiona con el suelo
        if (collision.gameObject.name == "Suelo" || collision.gameObject.CompareTag("Ground"))
        {
            hasImpacted = true;

            // Aplicar daño en área inmediatamente
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    Debug.Log($"Meteor hit player! Position: {transform.position}");
                    // TODO: Aplicar daño al jugador
                    PlayerMovement player = hitCollider.GetComponent<PlayerMovement>();
                    if (player != null)
                    {
                        // Aquí puedes agregar daño cuando tengas el sistema de vida
                    }
                }
            }

            // Desactivar la física para que no siga rodando
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Destruir después de 2 segundos
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Visualizar el radio de daño en el editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
