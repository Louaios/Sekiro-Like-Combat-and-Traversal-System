using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBrokenState : PlayerBaseState
{
    private readonly int BrokenHash = Animator.StringToHash("Broken");
    private const float AnimationDampTime = 0.1f;

    private float timer;

    public PlayerBrokenState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("BROKEN");
        stateMachine.animator.CrossFadeInFixedTime(BrokenHash, AnimationDampTime);
        timer = stateMachine.BrokenDuration;
    }

    public override void Tick(float deltaTime)
    {
       timer -= deltaTime;

        if (timer < 0)
        {
            stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
        }
    }

    public override void Exit()
    {
        stateMachine.Health.posture = 0;
    }

}
