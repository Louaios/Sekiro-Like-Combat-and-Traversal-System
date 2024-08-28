using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Targeter : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup cineTargetGroup;

    private Camera mainCamera;

    public List<Target> targets = new List<Target>();
    public Target CurrentTarget { get; private set; }
    private void Start()
    {
        mainCamera = Camera.main;  
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Target>(out Target target)) { return; }

        targets.Add(target);
        target.OnDestroyEvent += RemoveTarget;
    }

    private void OnTriggerExit(Collider other)
    {

        if (!other.TryGetComponent<Target>(out Target target)) { return; }

        RemoveTarget(target);

    }

    public bool SelectTarget()
    {
        if (targets.Count == 0) return false;

        Target closestTarget = null;
        float closestTargetDistance = Mathf.Infinity;

        foreach (Target target in targets)
        {
            Vector2 viewPos = mainCamera.WorldToViewportPoint(target.transform.position);

            if (!target.GetComponentInChildren<Renderer>().isVisible)
            {
                continue;
            }

            Vector2 toCenter = viewPos - new Vector2(0.5f, 0.5f);
            if(toCenter.sqrMagnitude < closestTargetDistance)
            {
                closestTarget = target;
                closestTargetDistance = toCenter.sqrMagnitude;
            }
        }

        if(closestTarget == null)
        {
            return false;
        }

        CurrentTarget = closestTarget;
        cineTargetGroup.AddMember(CurrentTarget.transform, 1f, 2f);
        Health targetHealth = CurrentTarget.GetComponent<Health>();
        targetHealth.SetUI(true);

        return true;
    }

    public void Cancel()
    {
        if (CurrentTarget == null) { return; }

        Health targetHealth = CurrentTarget.GetComponent<Health>();
        targetHealth.SetUI(false);
        cineTargetGroup.RemoveMember(CurrentTarget.transform);
        CurrentTarget = null;
    }

    private void RemoveTarget(Target target)
    {
        if (CurrentTarget == target)
        {
            Health targetHealth = CurrentTarget.GetComponent<Health>();
            targetHealth.SetUI(false);
            cineTargetGroup.RemoveMember(CurrentTarget.transform);
            CurrentTarget = null;
        }

        target.OnDestroyEvent -= RemoveTarget;
        targets.Remove(target);

    }
}
