using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private const float CrossFadeDuration = 0.1f;

    private readonly int DashHash = Animator.StringToHash("DashingBlendTree");
    private readonly int DashForwardHash = Animator.StringToHash("DashForward");
    private readonly int DashRightHash = Animator.StringToHash("DashRight");

    private Vector3 dashingDirInput;
    private float remainingDashTime;
    public PlayerDashState(PlayerStateMachine stateMachine, Vector3 dashingDirInput) : base(stateMachine)
    {
        this.dashingDirInput = dashingDirInput;
    }

    public override void Enter()
    {
        remainingDashTime = stateMachine.DashDuration;
        stateMachine.animator.SetFloat(DashForwardHash, dashingDirInput.y);
        stateMachine.animator.SetFloat(DashRightHash, dashingDirInput.x);
        stateMachine.animator.CrossFadeInFixedTime(DashHash, CrossFadeDuration);

       // stateMachine.Health.SetInvulnerability(true);
       //TO CHANGE so that it doesnt conflict with blocking sounds
    }
    public override void Tick(float deltaTime)
    {
        Vector3 movement = new Vector3();
        movement += stateMachine.transform.right * dashingDirInput.x * stateMachine.dashForce / stateMachine.DashDuration;
        movement += stateMachine.transform.forward * dashingDirInput.y * stateMachine.dashForce / stateMachine.DashDuration;

        Move(movement * stateMachine.dashForce, deltaTime);
        FaceTarget();

        remainingDashTime -= deltaTime;

        if (remainingDashTime < 0f)
            stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
    }

    public override void Exit()
    {
        //stateMachine.Health.SetInvulnerability(true);
        //same as above
    }
    private Vector3 CalculateMouvement(float deltaTime)
    {
        Vector3 movement = new Vector3();

        movement = new Vector3(dashingDirInput.x, 0f, dashingDirInput.y);

        return movement;
    }
}
