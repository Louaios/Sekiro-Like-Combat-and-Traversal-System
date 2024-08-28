using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBaseState : State
{
    protected EnemyStateMachine stateMachine;

    public EnemyBaseState(EnemyStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    protected void Move(float deltaTime)
    {
        Move(Vector3.zero, deltaTime);
    }
    protected void Move(Vector3 motion, float deltaTime)
    {
        stateMachine.controller.Move((motion + stateMachine.ForceReceiver.Mouvement) * deltaTime);
    }

    protected void FacePlayer() 
    { 
        if(stateMachine.Player == null) { return; }

        Vector3 lookPos = stateMachine.Player.transform.position - stateMachine.transform.position;
        lookPos.y = 0f;

        stateMachine.transform.rotation = Quaternion.Lerp(stateMachine.transform.rotation, Quaternion.LookRotation(lookPos), 0.5f);
        // to modify LATERRR


    }

    protected bool isInChaseRange()
    {
        if(stateMachine.Player.isDead) { return false;}
        float DisEnemyPlayer = (stateMachine.Player.transform.position - stateMachine.transform.position).sqrMagnitude;
        return DisEnemyPlayer <= stateMachine.DetectionRange * stateMachine.DetectionRange ;
    }
}
