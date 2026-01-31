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
    [SerializeField] private LayerMask hitMask;

    protected override void StateEnter()
    {
        Debug.Log("Entering RayState");
        ChargingCounter = 2f;
        LockedInCounter = 1f;
        FiringCounter = fireTime;
        currentPhase = RayAttackPhase.Charging;
    }

    protected override void StateUpdate()
    {
        switch (currentPhase)
        {
            case RayAttackPhase.Charging:
                ChargingCounter -= Time.deltaTime;
                machine.MirrorPlayerPosition();
                machine.LookAtPlayer();

                if (ChargingCounter <= 0)
                    currentPhase = RayAttackPhase.LockedIn;
                break;

            case RayAttackPhase.LockedIn:
                LockedInCounter -= Time.deltaTime;

                if (LockedInCounter <= 0)
                    currentPhase = RayAttackPhase.Firing;
                break;

            case RayAttackPhase.Firing:
                FiringCounter -= Time.deltaTime;

                FireBeamTick();

                if (FiringCounter <= 0)
                    machine.SetState(machine.idleState.Value);
                break;
        }
    }

    private void FireBeamTick()
    {
        Vector3 origin = machine.transform.position;
        Vector3 direction = machine.transform.forward;

        Vector3 point1 = origin + Vector3.up * beamRadius;
        Vector3 point2 = origin - Vector3.up * beamRadius;

        RaycastHit[] hits = Physics.CapsuleCastAll(
            point1,
            point2,
            beamRadius,
            direction,
            rayRange,
            hitMask
        );

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Player taking beam damage");
                //TODO - Añadir daño real
            }
        }
    }
}

public enum RayAttackPhase { Charging, LockedIn, Firing }