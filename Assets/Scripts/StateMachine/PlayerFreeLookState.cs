using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFreeLookState : PlayerBaseState
{
    private const float CrossFadeDurationFreeLook = 0.1f;

    private readonly int FreeLookBlendTreeHash = Animator.StringToHash("FreeLookBlendTree");
    private readonly int FreeLookSpeed = Animator.StringToHash("FreeLookSpeed");
    private const float AnimatorDampTime = 0.1f;


    public PlayerFreeLookState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.InputReader.TargetEvent += OnTarget;
        stateMachine.animator.CrossFadeInFixedTime(FreeLookBlendTreeHash, CrossFadeDurationFreeLook);
    }


    public override void Tick(float deltaTime)
    {
        if (stateMachine.InputReader.isAttacking)
        {
            stateMachine.SwitchState(new PlayerAttackingState(stateMachine, 0));
            return;
        }

        Vector3 mouvement = CalculateMouvement();

        Move(mouvement * stateMachine.FreeLookMouvementSpeed, deltaTime);

        if (stateMachine.InputReader.MouvementValue == Vector2.zero)
        {
            stateMachine.animator.SetFloat(FreeLookSpeed, 0, AnimatorDampTime, deltaTime);
            return;
        }
        stateMachine.animator.SetFloat(FreeLookSpeed, 1, AnimatorDampTime, deltaTime);
        LookDirection(mouvement, Time.deltaTime);
    }


    public override void Exit()
    {

    }

    private void OnTarget()
    {
        if(!stateMachine.Targeter.SelectTarget()) { return; }

        stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
    }

    private Vector3 CalculateMouvement()
    {
        Vector3 forward = stateMachine.MainCameraTransform.forward;
        Vector3 right = stateMachine.MainCameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return forward * stateMachine.InputReader.MouvementValue.y + right * stateMachine.InputReader.MouvementValue.x;
    }
    private void LookDirection(Vector3 mouvement, float deltaTime)
    {
        stateMachine.transform.rotation = Quaternion.Lerp(stateMachine.transform.rotation, Quaternion.LookRotation(mouvement), deltaTime * stateMachine.RotationDampSmooth);
    }
}
