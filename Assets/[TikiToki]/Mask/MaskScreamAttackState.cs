using UnityEngine;

public class MaskScreamAttackState : MaskState
{
    private PlayerMovement playerMovement;
    private ScreamPhase currentPhase;
    private float phaseCounter;

    [Header("Ajustes del Ataque")]
    [SerializeField] private float growlDuration = 1f;
    [SerializeField] private float screamDuration = 2f;
    [SerializeField] private float stunDuration = 3f;

    [Header("Animación de Mandíbula")]
    [SerializeField] private float jawOpenDistance = 1.5f;
    [SerializeField] private float jawAnimationSpeed = 6f;
    private Vector3 jawInitialPosition;
    private Vector3 jawTargetPosition;
    private bool hasStunnedPlayer = false;

    protected override void StateEnter()
    {
        Debug.Log("Entering Scream State");
        playerMovement = machine.PlayerTransform.GetComponent<PlayerMovement>();
        currentPhase = ScreamPhase.Growling;
        phaseCounter = growlDuration;
        hasStunnedPlayer = false;

        // Animar mandíbula - apertura parcial para gruñido
        if (machine.JawTransform != null)
        {
            jawInitialPosition = machine.JawTransform.localPosition;
            jawTargetPosition = jawInitialPosition + Vector3.back * (jawOpenDistance * 0.3f);
        }
    }

    protected override void StateUpdate()
    {
        machine.MirrorPlayerPosition();
        machine.LookAtPlayer();

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
        // Cerrar mandíbula
        if (machine.JawTransform != null)
        {
            machine.JawTransform.localPosition = jawInitialPosition;
        }
    }
}

public enum ScreamPhase { Growling, Screaming }
