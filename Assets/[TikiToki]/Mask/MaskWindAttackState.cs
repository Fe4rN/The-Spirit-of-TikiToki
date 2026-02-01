using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MaskWindAttackState : MaskState
{
    private PlayerMovement playerMovement;
    private SpawnerMaster spawnerMaster;

    private Vector3 windDirection;
    [SerializeField] private float windStrength = 0.7f;
    [SerializeField] private float windDuration = 5f;
    private float windCounter;

    [Header("Configuración de Partículas")]
    [SerializeField] private ParticleSystem mouthParticles; // Arrastra aquí el ParticleSystem que ya tienes orientado
    [SerializeField] private GameObject windEffectPrefab;   // Solo para las ráfagas del mapa
    [SerializeField] private int mapWindCount = 5;
    [SerializeField] private float spawnRadius = 15f;

    private List<ParticleSystem> _mapParticlesList = new List<ParticleSystem>();

    [Header("Animación de Mandíbula")]
    [SerializeField] private float jawOpenDistance = 0.3f;
    [SerializeField] private float jawAnimationSpeed = 5f;
    [SerializeField] private float anticipationDuration = 0.5f;
    private Vector3 jawInitialPosition;
    private Vector3 jawTargetPosition;
    private bool isJawAnimating = false;

    private WindPhase currentPhase;
    private float anticipationCounter;

    [Header("Audio")]
    [SerializeField] private AudioClip windSwooshSound; // El sonido de la ráfaga de viento

    protected override void StateEnter()
    {
        // Failsafe del Player
        if (machine.PlayerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) machine.PlayerTransform = player.transform;
        }

        spawnerMaster = FindFirstObjectByType<SpawnerMaster>();

        playerMovement = machine.PlayerTransform.GetComponent<PlayerMovement>();
        windDirection = ChooseRandomAxis();
        windCounter = windDuration;
        anticipationCounter = anticipationDuration;
        currentPhase = WindPhase.Anticipation;

        // Preparamos las ráfagas del mapa
        PrepareMultipleMapParticles();

        if (machine.JawTransform != null)
        {
            jawInitialPosition = machine.JawTransform.localPosition;
            jawTargetPosition = jawInitialPosition + Vector3.back * (jawOpenDistance * 0.5f);
            isJawAnimating = true;
        }
    }

    protected override void StateUpdate()
    {
        machine.MirrorPlayerPosition();
        machine.LookAtPlayer();

        switch (currentPhase)
        {
            case WindPhase.Anticipation:
                anticipationCounter -= Time.deltaTime;

                if (isJawAnimating && machine.JawTransform != null)
                {
                    machine.JawTransform.localPosition = Vector3.Lerp(
                        machine.JawTransform.localPosition,
                        jawTargetPosition,
                        Time.deltaTime * jawAnimationSpeed
                    );
                }

                if (anticipationCounter <= 0)
                {
                    currentPhase = WindPhase.Attacking;
                    playerMovement.SetWind(windDirection, windStrength);

                    // ACTIVAR VISUALES
                    ActivateWindVisuals();

                    if (windSwooshSound != null && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.Play3DSound(windSwooshSound, machine.transform.position);
                    }

                    if (machine.JawTransform != null)
                    {
                        jawTargetPosition = jawInitialPosition + Vector3.back * jawOpenDistance;
                    }
                }
                break;

            case WindPhase.Attacking:
                if (isJawAnimating && machine.JawTransform != null)
                {
                    machine.JawTransform.localPosition = Vector3.Lerp(
                        machine.JawTransform.localPosition,
                        jawTargetPosition,
                        Time.deltaTime * jawAnimationSpeed
                    );
                }
            
                windCounter -= Time.deltaTime;
                if (windCounter <= 0) machine.SetState(machine.idleState.Value);
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
            Vector3 spawnPos = machine.PlayerTransform.position + randomOffset;

            GameObject mapGo = Instantiate(windEffectPrefab, spawnPos, Quaternion.LookRotation(windDirection));
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
        // 1. Play a la boca (la que ya tienes puesta en el editor)
        if (mouthParticles != null)
        {
            mouthParticles.Play();
        }

        // 2. Play a las del mapa
        foreach (ParticleSystem ps in _mapParticlesList)
        {
            if (ps != null) ps.Play();
        }
    }

    protected override void StateExit()
    {
        AffectMap();
        playerMovement.ClearWind();

        // 1. Parar partículas de la boca
        if (mouthParticles != null)
        {
            mouthParticles.Stop();
        }

        // 2. Parar y limpiar ráfagas del mapa
        foreach (ParticleSystem ps in _mapParticlesList)
        {
            if (ps != null)
            {
                ps.Stop();
                Destroy(ps.gameObject, 2f);
            }
        }
        _mapParticlesList.Clear();

        if (machine.JawTransform != null)
        {
            machine.JawTransform.localPosition = jawInitialPosition;
            isJawAnimating = false;
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
        // El viento trae hojas nuevas al mapa
        spawnerMaster?.GenerarAlgunasHojas();
    }

    private void ExtinguishBonfires()
    {
        // Buscamos todas las hogueras de la escena
        Hoguera[] todasLasHogueras = FindObjectsByType<Hoguera>(FindObjectsSortMode.None);

        foreach (Hoguera h in todasLasHogueras)
        {
            // Si está encendida, hay un 20% de probabilidad de apagarla
            if (h.estaEncendida)
            {
                if (Random.value <= 0.20f) // 0.20 = 20%
                {
                    h.ApagarHoguera();
                }
            }
        }
    }
}

public enum WindPhase { Anticipation, Attacking }