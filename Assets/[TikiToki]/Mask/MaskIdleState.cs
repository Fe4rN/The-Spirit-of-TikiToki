using NUnit.Framework;
using UnityEngine;

public class MaskIdleState : MaskState
{
    MaskAttacks nextAttack;
    float decisionTimer;
    bool isAttackChosen = false;

    [Header("Cooldown de Ataque")]
    public float minAttackCooldown;
    public float maxAttackCooldown;

    protected override void StateEnter()
    {
        Debug.Log("Entering Idle State");
        decisionTimer = Random.Range(minAttackCooldown, maxAttackCooldown);
        isAttackChosen = false;
    }

    protected override void StateUpdate()
    {
        machine.MirrorPlayerPosition();

        if (!isAttackChosen) decisionTimer -= Time.deltaTime;

        if (decisionTimer <= 0f && !isAttackChosen)
        {
            isAttackChosen = true;
            ChooseRandomAttack();
            SwitchToAttackState();
        }
    }

    private void ChooseRandomAttack()
    {
        int choice = Random.Range(0, 4);

        nextAttack = (MaskAttacks)choice;
    }

    private void SwitchToAttackState()
    {
        switch (nextAttack)
        {
            case MaskAttacks.Laser:
                machine.SetState(machine.laserState.Value);
                break;

            case MaskAttacks.Wind:
                machine.SetState(machine.windState.Value);
                break;

            case MaskAttacks.Meteor:
                machine.SetState(machine.meteorState.Value);
                break;

            case MaskAttacks.Scream:
                machine.SetState(machine.screamState.Value);
                break;
        }
    }
}

public enum MaskAttacks { Laser, Wind, Meteor, Scream }