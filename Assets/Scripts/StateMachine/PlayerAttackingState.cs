using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerAttackingState : PlayerBaseState
{
    private float _previousFrameTime;
    private bool alreadyAppliedForce;
    private Attack _attack;
    public PlayerAttackingState(PlayerStateMachine stateMachine, int AttackIndex) : base(stateMachine)
    {
        _attack = stateMachine.Attacks[AttackIndex];
    }

    public override void Enter()
    {
        stateMachine.WeaponDamage.SetAttack(_attack.Damage, stateMachine.KnockBack);
        stateMachine.animator.CrossFadeInFixedTime(_attack.AnimationName, _attack.TransitionDuration);
    }
    public override void Tick(float deltaTime)
    {
        Move(deltaTime);

        FaceTarget();

        float normalizedTime = GetNormalizedTime(stateMachine.animator);
        if (normalizedTime >= _previousFrameTime && normalizedTime < 1f)
        {
            if(normalizedTime > _attack.ForceTime)
            {
                TryApplyForce();
            }

            if(stateMachine.InputReader.isAttacking)
            {
                TryComboAttack(normalizedTime);
            }
        }
        else
        {
            // going to freelook or targetingstate
            if(stateMachine.Targeter.CurrentTarget != null)
            {
                stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
            }
            else
            {
                stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
            }
        }

        _previousFrameTime = normalizedTime;
    }

    public override void Exit()
    {

    }

    private void TryComboAttack(float normalizedTime)
    {
        if(_attack.ComboStateIndex == -1) { return; }

        if(normalizedTime < _attack.ComboAttackTime) { return; }

        stateMachine.SwitchState
            (

              new PlayerAttackingState
              (

                stateMachine,
                _attack.ComboStateIndex

               )

            ); 
    }

    private void TryApplyForce()
    {
        if (alreadyAppliedForce) { return; }

        stateMachine.ForceReceiver.AddForce(stateMachine.transform.forward * _attack.Force);

        alreadyAppliedForce = true;
    }

}
