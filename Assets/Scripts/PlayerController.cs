using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;
    CharacterController characterController;
    Animator animator;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] float groundCheckRadius;
    [SerializeField] LayerMask GroundLayer;


    Quaternion targetRotation;
    bool isGrounded;
    float ySpeed;

    CameraFollow cameraFollow;

    private void Awake()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(v) + Mathf.Abs(h)); 

        var moveInput = (new Vector3(h, 0, v)).normalized;

        var moveDir = cameraFollow.PlanarRotation * moveInput;
        GroundCheck();

        if (isGrounded)
        {
            ySpeed = -0.5f;
        }else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDir * moveSpeed;

        velocity.y = ySpeed;
        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        animator.SetFloat("moveAmount", moveAmount, 0.2f, Time.deltaTime);
        animator.SetBool("isGrounded", isGrounded);

    }

    void GroundCheck()
    {
         isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, GroundLayer);   
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}
