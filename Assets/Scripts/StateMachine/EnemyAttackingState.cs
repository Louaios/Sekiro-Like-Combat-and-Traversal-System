using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyAttackingState : EnemyBaseState
{
    private readonly int AttackHash0 = Animator.StringToHash("Attack");
    private readonly int AttackHash1 = Animator.StringToHash("Attack1");
    private readonly int AttackHash2 = Animator.StringToHash("Attack2");
    private const float TransitionDuration = 0.1f;
    public EnemyAttackingState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        UpdateAttackAnimation();
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

    private void UpdateAttackAnimation()
    {
        System.Random random = new System.Random();
        int randomValue = random.Next(0, 3);
        int AttackToPlayHash;

        if (randomValue == 0)
        {
            AttackToPlayHash = AttackHash0;
        }else if (randomValue == 1)
        {
            AttackToPlayHash = AttackHash1;
        }else
        {
            AttackToPlayHash= AttackHash2;
        }
        stateMachine.animator.CrossFadeInFixedTime(AttackToPlayHash, TransitionDuration);
    }
}
