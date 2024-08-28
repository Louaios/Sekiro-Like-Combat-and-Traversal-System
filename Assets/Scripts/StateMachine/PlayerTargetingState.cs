using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerTargetingState : PlayerBaseState
{
    private const float CrossFadeDurationTargeting = 0.1f;

    private readonly int LockOnBlendTreeHash = Animator.StringToHash("LockOnBlendTree");
    private readonly int TargetingForwardHash = Animator.StringToHash("LockOnForward");
    private readonly int TargetingRightHash = Animator.StringToHash("LockOnRight");
    public PlayerTargetingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.InputReader.CancelEvent += OnCancel;
        stateMachine.InputReader.Dashevent += OnDash;
        stateMachine.animator.CrossFadeInFixedTime(LockOnBlendTreeHash, CrossFadeDurationTargeting);
    }

    private void OnDash()
    {
        stateMachine.SwitchState(new PlayerDashState(stateMachine, stateMachine.InputReader.MouvementValue));
       /* if (Time.time - stateMachine.previousDashTime < stateMachine.DashCooldown)
            return;

        stateMachine.SetDashTime(Time.time);
        dashingDirInput = stateMachine.InputReader.MouvementValue;
        remainingDashTime = stateMachine.DashDuration;*/
    }

    public override void Tick(float deltaTime)
    {
        if (stateMachine.InputReader.isAttacking)
        {
            stateMachine.SwitchState(new PlayerAttackingState(stateMachine, 0));
        }

        if (stateMachine.InputReader.isBlocking)
        {
            stateMachine.SwitchState(new PlayerBlockingState(stateMachine));
            return;
        }

        if (stateMachine.Targeter.CurrentTarget == null)
        {
            stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
            return;
        }

        Vector3 movement = CalculateMouvement(deltaTime);
        Move(movement * stateMachine.TargetingSpeed, deltaTime);

        UpdateAnimator(deltaTime);
        FaceTarget();
    }

    public override void Exit()
    {
        stateMachine.InputReader.Dashevent -= OnDash;
    }

    private void OnCancel()
    {
        stateMachine.Targeter.Cancel();
        stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
    }

    private Vector3 CalculateMouvement(float deltaTime)
    {
        Vector3 movement = new Vector3();

        movement += stateMachine.transform.right * stateMachine.InputReader.MouvementValue.x;
        movement += stateMachine.transform.forward * stateMachine.InputReader.MouvementValue.y;

        return movement;
    }
    private void UpdateAnimator(float deltaTime)
    {
        if(stateMachine.InputReader.MouvementValue.y == 0)
        {

           stateMachine.animator.SetFloat(TargetingForwardHash, 0, 0.1f, deltaTime);

        }
        else
        {
            float value = stateMachine.InputReader.MouvementValue.y > 0 ? 1f : -1f;
            stateMachine.animator.SetFloat(TargetingForwardHash, value, 0.1f, deltaTime);
        }
        if (stateMachine.InputReader.MouvementValue.x == 0)
        {

            stateMachine.animator.SetFloat(TargetingRightHash, 0, 0.1f, deltaTime);

        }
        else
        {
            float value = stateMachine.InputReader.MouvementValue.x > 0 ? 1f : -1f;
            stateMachine.animator.SetFloat(TargetingRightHash, value, 0.1f, deltaTime);
        }
    }

}
