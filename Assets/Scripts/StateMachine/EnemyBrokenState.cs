using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBrokenState : EnemyBaseState
{
    private readonly int BrokenHash = Animator.StringToHash("Broken");
    private const float AnimationDampTime = 0.1f;

    private float timer;

    public EnemyBrokenState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
       stateMachine.animator.CrossFadeInFixedTime(BrokenHash, AnimationDampTime);
        timer = stateMachine.BrokenDuration;
       
    }
    public override void Tick(float deltaTime)
    {
        timer -= deltaTime;
        if(timer < 0f)
        {
            stateMachine.SwitchState(new EnemyChasingState(stateMachine));
        }
    }

    public override void Exit()
    {
        stateMachine.health.posture = 0;
    }

}
