using UnityEngine;

public class MaskWindAttackState : MaskState
{
    private PlayerMovement playerMovement;
    private Vector3 windDirection;
    [SerializeField] private float windStrength = 0.7f;
    [SerializeField] private float windDuration = 5f;
    private float windCounter;

    protected override void StateEnter()
    {
        Debug.Log("Entering Wind State");
        playerMovement = machine.PlayerTransform.GetComponent<PlayerMovement>();
        windDirection = ChooseRandomAxis();
        windCounter = windDuration;

        playerMovement.SetWind(windDirection, windStrength);
    }

    protected override void StateUpdate()
    {
        windCounter -= Time.deltaTime;
        if (windCounter <= 0) machine.SetState(machine.idleState.Value);
    }

    protected override void StateExit()
    {
        AffectMap();
        playerMovement.ClearWind();
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

    private void ReplenishLeaves(){}
    private void ExtinguishBonfires(){}

}
