using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TikiToki.Gameplay.Boss
{
    public class MaskMeteorAttackState : MaskState
    {
        [Header("Attack Settings")]
        [SerializeField] private float anticipationDuration = 1.5f;
        [SerializeField] private float attackDuration = 3f;
        private float _attackCounter;
        private MeteorPhase _currentPhase;

        [Header("Jaw Animation")]
        [SerializeField] private float jawOpenDistance = 1.2f;
        [SerializeField] private float jawChompSpeed = 8f;
        [SerializeField] private int chompCount = 3;
        private Vector3 _jawInitialPosition;
        private Vector3 _jawTargetPosition;
        private float _chompTimer;
        private int _currentChomps;
        private bool _isOpening;

        [Header("Meteor Configuration")]
        [SerializeField] private GameObject[] meteorPrefabs;
        [SerializeField] private Transform groundReference;
        [SerializeField] private int minMeteors = 10;
        [SerializeField] private int maxMeteors = 20;
        [SerializeField] private float meteorHeight = 10f;
        [SerializeField] private float damageRadius = 1f;
        [SerializeField] private float warningDuration = 2f;
        [SerializeField] private Color warningColor = new Color(1f, 0f, 0f, 0.6f);
        [SerializeField] private float warningBlinkSpeed = 6f;
        [SerializeField] private Color warningEmissionColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private float warningEmissionIntensity = 1.5f;

        [Header("Sounds")]
        [SerializeField] private AudioClip maskMeteorSound;
        [SerializeField] private AudioClip[] meteorImpactSounds;

        private List<GameObject> _activeWarnings = new List<GameObject>();
        private bool _meteorsSpawned = false;

        protected override void StateEnter()
        {
            Debug.Log("Entering Meteor State");
            _attackCounter = anticipationDuration;
            _currentPhase = MeteorPhase.Anticipation;
            _currentChomps = 0;
            _isOpening = true;
            _chompTimer = 0;

            if (machine.jawTransform != null)
            {
                _jawInitialPosition = machine.jawTransform.localPosition;
                _jawTargetPosition = _jawInitialPosition + Vector3.back * jawOpenDistance;
            }

            if (maskMeteorSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.Play3DSound(maskMeteorSound, machine.transform.position);
            }
        }

        protected override void StateUpdate()
        {
            machine.MirrorPlayerPosition();
            machine.LookAtPlayer();

            switch (_currentPhase)
            {
                case MeteorPhase.Anticipation:
                    _attackCounter -= Time.deltaTime;

                    if (_currentChomps < chompCount && machine.jawTransform != null)
                    {
                        machine.jawTransform.localPosition = Vector3.Lerp(
                            machine.jawTransform.localPosition,
                            _jawTargetPosition,
                            Time.deltaTime * jawChompSpeed
                        );

                        float distance = Vector3.Distance(machine.jawTransform.localPosition, _jawTargetPosition);
                        if (distance < 0.05f)
                        {
                            if (_isOpening)
                            {
                                _isOpening = false;
                                _jawTargetPosition = _jawInitialPosition;
                            }
                            else
                            {
                                _isOpening = true;
                                _currentChomps++;
                                if (_currentChomps < chompCount)
                                {
                                    _jawTargetPosition = _jawInitialPosition + Vector3.back * jawOpenDistance;
                                }
                            }
                        }
                    }

                    if (_attackCounter <= 0)
                    {
                        _currentPhase = MeteorPhase.Attacking;
                        _attackCounter = attackDuration;

                        if (machine.jawTransform != null)
                        {
                            machine.jawTransform.localPosition = _jawInitialPosition;
                        }

                        StartMeteorRain();
                    }
                    break;

                case MeteorPhase.Attacking:
                    _attackCounter -= Time.deltaTime;

                    if (_attackCounter <= 0)
                        machine.SetState(machine.idleState.Value);
                    break;
            }
        }

        protected override void StateExit()
        {
            if (machine.jawTransform != null)
            {
                machine.jawTransform.localPosition = _jawInitialPosition;
            }

            CleanupWarnings();
            _meteorsSpawned = false;
        }

        private void StartMeteorRain()
        {
            if (meteorPrefabs == null || meteorPrefabs.Length == 0)
            {
                Debug.LogError("No meteor prefabs assigned!");
                return;
            }

            if (groundReference == null)
            {
                Debug.LogError("Ground reference not assigned!");
                return;
            }

            int meteorCount = Random.Range(minMeteors, maxMeteors + 1);

            for (int i = 0; i < meteorCount; i++)
            {
                float delay = Random.Range(0f, attackDuration - warningDuration);
                StartCoroutine(SpawnMeteorWithWarning(delay));
            }

            _meteorsSpawned = true;
        }

        private IEnumerator SpawnMeteorWithWarning(float initialDelay)
        {
            yield return new WaitForSeconds(initialDelay);

            Vector3 targetPosition = GetRandomPositionOnGround();

            GameObject warning = CreateWarningIndicator(targetPosition);
            _activeWarnings.Add(warning);

            yield return new WaitForSeconds(warningDuration);

            if (warning != null)
            {
                _activeWarnings.Remove(warning);
                Destroy(warning);
            }

            SpawnMeteor(targetPosition);
        }

        private Vector3 GetRandomPositionOnGround()
        {
            if (groundReference == null) return Vector3.zero;

            Renderer groundRenderer = groundReference.GetComponent<Renderer>();
            if (groundRenderer != null)
            {
                Bounds bounds = groundRenderer.bounds;

                float randomX = Random.Range(bounds.min.x, bounds.max.x);
                float randomZ = Random.Range(bounds.min.z, bounds.max.z);

                return new Vector3(randomX, bounds.max.y, randomZ);
            }

            return groundReference.position + new Vector3(
                Random.Range(-10f, 10f),
                0f,
                Random.Range(-10f, 10f)
            );
        }

        private GameObject CreateWarningIndicator(Vector3 position)
        {
            GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            warning.name = "MeteorWarning";

            warning.transform.position = position + Vector3.up * 0.05f;
            warning.transform.localScale = new Vector3(damageRadius * 2, 0.01f, damageRadius * 2);

            Renderer renderer = warning.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = warningColor;
            renderer.material.EnableKeyword("_EMISSION");
            Color emission = warningEmissionColor * warningEmissionIntensity;
            renderer.material.SetColor("_EmissionColor", emission);
            renderer.material.SetFloat("_Mode", 3);
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetInt("_ZWrite", 0);
            renderer.material.DisableKeyword("_ALPHATEST_ON");
            renderer.material.EnableKeyword("_ALPHABLEND_ON");
            renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderer.material.renderQueue = 3000;

            WarningBlink blink = warning.AddComponent<WarningBlink>();
            blink.blinkSpeed = warningBlinkSpeed;
            blink.baseColor = warningColor;

            Collider collider = warning.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            return warning;
        }

        private void SpawnMeteor(Vector3 targetPosition)
        {
            GameObject meteorPrefab = meteorPrefabs[Random.Range(0, meteorPrefabs.Length)];
            Vector3 spawnPosition = targetPosition + Vector3.up * meteorHeight;
            GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);

            Rigidbody rb = meteor.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = meteor.AddComponent<Rigidbody>();
            }
            rb.useGravity = true;

            Meteor meteorScript = meteor.GetComponent<Meteor>();
            if (meteorScript == null)
            {
                meteorScript = meteor.AddComponent<Meteor>();
            }
            meteorScript.Initialize(damageRadius, meteorImpactSounds);
        }

        private void CleanupWarnings()
        {
            foreach (GameObject warning in _activeWarnings)
            {
                if (warning != null)
                    Destroy(warning);
            }
            _activeWarnings.Clear();
        }
    }

    public class WarningBlink : MonoBehaviour
    {
        public float blinkSpeed = 6f;
        public Color baseColor = new Color(1f, 0f, 0f, 0.6f);

        private Renderer _cachedRenderer;

        private void Awake()
        {
            _cachedRenderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            if (_cachedRenderer == null) return;

            float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;
            Color c = baseColor;
            c.a = Mathf.Lerp(0.1f, baseColor.a, t);
            _cachedRenderer.material.color = c;
        }
    }

    public enum MeteorPhase { Anticipation, Attacking }
}
