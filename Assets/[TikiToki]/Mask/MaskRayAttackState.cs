using UnityEngine;

public class MaskRayAttackState : MaskState
{
    private RayAttackPhase currentPhase;
    private float ChargingCounter;
    private float LockedInCounter;
    private float FiringCounter;


    [Header("Ajustes del Laser")]
    [SerializeField] private float fireTime = 3f;
    [SerializeField] private float rayRange = 25f;
    [SerializeField] private float beamRadius = 1.5f;
    [SerializeField] private LayerMask playerMask;

    [Header("Animación de Mandíbula")]
    [SerializeField] private float jawOpenDistance = 0.8f;
    [SerializeField] private float jawAnimationSpeed = 5f;
    private Vector3 jawInitialPosition;
    private Vector3 jawTargetPosition;

    protected override void StateEnter()
    {
        Debug.Log("Entering RayState");
        ChargingCounter = 2f;
        LockedInCounter = 1f;
        FiringCounter = fireTime;
        currentPhase = RayAttackPhase.Charging;

        // Animar mandíbula - apertura parcial durante carga
        if (machine.JawTransform != null)
        {
            jawInitialPosition = machine.JawTransform.localPosition;
            jawTargetPosition = jawInitialPosition + Vector3.back * (jawOpenDistance * 0.4f);
        }
    }

    protected override void StateUpdate()
    {
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
            case RayAttackPhase.Charging:
                ChargingCounter -= Time.deltaTime;
                machine.MirrorPlayerPosition();
                machine.LookAtPlayer();

                if (ChargingCounter <= 0)
                {
                    currentPhase = RayAttackPhase.LockedIn;
                    // Abrir más la mandíbula cuando está bloqueado
                    if (machine.JawTransform != null)
                    {
                        jawTargetPosition = jawInitialPosition + Vector3.back * (jawOpenDistance * 0.7f);
                    }
                }
                break;

            case RayAttackPhase.LockedIn:
                LockedInCounter -= Time.deltaTime;

                if (LockedInCounter <= 0)
                {
                    currentPhase = RayAttackPhase.Firing;
                    // Abrir completamente la mandíbula al disparar
                    if (machine.JawTransform != null)
                    {
                        jawTargetPosition = jawInitialPosition + Vector3.back * jawOpenDistance;
                    }
                }
                break;

            case RayAttackPhase.Firing:
                FiringCounter -= Time.deltaTime;

                FireBeamTick();

                if (FiringCounter <= 0)
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

    private void FireBeamTick()
    {
        Vector3 origin = machine.transform.position;

        // Flip direction
        Vector3 direction = -machine.transform.forward;

        Vector3 point1 = origin + Vector3.up * beamRadius;
        Vector3 point2 = origin - Vector3.up * beamRadius;

        RaycastHit[] hits = Physics.CapsuleCastAll(
            point1,
            point2,
            beamRadius,
            direction,
            rayRange,
            playerMask,
            QueryTriggerInteraction.Ignore
        );

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<PlayerHealth>().TakeDamage();
            }
        }
    }

}

public enum RayAttackPhase { Charging, LockedIn, Firing }