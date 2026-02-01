using UnityEngine;
using System.Collections;

public class MaskRayAttackState : MaskState
{
    private RayAttackPhase currentPhase;
    private float elapsedTime;
    private float FiringCounter;

    [Header("Referencias de Objetos")]
    [SerializeField] private ParticleSystem beamParticles;
    [SerializeField] private BoxCollider beamCollider;

    [Header("Ajustes del Laser")]
    [SerializeField] private float fireTime = 3f;
    [SerializeField] private float rayRange = 25f;
    [SerializeField] private float beamRadius = 1.5f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float timeToShoot = 1.3f;

    [Header("Animación de Mandíbula")]
    [SerializeField] private float jawOpenDistance = 0.8f;
    [SerializeField] private float jawAnimationSpeed = 5f;
    private Vector3 jawInitialPosition;
    private Vector3 jawTargetPosition;

    private bool _isActuallyFiring = false;
    [Header("Audio")]
    [SerializeField] private AudioClip audioQ;

    protected override void StateEnter()
    {
        elapsedTime = 0f;
        FiringCounter = fireTime;
        currentPhase = RayAttackPhase.Charging;
        _isActuallyFiring = false;

        ToggleBeam(false);

        // 1. EL SONIDO EMPIEZA AQUÍ (Desde el inicio de la carga)
        if (audioQ != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.Play3DSound(audioQ, machine.transform.position);
        }

        if (machine.JawTransform != null)
        {
            jawInitialPosition = machine.JawTransform.localPosition;
            // NIVEL 1: Apertura inicial (40%)
            jawTargetPosition = jawInitialPosition + Vector3.back * (jawOpenDistance * 0.4f);
        }
    }

    protected override void StateUpdate()
    {
        UpdateJawAnimation();

        switch (currentPhase)
        {
            case RayAttackPhase.Charging:
            case RayAttackPhase.LockedIn:
                UpdatePreparationPhase();
                break;

            case RayAttackPhase.Firing:
                if (_isActuallyFiring)
                {
                    FiringCounter -= Time.deltaTime;
                    FireBeamTick();

                    if (FiringCounter <= 0)
                        machine.SetState(machine.idleState.Value);
                }
                break;
        }
    }

    private void UpdatePreparationPhase()
    {
        elapsedTime += Time.deltaTime;

        float lockInTime = timeToShoot * 0.7f;

        if (elapsedTime < lockInTime)
        {
            currentPhase = RayAttackPhase.Charging;
            machine.MirrorPlayerPosition();
            machine.LookAtPlayer();
            // Mantener el objetivo en el 40%
        }
        else if (elapsedTime < timeToShoot)
        {
            // NIVEL 2: Bloqueado (70% de apertura)
            if (currentPhase != RayAttackPhase.LockedIn)
            {
                currentPhase = RayAttackPhase.LockedIn;
                jawTargetPosition = jawInitialPosition + Vector3.back * (jawOpenDistance * 0.7f);
            }
        }
        else
        {
            // ¡SEGUNDO 1.3! DISPARO
            currentPhase = RayAttackPhase.Firing;
            ActivateBeam();
        }
    }

    private void ActivateBeam()
    {
        // NIVEL 3: Apertura total al disparar (100%)
        if (machine.JawTransform != null)
            jawTargetPosition = jawInitialPosition + Vector3.back * jawOpenDistance;

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

        if (machine.JawTransform != null)
            machine.JawTransform.localPosition = jawInitialPosition;
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
        if (machine.JawTransform != null)
        {
            machine.JawTransform.localPosition = Vector3.Lerp(
                machine.JawTransform.localPosition,
                jawTargetPosition,
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