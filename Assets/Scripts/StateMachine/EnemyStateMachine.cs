using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateMachine : StateMachine
{
    [field: SerializeField] public Animator animator { get; private set; }
    [field: SerializeField] public CharacterController controller { get; private set; }
    [field: SerializeField] public NavMeshAgent Agent { get; private set; }
    [field: SerializeField] public WeaponDamage weaponDamage { get; private set; }
    [field: SerializeField] public Target Target { get; private set; }
    [field: SerializeField] public Ragdoll Ragdoll { get; private set; }
    [field: SerializeField] public Health health { get; private set; }
    [field: SerializeField] public float MouvementSpeed { get; private set; }
    [field: SerializeField] public ForceReceiver ForceReceiver { get; private set; }
    public  Health Player { get; private set; }
    [field: SerializeField] public float DetectionRange { get; private set; }
    [field: SerializeField] public float AttackRange { get; private set; }
    [field: SerializeField] public int AttackDamage { get; private set; }
    [field: SerializeField] public float KnockBack { get; private set; }
    [field: SerializeField] public float BrokenDuration { get; private set; }

    private void Start()
    {
        Player = GameObject.FindWithTag("Player").GetComponent<Health>();

        Agent.updatePosition = false; // so that the agent doesnt move to player on its own
        Agent.updateRotation = false;

        SwitchState(new EnemyIdleState(this));
    }

    private void OnEnable()
    {
        health.OnTakeDamage += handleTakeDamage;
        health.OnDieEvent += OnDie;
        health.OnBreakEvent += OnBroken;
    }


    private void OnDisable()
    {
        health.OnTakeDamage -= handleTakeDamage;
        health.OnDieEvent -= OnDie;
        health.OnBreakEvent -= OnBroken;
    }

    private void handleTakeDamage()
    {
        SwitchState(new EnemyImpactState(this));
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
    }
    private void OnBroken()
    {
        SwitchState(new EnemyBrokenState(this));
    }

    private void OnDie()
    {
        SwitchState(new EnemyDeadState(this));
    }
}