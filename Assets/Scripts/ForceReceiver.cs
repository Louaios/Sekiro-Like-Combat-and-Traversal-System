using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ForceReceiver : MonoBehaviour
{
    private float verticalVelocity;
    [SerializeField] private CharacterController controller;
    [SerializeField] private float  drag = 0.3f; // We modify the drag to change how much the Player moves when attacking RECOIL
    private Vector3 dampingVelocity;
    [SerializeField] private NavMeshAgent agent;

    private Vector3 _impact;

    public Vector3 Mouvement => _impact + Vector3.up * verticalVelocity;

    private void Update()
    {
        if (verticalVelocity < 0f && controller.isGrounded)
        {
            verticalVelocity = Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        _impact = Vector3.SmoothDamp(_impact, Vector3.zero, ref dampingVelocity, drag);
        //methods dont keep the changes on Vector3's so we use the ref before a vector  variable
        // to tell the methode that we want to keep the modified value on the given Vector
        if (agent != null)
        {
            if (_impact.sqrMagnitude <= 0.2f * 0.2f)
            {
                _impact = Vector3.zero;
                agent.enabled = true;
            }
        }
    }

    public void AddForce(Vector3 Force)
    {
        _impact += Force;
        if (agent != null)
        {
            agent.enabled = false;
        }
    }
}
