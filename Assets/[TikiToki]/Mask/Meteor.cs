using UnityEngine;
using System.Collections;

public class Meteor : MonoBehaviour
{
    private float damageRadius;
    private bool hasImpacted = false;
    [SerializeField] private float destroyDelay = 5f; // Tiempo total que tarda en desaparecer

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

            // Parar emisión de partículas
            ParticleSystem[] allParticles = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in allParticles)
            {
                ps.Stop();
            }

            // Aplicar daño en área
            ApplyDamage();

            // Desactivar física
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Iniciar desaparición gradual
            StartCoroutine(ShrinkAndDestroy());
        }
    }

    private void ApplyDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlayerMovement player = hitCollider.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    // Lógica de daño aquí
                }
            }
        }
    }

    private IEnumerator ShrinkAndDestroy()
    {
        Vector3 originalScale = transform.localScale;
        float currentTime = 0f;

        // Mientras no hayamos llegado al tiempo límite
        while (currentTime < destroyDelay)
        {
            currentTime += Time.deltaTime;

            // Interpolamos la escala de la original a cero
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, currentTime / destroyDelay);

            yield return null; // Esperar al siguiente frame
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}