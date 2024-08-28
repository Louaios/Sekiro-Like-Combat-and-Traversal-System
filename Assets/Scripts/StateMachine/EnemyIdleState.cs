using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    private readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private readonly int SpeedHash = Animator.StringToHash("Speed");

    private float CrossFadeDuration = 0.2f;
    private float AnimatornDampTime = 0.1f;
    public EnemyIdleState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.animator.CrossFadeInFixedTime(LocomotionHash, CrossFadeDuration);
    }
    public override void Tick(float deltaTime)
    {
        Move(deltaTime);
        if (isInChaseRange())
        {
            //Transition
            //Debug.Log("inRange");
            stateMachine.SwitchState(new EnemyChasingState(stateMachine));
            return;
        }
        stateMachine.animator.SetFloat(SpeedHash, 0f, AnimatornDampTime, deltaTime);
        // Debug.Log("Enemy is in Idle State");
        FacePlayer();

    }

    public override void Exit()
    {
    }

}
