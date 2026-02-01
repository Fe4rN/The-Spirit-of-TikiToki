using UnityEngine;

public class MaskScreamAttackState : MaskState
{
    private PlayerMovement playerMovement;
    private SpawnerMaster spawnerMaster;
    private ScreamPhase currentPhase;
    private float phaseCounter;

    [Header("Ajustes del Ataque")]
    [SerializeField] private float growlDuration = 1f;
    [SerializeField] private float screamDuration = 2f;
    [SerializeField] private float stunDuration = 3f;

    [Header("Efectos Visuales")]
    [SerializeField] private ParticleSystem screamParticles; // Arrastra aquí el efecto de distorsión/grito

    [Header("Camera Shake")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float shakeIntensity = 0.5f;
    private Vector3 cameraOriginalPosition;

    [Header("Animación de Mandíbula")]
    [SerializeField] private float jawOpenDistance = 1.5f;
    [SerializeField] private float jawAnimationSpeed = 6f;
    private Vector3 jawInitialPosition;
    private Vector3 jawTargetPosition;
    private bool hasStunnedPlayer = false;


    [Header("Audio")]
    [SerializeField] private AudioClip growlSound; // Antes audioQ (Gruñido inicial)
    [SerializeField] private AudioClip screamSound;

    protected override void StateEnter()
    {
        Debug.Log("Entering Scream State");
        playerMovement = machine.PlayerTransform.GetComponent<PlayerMovement>();
        spawnerMaster = FindFirstObjectByType<SpawnerMaster>();

        currentPhase = ScreamPhase.Growling;
        phaseCounter = growlDuration;
        hasStunnedPlayer = false;

        // Aseguramos que las partículas estén paradas al inicio
        if (screamParticles != null) screamParticles.Stop();

        // Guardar posición original de la cámara
        if (mainCamera != null)
        {
            cameraOriginalPosition = mainCamera.transform.localPosition;
        }

        // Animar mandíbula - apertura parcial para gruñido
        if (machine.JawTransform != null)
        {
            jawInitialPosition = machine.JawTransform.localPosition;
            jawTargetPosition = jawInitialPosition + Vector3.back * (jawOpenDistance * 0.3f);
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

        // Aplicar camera shake durante todo el ataque
        ApplyCameraShake();

        // Animar mandíbula
        if (machine.JawTransform != null)
        {
            machine.JawTransform.localPosition = Vector3.Lerp(
                machine.JawTransform.localPosition,
                jawTargetPosition,
                Time.deltaTime * jawAnimationSpeed
            );
        }

        switch (currentPhase)
        {
            case ScreamPhase.Growling:
                phaseCounter -= Time.deltaTime;

                if (phaseCounter <= 0)
                {
                    currentPhase = ScreamPhase.Screaming;
                    phaseCounter = screamDuration;

                    // Abrir completamente la mandíbula para gritar
                    if (machine.JawTransform != null)
                    {
                        jawTargetPosition = jawInitialPosition + Vector3.back * jawOpenDistance;
                    }

                    if (screamSound != null && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.Play3DSound(screamSound, machine.transform.position);
                    }

                    // --- ACTIVAR EFECTO DE GRITO ---
                    if (screamParticles != null)
                    {
                        screamParticles.Play();
                    }

                    // Regenerar arboles
                    spawnerMaster?.GenerarAlgunosArboles();

                    // TODO: Reproducir sonido de grito
                }
                break;

            case ScreamPhase.Screaming:
                phaseCounter -= Time.deltaTime;

                // Aplicar stun al jugador una sola vez
                if (!hasStunnedPlayer && playerMovement != null)
                {
                    playerMovement.ApplyStun(stunDuration);
                    hasStunnedPlayer = true;
                    Debug.Log($"Player stunned for {stunDuration} seconds!");
                }

                if (phaseCounter <= 0)
                    machine.SetState(machine.idleState.Value);
                break;
        }
    }

    protected override void StateExit()
    {
        // Detener partículas al salir del estado
        if (screamParticles != null)
        {
            screamParticles.Stop();
        }

        // Restaurar posición original de la cámara
        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = cameraOriginalPosition;
        }

        // Cerrar mandíbula
        if (machine.JawTransform != null)
        {
            machine.JawTransform.localPosition = jawInitialPosition;
        }
    }

    private void ApplyCameraShake()
    {
        if (mainCamera == null) return;

        Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
        mainCamera.transform.localPosition = cameraOriginalPosition + randomOffset;
    }
}

public enum ScreamPhase { Growling, Screaming }