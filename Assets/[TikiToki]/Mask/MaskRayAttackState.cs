using UnityEngine;
using System.Collections;

public class MaskRayAttackState : MaskState
{
    private RayAttackPhase currentPhase;
    private float elapsedTime; // Cronómetro que empieza en 0
    private float FiringCounter;

    [Header("Referencias de Objetos")]
    [SerializeField] private ParticleSystem beamParticles;
    [SerializeField] private BoxCollider beamCollider;

    [Header("Ajustes del Laser")]
    [SerializeField] private float fireTime = 3f;
    [SerializeField] private float rayRange = 25f;
    [SerializeField] private float beamRadius = 1.5f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float timeToShoot = 1.3f; // El momento exacto del disparo

    [Header("Animación de Mandíbula")]
    [SerializeField] private float jawOpenDistance = 0.8f;
    [SerializeField] private float jawAnimationSpeed = 5f;
    private Vector3 jawInitialPosition;
    private Vector3 jawTargetPosition;

    private bool _isActuallyFiring = false;
    [Header("Audio")]
    [SerializeField] private AudioClip chargeSound; // Sonido mientras carga
    [SerializeField] private AudioClip beamSound;   // Sonido al disparar el láser
    private AudioSource audioSource;

    protected override void StateEnter()
    {

        elapsedTime = 0f; // Reset del cronómetro
        FiringCounter = fireTime;
        currentPhase = RayAttackPhase.Charging;
        _isActuallyFiring = false;

        ToggleBeam(false);

        // 1. EL SONIDO Y LA BOCA EMPIEZAN JUNTOS
        if (chargeSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.Play3DSound(chargeSound, machine.transform.position);
        }

        if (machine.JawTransform != null)
        {
            jawInitialPosition = machine.JawTransform.localPosition;
            // Apuntamos a que esté abierta para cuando llegue el disparo
            jawTargetPosition = jawInitialPosition + Vector3.back * jawOpenDistance;
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

        // Decidimos cuándo dejar de trackear al jugador (un poco antes del disparo)
        // Por ejemplo, al segundo 0.9 deja de seguirle para "apuntar"
        float lockInTime = timeToShoot * 0.7f;

        if (elapsedTime < lockInTime)
        {
            currentPhase = RayAttackPhase.Charging;
            machine.MirrorPlayerPosition();
            machine.LookAtPlayer();
        }
        else if (elapsedTime < timeToShoot)
        {
            // Entre 0.9s y 1.3s se queda quieto (LockedIn)
            currentPhase = RayAttackPhase.LockedIn;
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
        if (beamCollider != null)
        {
            beamCollider.size = new Vector3(beamRadius * 2, beamRadius * 2, rayRange);
            beamCollider.center = new Vector3(0, 0, rayRange / 2f);
            beamCollider.enabled = true;
        }

        if (beamSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.Play3DSound(beamSound, machine.transform.position);
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