using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerImpactState : PlayerBaseState
{
    private readonly int ImpactHash = Animator.StringToHash("Impact");
    private const float CrossFadeDuration = 0.1f;

    private float Duration = 1f;

    public PlayerImpactState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.animator.CrossFade(ImpactHash, CrossFadeDuration);
    }
    public override void Tick(float deltaTime)
    {
        Move(deltaTime);

       Duration -= deltaTime;

        if (Duration <= 0f)
        {
            ReturnToLocomotion(); // Methode that checks if player is targeting then return to targeting
            //else return to freeloockstate
        }
    }

    public override void Exit()
    {
        
    }

}
