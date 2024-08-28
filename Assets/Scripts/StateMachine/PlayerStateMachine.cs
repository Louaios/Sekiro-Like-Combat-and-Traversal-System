using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerStateMachine : StateMachine
{
    [field : SerializeField] public InputReader InputReader {  get; private set; } // you cant set the Input Reader elsewhere 
    //field : serializefield to create a field and serialize it for us to use this property
    //we always use this to reference something from inside a state
    [field: SerializeField] public CharacterController controller { get; private set; }
    [field: SerializeField] public float FreeLookMouvementSpeed { get; private set; }
    [field: SerializeField] public float TargetingSpeed { get; private set; }
    [field: SerializeField] public float RotationDampSmooth { get; private set; }
    [field: SerializeField] public Animator animator { get; private set; }
    [field: SerializeField] public Targeter Targeter { get; private set; }
    [field: SerializeField] public ForceReceiver ForceReceiver { get; private set; }
    [field: SerializeField] public WeaponDamage WeaponDamage { get; private set; } //to reference weapon logic component
    [field: SerializeField] public Attack[] Attacks { get; private set; } //Attck class does derive from monobehiavor thus
                                                                          //we must make the class serializable
    [field: SerializeField] public float KnockBack { get; private set; }
    [field: SerializeField] public float DashDuration { get; private set; }

    [field: SerializeField] public float dashForce { get; private set; }
    [field: SerializeField] public float previousDashTime { get; private set; } = Mathf.NegativeInfinity;
    [field: SerializeField] public float BrokenDuration { get; private set; }

    [field: SerializeField] public Health Health { get; private set; }


    public Transform MainCameraTransform { get; private set; }
    void Start()
    {
        MainCameraTransform = Camera.main.transform;

        SwitchState(new PlayerFreeLookState(this));
    }

    private void OnEnable()
    {
        Health.OnTakeDamage += HandleTakingDamage;
        Health.OnBreakEvent += OnBroken;
        Health.OnDieEvent += OnDie;
    }

    private void OnDisable()
    {
        Health.OnTakeDamage -= HandleTakingDamage;
        Health.OnBreakEvent -= OnBroken;
        Health.OnDieEvent -= OnDie;
    }

    private void HandleTakingDamage()
    {
        SwitchState(new PlayerImpactState(this));
    }

    private void OnBroken()
    {
        SwitchState(new PlayerBrokenState(this));
    }

    private void OnDie()
    {
        SwitchState(new PlayerDeadState(this));
    }
}
