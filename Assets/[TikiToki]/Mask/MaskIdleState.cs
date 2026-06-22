using System.Collections.Generic;
using UnityEngine;

namespace TikiToki.Gameplay.Boss
{
    public class MaskIdleState : MaskState
    {
        private MaskAttacks _nextAttack;
        private float _decisionTimer;
        private bool _isAttackChosen = false;

        [Header("Attack Cooldown")]
        public float minAttackCooldown;
        public float maxAttackCooldown;
        private List<MaskAttacks> _attackPool = new List<MaskAttacks>();
        private MaskAttacks _lastAttack;

        protected override void StateEnter()
        {
            Debug.Log("Entering Idle State");
            _decisionTimer = Random.Range(minAttackCooldown, maxAttackCooldown);
            _isAttackChosen = false;
        }

        protected override void StateUpdate()
        {
            machine.MirrorPlayerPosition();

            if (!_isAttackChosen) _decisionTimer -= Time.deltaTime;

            if (_decisionTimer <= 0f && !_isAttackChosen)
            {
                _isAttackChosen = true;
                ChooseRandomAttack();
                SwitchToAttackState();
            }
        }

        private void ChooseRandomAttack()
        {
            if (_attackPool.Count == 0)
                RefillAttackPool();

            _nextAttack = _attackPool[0];
            _attackPool.RemoveAt(0);
            _lastAttack = _nextAttack;
        }

        private void SwitchToAttackState()
        {
            switch (_nextAttack)
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
            _attackPool.Clear();

            foreach (MaskAttacks attack in System.Enum.GetValues(typeof(MaskAttacks)))
                _attackPool.Add(attack);

            _attackPool.Add(MaskAttacks.Scream);
            _attackPool.Add(MaskAttacks.Wind);
            
            ShuffleAttackPool();
            PreventImmediateRepeat();
        }

        private void ShuffleAttackPool()
        {
            for (int i = 0; i < _attackPool.Count; i++)
            {
                int rand = Random.Range(i, _attackPool.Count);
                (_attackPool[i], _attackPool[rand]) = (_attackPool[rand], _attackPool[i]);
            }
        }

        private void PreventImmediateRepeat()
        {
            if (_attackPool.Count <= 1) return;

            if (_attackPool[0] == _lastAttack)
            {
                int swapIndex = Random.Range(1, _attackPool.Count);
                (_attackPool[0], _attackPool[swapIndex]) = (_attackPool[swapIndex], _attackPool[0]);
            }
        }
    }

    public enum MaskAttacks { Laser, Wind, Meteor, Scream }
}
