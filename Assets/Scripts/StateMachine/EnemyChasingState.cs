using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChasingState : EnemyBaseState
{
    private readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private readonly int SpeedHash = Animator.StringToHash("Speed");

    private float CrossFadeDuration = 0.2f;
    private float AnimatornDampTime = 0.1f;

    public EnemyChasingState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.animator.CrossFadeInFixedTime(LocomotionHash, CrossFadeDuration);
    }
    public override void Tick(float deltaTime)
    {
        Move(deltaTime);
        if (!isInChaseRange())
        {
            //Transition
            //Debug.Log("not in range");
            stateMachine.SwitchState(new EnemyIdleState(stateMachine));
            return;
        }else if (isInAttackRange() && stateMachine.canAttack)
        {
            stateMachine.SwitchState(new EnemyAttackingState(stateMachine));
            return;
        }
        stateMachine.animator.SetFloat(SpeedHash, 1f, AnimatornDampTime, deltaTime);
        // Debug.Log("Enemy is in Idle State");
        MoveToPlayer(deltaTime);
        FacePlayer();

        

    }

    public override void Exit()
    {
        stateMachine.Agent.ResetPath();
        stateMachine.Agent.velocity = Vector3.zero;
    }

    private void MoveToPlayer(float deltaTime)
    {
        stateMachine.Agent.destination = stateMachine.Player.transform.position;

        Move(stateMachine.Agent.desiredVelocity.normalized * stateMachine.MouvementSpeed, deltaTime);
        stateMachine.Agent.velocity = stateMachine.controller.velocity;
    }

    private bool isInAttackRange()
    {
        if(stateMachine.Player.isDead) return false;
        float PlayerDissqr = (stateMachine.Player.transform.position - stateMachine.transform.position).sqrMagnitude;
        return PlayerDissqr <= stateMachine.AttackRange * stateMachine.AttackRange;
    }

}
