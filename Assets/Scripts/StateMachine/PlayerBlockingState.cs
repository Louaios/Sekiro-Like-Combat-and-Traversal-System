using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlockingState : PlayerBaseState
{
    private readonly int BlockingHash = Animator.StringToHash("Blocking");
    private const float AnimationDampTime = 0.1f;

    public PlayerBlockingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.Health.SetInvulnerability(true);
        stateMachine.Health.OnBreakEvent += OnBroken;
        stateMachine.animator.CrossFadeInFixedTime(BlockingHash, AnimationDampTime);   
    }
    public override void Tick(float deltaTime)
    {
        Move(deltaTime);

        if (!stateMachine.InputReader.isBlocking)
        {
            stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
            return;
        }
        if (stateMachine.Targeter.CurrentTarget == null)
        {
            stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
            return;
        }
    }

    public override void Exit()
    {
        stateMachine.Health.SetInvulnerability(false);
        stateMachine.Health.OnBreakEvent -= OnBroken;
    }
    private void OnBroken()
    {
        stateMachine.SwitchState(new PlayerBrokenState(stateMachine));
    }

}
