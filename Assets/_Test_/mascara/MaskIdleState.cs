using UnityEngine;

public class MaskIdleState : MaskState
{
    MaskAttacks nextAttack;
    float decisionTimer;
    protected override void StateEnter()
    {
        decisionTimer = Random.Range(machine.minAttackCooldown, machine.maxAttackCooldown);
    }

    //TODO - Hay que hacer que una vez elegido el ataque, deje de pasar por la resta al contador
    protected override void StateUpdate()
    {
        machine.MirrorPlayerPosition();

        if (decisionTimer >= 0) decisionTimer -= Time.deltaTime;

        if (decisionTimer <= 0f)
        {
            ChooseRandomAttack();
            SwitchToAttackState();
        }
    }

    private void ChooseRandomAttack()
    {
        int choice = Random.Range(0, 3);

        nextAttack = (MaskAttacks)choice;
    }

    private void SwitchToAttackState()
    {
        switch (nextAttack)
        {
            case MaskAttacks.Laser:
                // machine.SetState(machine.LaserChargeState);
                break;

            case MaskAttacks.Wind:
                // machine.SetState(machine.WindState);
                break;

            case MaskAttacks.Meteor:
                // machine.SetState(machine.MeteorState);
                break;
        }
    }
}

public enum MaskAttacks { Laser, Wind, Meteor }