using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TikiToki.Gameplay.Environment
{
    public class Tree : MonoBehaviour, IInteractable
    {
        [Header("Chopping Settings")]
        public int tapsToCut = 5;
        private float _currentDamage = 0f;

        [Header("Regeneration Settings")]
        public float timeToStartRegen = 3.0f;
        public float regenSpeed = 0.5f;
        private float _lastHitTime;

        [Header("References")]
        public TreeHealthBarTree healthBar;
        public GameObject woodPrefab;
        public int woodAmount = 3;

        [Header("Visuals")]
        public Vector3 normalScale = Vector3.one;
        private Vector3 _originalScale;
        private MeshRenderer[] _renderers;
        [ColorUsage(true, true)] public Color highlightColor = new Color(0.5f, 0.5f, 0.5f);

        [Header("Audio")]
        public AudioClip soundTreeHit;
        public AudioClip soundTreeFall;

        public static Action OnTreeHit;
        public static Action OnTreeDestroyed;

        // --- IInteractable Implementation ---
        public string InteractionPrompt => "Chop Tree";

        public bool CanInteract(TikiToki.Inventory.PlayerInventory inventory)
        {
            var activeSlot = inventory.slots[inventory.activeSlotIndex];
            return activeSlot.item != null && activeSlot.item.itemName.ToLower() == "axe";
        }

        public void Interact(TikiToki.Inventory.PlayerInventory inventory)
        {
            TakeHit();
        }

        public void SetHighlight(bool active)
        {
            if (_renderers == null) _renderers = GetComponentsInChildren<MeshRenderer>();

            foreach (var ren in _renderers)
            {
                if (ren == null) continue;
                Material[] mats = ren.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (active)
                    {
                        mats[i].EnableKeyword("_EMISSION");
                        mats[i].SetColor("_EmissionColor", highlightColor);
                    }
                    else
                    {
                        mats[i].SetColor("_EmissionColor", Color.black);
                    }
                }
                ren.materials = mats;
            }
        }

        void Awake()
        {
            _originalScale = (transform.localScale.magnitude > 0.1f) ? transform.localScale : normalScale;
            _renderers = GetComponentsInChildren<MeshRenderer>();
        }

        void Start()
        {
            if (_originalScale.magnitude < 0.1f) _originalScale = normalScale;
        }

        void Update()
        {
            if (_currentDamage > 0)
            {
                if (Time.time - _lastHitTime > timeToStartRegen)
                {
                    _currentDamage -= Time.deltaTime * regenSpeed;

                    if (_currentDamage <= 0)
                    {
                        _currentDamage = 0;
                        if (healthBar != null) healthBar.gameObject.SetActive(false);
                    }

                    OnTreeHit?.Invoke();
                    UpdateVisuals();
                }
            }
        }

        public void TakeHit()
        {
            _currentDamage += 1f;
            _lastHitTime = Time.time;

            if (AudioManager.Instance != null && soundTreeHit != null)
            {
                AudioManager.Instance.Play3DSound(soundTreeHit, transform.position);
            }

            UpdateVisuals();

            StopAllCoroutines();
            StartCoroutine(HitEffect());

            if (_currentDamage >= tapsToCut)
            {
                Die();
            }
        }

        void UpdateVisuals()
        {
            if (healthBar != null)
            {
                if (_currentDamage > 0)
                {
                    if (!healthBar.gameObject.activeSelf) healthBar.gameObject.SetActive(true);
                    healthBar.SetHealth(tapsToCut - _currentDamage, tapsToCut);
                }
                else
                {
                    if (healthBar.gameObject.activeSelf) healthBar.gameObject.SetActive(false);
                }
            }
        }

        IEnumerator HitEffect()
        {
            transform.localScale = _originalScale * 0.9f;
            yield return new WaitForSeconds(0.05f);
            transform.localScale = _originalScale;
        }

        void Die()
        {
            if (AudioManager.Instance != null && soundTreeFall != null)
            {
                AudioManager.Instance.Play3DSound(soundTreeFall, transform.position);
            }

            for (int i = 0; i < woodAmount; i++)
            {
                Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), 0.1f, Random.Range(-0.3f, 0.1f));
                Instantiate(woodPrefab, transform.position + randomOffset, Quaternion.identity);
            }

            OnTreeDestroyed?.Invoke();
            gameObject.SetActive(false);
        }

        public void ApplyMeteorDamage()
        {
            _currentDamage = tapsToCut;
            UpdateVisuals();
            Die();
        }

        public void ResetearArbol()
        {
            _currentDamage = 0f;

            if (healthBar != null)
            {
                healthBar.gameObject.SetActive(false);
                healthBar.SetHealth(tapsToCut, tapsToCut);
            }

            transform.localScale = Vector3.zero;
        }
    }
}
