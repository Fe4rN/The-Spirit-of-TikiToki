using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    private float damageRadius;
    private bool hasImpacted = false;
    [SerializeField] private float destroyDelay = 5f;
    [SerializeField] private float bounceForce = 5f;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Sonidos de Impacto")]
    [SerializeField] private AudioClip[] impactSounds;
    private AudioSource _audioSource;

    public void Initialize(float radius, AudioClip[] sounds)
    {
        damageRadius = radius;
        impactSounds = sounds;
        playerHealth = FindFirstObjectByType<PlayerHealth>();

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;

        // Añadimos la comprobación de "Player" para que explote si le cae encima
        if (collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.name == "Suelo" ||
            collision.gameObject.CompareTag("Tree") ||
            collision.gameObject.CompareTag("Player"))
        {
            hasImpacted = true;

            if (impactSounds != null && impactSounds.Length > 0 && AudioManager.Instance != null)
            {
                AudioClip clipSelected = impactSounds[Random.Range(0, impactSounds.Length)];
                AudioManager.Instance.Play3DSound(clipSelected, transform.position);
            }

            // Parar emisión de partículas
            ParticleSystem[] allParticles = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in allParticles)
            {
                ps.Stop();
            }

            // Aplicar daño en área
            ApplyDamage();

            // --- LÓGICA DE REBOTE ---
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Aplicamos un impulso hacia arriba para el rebote
                rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
            }

            // Iniciamos la rutina que espera un poco antes de clavar el objeto y encogerlo
            StartCoroutine(WaitThenShrink());
        }
        else if (collision.gameObject.CompareTag("Destruible") || collision.gameObject.CompareTag("InvisibleWall"))
        { 
            Destroy(gameObject);
        }
        
    }

    private void ApplyDamage()
    {
        HashSet<Tree> treesProcessed = new HashSet<Tree>();
        // 1. DAÑO AL PLAYER
        // OverlapSphere detectará el BoxCollider del player sin problemas
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                // Intentamos obtener la salud directamente del objeto impactado
                PlayerHealth health = hitCollider.GetComponent<PlayerHealth>();

                if (health != null)
                {
                    health.TakeDamage();
                }
                else if (playerHealth != null)
                {
                    // Usamos la referencia global si el componente no está en el collider
                    playerHealth.TakeDamage();
                }
            }
            // 2. DAÑO A LOS ÁRBOLES
            else if (hitCollider.CompareTag("Tree"))
            {
                Tree tree = hitCollider.GetComponentInParent<Tree>();

                if (tree != null && !treesProcessed.Contains(tree))
                {
                    treesProcessed.Add(tree); // Marcamos que este árbol ya recibió el impacto

                    // USAMOS EL NUEVO MÉTODO SIN TOCAR EL ANTERIOR
                    tree.ApplyMeteorDamage();
                }
            }
        }
    }

    private IEnumerator WaitThenShrink()
    {
        // Esperamos un tiempo breve (0.5s) para que se vea el rebote físico
        yield return new WaitForSeconds(2f);

        // Ahora sí desactivamos la física para que se quede en el sitio
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Iniciar desaparición gradual
        float currentTime = 0f;
        Vector3 originalScale = transform.localScale;

        while (currentTime < destroyDelay)
        {
            currentTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, currentTime / destroyDelay);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}