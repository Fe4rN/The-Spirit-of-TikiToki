using Mono.Cecil;
using UnityEngine;

public class MaskRayAttackState : MaskState
{
    private RayAttackPhase currentPhase;

    private float ChargingCounter;
    private float LockedInCounter;
    protected override void StateEnter()
    {
        ChargingCounter = 0;
        LockedInCounter = 2;
        currentPhase = RayAttackPhase.Charging;
    }

    protected override void StateUpdate()
    {
        if (currentPhase == RayAttackPhase.Charging)
        {
            ChargingCounter -= Time.deltaTime;
            machine.MirrorPlayerPosition();
            machine.LookAtPlayer();

            if(ChargingCounter <= 0) currentPhase = RayAttackPhase.LockedIn;
        }

        if (currentPhase == RayAttackPhase.LockedIn)
        {
            LockedInCounter -= Time.deltaTime;
            if(LockedInCounter <= 0) fireRay();
        }
    }

    private void fireRay()
    {
        
    }
}

public enum RayAttackPhase { Charging, LockedIn}