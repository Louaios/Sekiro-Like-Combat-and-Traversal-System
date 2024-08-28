using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, Controls.IPlayerActions
{
    public bool isAttacking {  get; private set; }
    public bool isBlocking { get; private set; }
    public Vector2 MouvementValue {  get; private set; }   
    
    public event Action jumpEvent;

    public event Action DodgeEvent;

    public event Action TargetEvent;

    public event Action CancelEvent;

    public event Action Dashevent;

    private Controls controls;
    void Start()
    {
        controls  = new Controls();
        controls.Player.SetCallbacks(this);
        controls.Player.Enable();
    }

    private void OnDestroy()
    {
        controls.Player.Disable();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; } //performed == pressed
        // conrtext used to check weather the player releases the button,
        // in this case we dont care
        jumpEvent?.Invoke();
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }

        DodgeEvent?.Invoke();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MouvementValue = context.ReadValue<Vector2>(); //we use context to read the value
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.performed) 
        { 
            isAttacking = true;

        }else if (context.canceled)
        {
            isAttacking = false;
        }
    }

    public void OnTarget(InputAction.CallbackContext context)
    {
        if(!context.performed) { return; }

        TargetEvent?.Invoke();
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }

        CancelEvent?.Invoke();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Dashevent?.Invoke();
    }

    public void OnBlock(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isBlocking = true;

        }
        else if (context.canceled)
        {
            isBlocking = false;
        }
    }
}
