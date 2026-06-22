using UnityEngine;
using TikiToki.Gameplay.Player;
using TikiToki.Gameplay.Environment;

namespace TikiToki.Gameplay.Boss
{
    public class MaskScreamAttackState : MaskState
    {
        private PlayerMovement _playerMovement;
        private SpawnerMaster _spawnerMaster;
        private ScreamPhase _currentPhase;
        private float _phaseCounter;

        [Header("Attack Settings")]
        [SerializeField] private float growlDuration = 1f;
        [SerializeField] private float screamDuration = 2f;
        [SerializeField] private float stunDuration = 3f;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem screamParticles;

        [Header("Camera Shake")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float shakeIntensity = 0.5f;
        private Vector3 _cameraOriginalPosition;

        [Header("Jaw Animation")]
        [SerializeField] private float jawOpenDistance = 1.5f;
        [SerializeField] private float jawAnimationSpeed = 6f;
        private Vector3 _jawInitialPosition;
        private Vector3 _jawTargetPosition;
        private bool _hasStunnedPlayer = false;

        [Header("Audio")]
        [SerializeField] private AudioClip growlSound;
        [SerializeField] private AudioClip screamSound;

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        protected override void StateEnter()
        {
            Debug.Log("Entering Scream State");
            _playerMovement = machine.playerTransform.GetComponent<PlayerMovement>();
            _spawnerMaster = FindFirstObjectByType<SpawnerMaster>();
            mainCamera = Camera.main;

            _currentPhase = ScreamPhase.Growling;
            _phaseCounter = growlDuration;
            _hasStunnedPlayer = false;

            if (screamParticles != null) screamParticles.Stop();

            if (mainCamera != null)
            {
                _cameraOriginalPosition = mainCamera.transform.localPosition;
            }

            if (machine.jawTransform != null)
            {
                _jawInitialPosition = machine.jawTransform.localPosition;
                _jawTargetPosition = _jawInitialPosition + Vector3.back * (jawOpenDistance * 0.3f);
            }

            if (growlSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.Play3DSound(growlSound, machine.transform.position);
            }
        }

        protected override void StateUpdate()
        {
            machine.MirrorPlayerPosition();
            machine.LookAtPlayer();

            ApplyCameraShake();

            if (machine.jawTransform != null)
            {
                machine.jawTransform.localPosition = Vector3.Lerp(
                    machine.jawTransform.localPosition,
                    _jawTargetPosition,
                    Time.deltaTime * jawAnimationSpeed
                );
            }

            switch (_currentPhase)
            {
                case ScreamPhase.Growling:
                    _phaseCounter -= Time.deltaTime;

                    if (_phaseCounter <= 0)
                    {
                        _currentPhase = ScreamPhase.Screaming;
                        _phaseCounter = screamDuration;

                        if (machine.jawTransform != null)
                        {
                            _jawTargetPosition = _jawInitialPosition + Vector3.back * jawOpenDistance;
                        }

                        if (screamSound != null && AudioManager.Instance != null)
                        {
                            AudioManager.Instance.Play3DSound(screamSound, machine.transform.position);
                        }

                        if (screamParticles != null)
                        {
                            screamParticles.Play();
                        }

                        if (_spawnerMaster != null)
                        {
                            _spawnerMaster.GenerateSomeTrees();
                        }
                    }
                    break;

                case ScreamPhase.Screaming:
                    _phaseCounter -= Time.deltaTime;

                    if (!_hasStunnedPlayer && _playerMovement != null)
                    {
                        _playerMovement.ApplyStun(stunDuration);
                        _hasStunnedPlayer = true;
                        Debug.Log($"Player stunned for {stunDuration} seconds!");
                    }

                    if (_phaseCounter <= 0)
                        machine.SetState(machine.idleState.Value);
                    break;
            }
        }

        protected override void StateExit()
        {
            if (screamParticles != null)
            {
                screamParticles.Stop();
            }

            if (mainCamera != null)
            {
                mainCamera.transform.localPosition = _cameraOriginalPosition;
            }

            if (machine.jawTransform != null)
            {
                machine.jawTransform.localPosition = _jawInitialPosition;
            }
        }

        private void ApplyCameraShake()
        {
            if (mainCamera == null) return;

            Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
            mainCamera.transform.localPosition = _cameraOriginalPosition + randomOffset;
        }
    }

    public enum ScreamPhase { Growling, Screaming }
}