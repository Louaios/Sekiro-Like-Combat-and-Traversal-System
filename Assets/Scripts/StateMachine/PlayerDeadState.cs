using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeadState : PlayerBaseState
{
    private readonly int DeadAnimHash = Animator.StringToHash("Dead");
    private const float AnimationDampTime = 0.1f;

    public PlayerDeadState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("Player DEAD");
        stateMachine.animator.CrossFadeInFixedTime(DeadAnimHash, AnimationDampTime);
    }


    public override void Tick(float deltaTime)
    {
        
    }

    public override void Exit()
    {
        
    }
}
