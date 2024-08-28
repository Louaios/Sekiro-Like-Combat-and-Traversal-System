using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackingState : EnemyBaseState
{
    private readonly int AttackHash = Animator.StringToHash("Attack");
    private const float TransitionDuration = 0.1f;
    public EnemyAttackingState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.animator.CrossFadeInFixedTime(AttackHash, TransitionDuration);
        stateMachine.weaponDamage.SetAttack(stateMachine.AttackDamage, stateMachine.KnockBack);
        FacePlayer();
    }
    public override void Tick(float deltaTime)
    {

        if (GetNormalizedTime(stateMachine.animator) > 1 )
        {
            stateMachine.SwitchState(new EnemyChasingState(stateMachine));
        }
    }

    public override void Exit()
    {

    }

    private bool isInAttackRange()
    {
        float PlayerDissqr = (stateMachine.Player.transform.position - stateMachine.transform.position).sqrMagnitude;
        return PlayerDissqr <= stateMachine.AttackRange * stateMachine.AttackRange;
    }
}
