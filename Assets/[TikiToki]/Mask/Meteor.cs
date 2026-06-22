using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TikiToki.Gameplay.Player;
using TikiToki.Gameplay.Environment;
using Tree = TikiToki.Gameplay.Environment.Tree;

namespace TikiToki.Gameplay.Boss
{
    public class Meteor : MonoBehaviour
    {
        private float damageRadius;
        private bool hasImpacted = false;
        [SerializeField] private float destroyDelay = 5f;
        [SerializeField] private float bounceForce = 5f;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Impact Sounds")]
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

            if (collision.gameObject.CompareTag("Ground") ||
                collision.gameObject.name == "Suelo" ||
                collision.gameObject.CompareTag("Tree") ||
                collision.gameObject.CompareTag("Untagged") ||
                collision.gameObject.CompareTag("Player"))
            {
                hasImpacted = true;

                if (impactSounds != null && impactSounds.Length > 0 && AudioManager.Instance != null)
                {
                    AudioClip clipSelected = impactSounds[Random.Range(0, impactSounds.Length)];
                    AudioManager.Instance.Play3DSound(clipSelected, transform.position);
                }

                ParticleSystem[] allParticles = GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in allParticles)
                {
                    ps.Stop();
                }

                ApplyDamage();

                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
                }

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
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    PlayerHealth health = hitCollider.GetComponent<PlayerHealth>();

                    if (health != null)
                    {
                        health.TakeDamage();
                    }
                    else if (playerHealth != null)
                    {
                        playerHealth.TakeDamage();
                    }
                }
                else if (hitCollider.CompareTag("Tree"))
                {
                    Tree tree = hitCollider.GetComponentInParent<Tree>();

                    if (tree != null && !treesProcessed.Contains(tree))
                    {
                        treesProcessed.Add(tree);
                        tree.ApplyMeteorDamage();
                    }
                }
            }
        }

        private IEnumerator WaitThenShrink()
        {
            yield return new WaitForSeconds(2f);

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

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
}
