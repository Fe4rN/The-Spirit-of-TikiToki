using UnityEngine;

public class MaskWindAttackState : MaskState
{
    private PlayerMovement playerMovement;
    private Vector3 windDirection;
    [SerializeField] private float windStrength = 0.7f;
    [SerializeField] private float windDuration = 5f;
    private float windCounter;

    [Header("Animación de Mandíbula")]
    [SerializeField] private float jawOpenDistance = 0.3f;
    [SerializeField] private float jawAnimationSpeed = 5f;
    [SerializeField] private float anticipationDuration = 0.5f;
    private Vector3 jawInitialPosition;
    private Vector3 jawTargetPosition;
    private bool isJawAnimating = false;

    private WindPhase currentPhase;
    private float anticipationCounter;

    protected override void StateEnter()
    {
        Debug.Log("Entering Wind State");
        playerMovement = machine.PlayerTransform.GetComponent<PlayerMovement>();
        windDirection = ChooseRandomAxis();
        windCounter = windDuration;
        anticipationCounter = anticipationDuration;
        currentPhase = WindPhase.Anticipation;

        // Animar mandíbula - apertura parcial para anticipación
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

                // Animar mandíbula a posición de anticipación
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

                    // Abrir completamente la mandíbula
                    if (machine.JawTransform != null)
                    {
                        jawTargetPosition = jawInitialPosition + Vector3.back * jawOpenDistance;
                    }
                }
                break;

            case WindPhase.Attacking:
                // Animar mandíbula a posición completamente abierta
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

    protected override void StateExit()
    {
        AffectMap();
        playerMovement.ClearWind();

        // Cerrar mandíbula
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

    private void ReplenishLeaves() { }
    private void ExtinguishBonfires() { }

}

public enum WindPhase { Anticipation, Attacking }
