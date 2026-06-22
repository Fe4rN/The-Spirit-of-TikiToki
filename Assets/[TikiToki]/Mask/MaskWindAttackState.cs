using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TikiToki.Gameplay.Player;
using TikiToki.Gameplay.Environment;

namespace TikiToki.Gameplay.Boss
{
    public class MaskWindAttackState : MaskState
    {
        private PlayerMovement _playerMovement;
        private SpawnerMaster _spawnerMaster;

        private Vector3 _windDirection;
        [SerializeField] private float windStrength = 0.7f;
        [SerializeField] private float windDuration = 5f;
        private float _windCounter;

        [Header("Particle Settings")]
        [SerializeField] private ParticleSystem mouthParticles;
        [SerializeField] private GameObject windEffectPrefab;
        [SerializeField] private int mapWindCount = 5;
        [SerializeField] private float spawnRadius = 15f;

        private List<ParticleSystem> _mapParticlesList = new List<ParticleSystem>();

        [Header("Jaw Animation")]
        [SerializeField] private float jawOpenDistance = 0.3f;
        [SerializeField] private float jawAnimationSpeed = 5f;
        [SerializeField] private float anticipationDuration = 0.5f;
        private Vector3 _jawInitialPosition;
        private Vector3 _jawTargetPosition;
        private bool _isJawAnimating = false;

        private WindPhase _currentPhase;
        private float _anticipationCounter;

        [Header("Audio")]
        [SerializeField] private AudioClip windSwooshSound;

        protected override void StateEnter()
        {
            if (machine.playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) machine.playerTransform = player.transform;
            }

            _spawnerMaster = FindFirstObjectByType<SpawnerMaster>();
            _playerMovement = machine.playerTransform.GetComponent<PlayerMovement>();
            _windDirection = ChooseRandomAxis();
            _windCounter = windDuration;
            _anticipationCounter = anticipationDuration;
            _currentPhase = WindPhase.Anticipation;

            PrepareMultipleMapParticles();

            if (machine.jawTransform != null)
            {
                _jawInitialPosition = machine.jawTransform.localPosition;
                _jawTargetPosition = _jawInitialPosition + Vector3.back * (jawOpenDistance * 0.5f);
                _isJawAnimating = true;
            }
        }

        protected override void StateUpdate()
        {
            machine.MirrorPlayerPosition();
            machine.LookAtPlayer();

            switch (_currentPhase)
            {
                case WindPhase.Anticipation:
                    _anticipationCounter -= Time.deltaTime;

                    if (_isJawAnimating && machine.jawTransform != null)
                    {
                        machine.jawTransform.localPosition = Vector3.Lerp(
                            machine.jawTransform.localPosition,
                            _jawTargetPosition,
                            Time.deltaTime * jawAnimationSpeed
                        );
                    }

                    if (_anticipationCounter <= 0)
                    {
                        _currentPhase = WindPhase.Attacking;
                        _playerMovement.SetWind(_windDirection, windStrength);

                        ActivateWindVisuals();

                        if (windSwooshSound != null && AudioManager.Instance != null)
                        {
                            AudioManager.Instance.Play3DSound(windSwooshSound, machine.transform.position);
                        }

                        if (machine.jawTransform != null)
                        {
                            _jawTargetPosition = _jawInitialPosition + Vector3.back * jawOpenDistance;
                        }
                    }
                    break;

                case WindPhase.Attacking:
                    if (_isJawAnimating && machine.jawTransform != null)
                    {
                        machine.jawTransform.localPosition = Vector3.Lerp(
                            machine.jawTransform.localPosition,
                            _jawTargetPosition,
                            Time.deltaTime * jawAnimationSpeed
                        );
                    }

                    _windCounter -= Time.deltaTime;
                    if (_windCounter <= 0) machine.SetState(machine.idleState.Value);
                    break;
            }
        }

        private void PrepareMultipleMapParticles()
        {
            if (windEffectPrefab == null) return;
            _mapParticlesList.Clear();

            for (int i = 0; i < mapWindCount; i++)
            {
                Vector3 randomOffset = new Vector3(Random.Range(-spawnRadius, spawnRadius), 2f, Random.Range(-spawnRadius, spawnRadius));
                Vector3 spawnPos = machine.playerTransform.position + randomOffset;

                GameObject mapGo = Instantiate(windEffectPrefab, spawnPos, Quaternion.LookRotation(_windDirection));
                ParticleSystem ps = mapGo.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Stop();
                    _mapParticlesList.Add(ps);
                }
            }
        }

        private void ActivateWindVisuals()
        {
            if (mouthParticles != null)
            {
                mouthParticles.Play();
            }

            foreach (ParticleSystem ps in _mapParticlesList)
            {
                if (ps != null) ps.Play();
            }
        }

        protected override void StateExit()
        {
            AffectMap();
            _playerMovement.ClearWind();

            if (mouthParticles != null)
            {
                mouthParticles.Stop();
            }

            foreach (ParticleSystem ps in _mapParticlesList)
            {
                if (ps != null)
                {
                    ps.Stop();
                    Destroy(ps.gameObject, 2f);
                }
            }
            _mapParticlesList.Clear();

            if (machine.jawTransform != null)
            {
                machine.jawTransform.localPosition = _jawInitialPosition;
                _isJawAnimating = false;
            }
        }

        private Vector3 ChooseRandomAxis()
        {
            int choice = Random.Range(0, 4);
            switch (choice)
            {
                case 0: return Vector3.right;
                case 1: return Vector3.left;
                case 2: return Vector3.forward;
                default: return Vector3.back;
            }
        }

        private void AffectMap()
        {
            ReplenishLeaves();
            ExtinguishBonfires();
        }

        private void ReplenishLeaves()
        {
            if (_spawnerMaster != null)
            {
                _spawnerMaster.GenerateSomeLeaves();
            }
        }

        private void ExtinguishBonfires()
        {
            Bonfire[] allBonfires = FindObjectsByType<Bonfire>(FindObjectsSortMode.None);

            foreach (Bonfire b in allBonfires)
            {
                if (b.isLit)
                {
                    if (Random.value <= 0.40f)
                    {
                        b.Extinguish();
                    }
                }
            }
        }
    }

    public enum WindPhase { Anticipation, Attacking }
}
