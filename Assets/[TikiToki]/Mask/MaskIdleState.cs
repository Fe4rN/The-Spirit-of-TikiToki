using System.Collections.Generic;
using UnityEngine;

public class MaskIdleState : MaskState
{
    MaskAttacks nextAttack;
    float decisionTimer;
    bool isAttackChosen = false;

    [Header("Cooldown de Ataque")]
    public float minAttackCooldown;
    public float maxAttackCooldown;
    List<MaskAttacks> attackPool = new List<MaskAttacks>();
    MaskAttacks lastAttack;

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
        if (attackPool.Count == 0)
            RefillAttackPool();

        nextAttack = attackPool[0];
        attackPool.RemoveAt(0);
        lastAttack = nextAttack;
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

    private void RefillAttackPool()
    {
        attackPool.Clear();

        foreach (MaskAttacks attack in System.Enum.GetValues(typeof(MaskAttacks)))
            attackPool.Add(attack);


        attackPool.Add(MaskAttacks.Scream);
        attackPool.Add(MaskAttacks.Wind);
        
        ShuffleAttackPool();

        PreventImmediateRepeat();
    }

    private void ShuffleAttackPool()
    {
        for (int i = 0; i < attackPool.Count; i++)
        {
            int rand = Random.Range(i, attackPool.Count);
            (attackPool[i], attackPool[rand]) = (attackPool[rand], attackPool[i]);
        }
    }

    private void PreventImmediateRepeat()
    {
        if (attackPool.Count <= 1) return;

        if (attackPool[0] == lastAttack)
        {
            int swapIndex = Random.Range(1, attackPool.Count);
            (attackPool[0], attackPool[swapIndex]) = (attackPool[swapIndex], attackPool[0]);
        }
    }
}

public enum MaskAttacks { Laser, Wind, Meteor, Scream }