using UnityEngine;

public class MaskMeteorAttackState : MaskState
{
    [Header("Ajustes del Ataque")]
    [SerializeField] private float anticipationDuration = 1.5f;
    [SerializeField] private float attackDuration = 3f;
    private float attackCounter;
    private MeteorPhase currentPhase;

    [Header("Animación de Mandíbula")]
    [SerializeField] private float jawOpenDistance = 1.2f;
    [SerializeField] private float jawChompSpeed = 8f;
    [SerializeField] private int chompCount = 3;
    private Vector3 jawInitialPosition;
    private Vector3 jawTargetPosition;
    private float chompTimer;
    private int currentChomps;
    private bool isOpening;

    protected override void StateEnter()
    {
        Debug.Log("Entering Meteor State");
        attackCounter = anticipationDuration;
        currentPhase = MeteorPhase.Anticipation;
        currentChomps = 0;
        isOpening = true;
        chompTimer = 0;

        // Inicializar posición de mandíbula
        if (machine.JawTransform != null)
        {
            jawInitialPosition = machine.JawTransform.localPosition;
            jawTargetPosition = jawInitialPosition + Vector3.back * jawOpenDistance;
        }
    }

    protected override void StateUpdate()
    {
        machine.MirrorPlayerPosition();
        machine.LookAtPlayer();

        switch (currentPhase)
        {
            case MeteorPhase.Anticipation:
                attackCounter -= Time.deltaTime;

                // Hacer chomping (abrir y cerrar 3 veces)
                if (currentChomps < chompCount && machine.JawTransform != null)
                {
                    machine.JawTransform.localPosition = Vector3.Lerp(
                        machine.JawTransform.localPosition,
                        jawTargetPosition,
                        Time.deltaTime * jawChompSpeed
                    );

                    // Detectar cuando alcanzamos el objetivo
                    float distance = Vector3.Distance(machine.JawTransform.localPosition, jawTargetPosition);
                    if (distance < 0.05f)
                    {
                        if (isOpening)
                        {
                            // Cambiar a cerrar
                            isOpening = false;
                            jawTargetPosition = jawInitialPosition;
                        }
                        else
                        {
                            // Cambiar a abrir y contar el chomp
                            isOpening = true;
                            currentChomps++;
                            if (currentChomps < chompCount)
                            {
                                jawTargetPosition = jawInitialPosition + Vector3.back * jawOpenDistance;
                            }
                        }
                    }
                }

                if (attackCounter <= 0)
                {
                    currentPhase = MeteorPhase.Attacking;
                    attackCounter = attackDuration;

                    // Asegurar que la mandíbula esté cerrada
                    if (machine.JawTransform != null)
                    {
                        machine.JawTransform.localPosition = jawInitialPosition;
                    }

                    // TODO: Iniciar lluvia de meteoritos
                }
                break;

            case MeteorPhase.Attacking:
                attackCounter -= Time.deltaTime;

                // TODO: Actualizar lógica de meteoritos

                if (attackCounter <= 0)
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

    private void chooseRandomPoint()
    {
        // TODO: Implementar selección de punto aleatorio para meteoritos
    }
}

public enum MeteorPhase { Anticipation, Attacking }
