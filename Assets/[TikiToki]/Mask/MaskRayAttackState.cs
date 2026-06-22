using UnityEngine;
using System.Collections;
using TikiToki.Gameplay.Player;

namespace TikiToki.Gameplay.Boss
{
    public class MaskRayAttackState : MaskState
    {
        private RayAttackPhase _currentPhase;
        private float _elapsedTime;
        private float _firingCounter;

        [Header("Object References")]
        [SerializeField] private ParticleSystem beamParticles;
        [SerializeField] private BoxCollider beamCollider;

        [Header("Laser Adjustments")]
        [SerializeField] private float fireTime = 3f;
        [SerializeField] private float rayRange = 25f;
        [SerializeField] private float beamRadius = 1.5f;
        [SerializeField] private LayerMask playerMask;
        [SerializeField] private float timeToShoot = 1.3f;

        [Header("Jaw Animation Settings")]
        [SerializeField] private float jawOpenDistance = 0.8f;
        [SerializeField] private float jawAnimationSpeed = 5f;
        private Vector3 _jawInitialPosition;
        private Vector3 _jawTargetPosition;

        private bool _isActuallyFiring = false;

        [Header("Audio")]
        [SerializeField] private AudioClip audioQ;

        protected override void StateEnter()
        {
            _elapsedTime = 0f;
            _firingCounter = fireTime;
            _currentPhase = RayAttackPhase.Charging;
            _isActuallyFiring = false;

            ToggleBeam(false);

            if (audioQ != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.Play3DSound(audioQ, machine.transform.position);
            }

            if (machine.jawTransform != null)
            {
                _jawInitialPosition = machine.jawTransform.localPosition;
                _jawTargetPosition = _jawInitialPosition + Vector3.back * (jawOpenDistance * 0.4f);
            }
        }

        protected override void StateUpdate()
        {
            UpdateJawAnimation();

            switch (_currentPhase)
            {
                case RayAttackPhase.Charging:
                case RayAttackPhase.LockedIn:
                    UpdatePreparationPhase();
                    break;

                case RayAttackPhase.Firing:
                    if (_isActuallyFiring)
                    {
                        _firingCounter -= Time.deltaTime;
                        FireBeamTick();

                        if (_firingCounter <= 0)
                            machine.SetState(machine.idleState.Value);
                    }
                    break;
            }
        }

        private void UpdatePreparationPhase()
        {
            _elapsedTime += Time.deltaTime;

            float lockInTime = timeToShoot * 0.7f;

            if (_elapsedTime < lockInTime)
            {
                _currentPhase = RayAttackPhase.Charging;
                machine.MirrorPlayerPosition();
                machine.LookAtPlayer();
            }
            else if (_elapsedTime < timeToShoot)
            {
                if (_currentPhase != RayAttackPhase.LockedIn)
                {
                    _currentPhase = RayAttackPhase.LockedIn;
                    _jawTargetPosition = _jawInitialPosition + Vector3.back * (jawOpenDistance * 0.7f);
                }
            }
            else
            {
                _currentPhase = RayAttackPhase.Firing;
                ActivateBeam();
            }
        }

        private void ActivateBeam()
        {
            if (machine.jawTransform != null)
                _jawTargetPosition = _jawInitialPosition + Vector3.back * jawOpenDistance;

            if (beamCollider != null)
            {
                beamCollider.size = new Vector3(beamRadius * 2, beamRadius * 2, rayRange);
                beamCollider.center = new Vector3(0, 0, rayRange / 2f);
                beamCollider.enabled = true;
            }

            if (beamParticles != null)
            {
                var main = beamParticles.main;
                main.startLifetime = rayRange / main.startSpeed.constant;
                main.simulationSpeed = 15f;
            }

            _isActuallyFiring = true;
            ToggleBeam(true);
        }

        protected override void StateExit()
        {
            _isActuallyFiring = false;
            ToggleBeam(false);

            if (machine.jawTransform != null)
                machine.jawTransform.localPosition = _jawInitialPosition;
        }

        private void ToggleBeam(bool active)
        {
            if (beamParticles != null)
            {
                if (active) beamParticles.Play();
                else
                {
                    beamParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    beamParticles.Clear();
                }
            }
            if (beamCollider != null) beamCollider.enabled = active;
        }

        private void UpdateJawAnimation()
        {
            if (machine.jawTransform != null)
            {
                machine.jawTransform.localPosition = Vector3.Lerp(
                    machine.jawTransform.localPosition,
                    _jawTargetPosition,
                    Time.deltaTime * jawAnimationSpeed
                );
            }
        }

        private void FireBeamTick()
        {
            if (!_isActuallyFiring) return;

            Vector3 origin = beamParticles != null ? beamParticles.transform.position : machine.transform.position;
            Vector3 direction = beamParticles != null ? beamParticles.transform.forward : -machine.transform.forward;

            RaycastHit[] hits = Physics.SphereCastAll(origin, beamRadius, direction, rayRange, playerMask);
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    hit.collider.GetComponent<PlayerHealth>()?.TakeDamage();
                }
            }
        }
    }

    public enum RayAttackPhase { Charging, LockedIn, Firing }
}
