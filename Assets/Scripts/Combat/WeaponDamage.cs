using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    private int _damage = 10;
    private float KnockBack;
    [SerializeField] private Collider myCollider;
    private List<Collider> alreadyCollidedWith = new List<Collider>();
    [SerializeField] private AudioSource deflectingAudio;

    private void OnEnable()
    {
        alreadyCollidedWith.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other == myCollider) { return; }

        if(alreadyCollidedWith.Contains(other)) { return; }

        alreadyCollidedWith.Add(other);

        if(other.TryGetComponent<Health>(out Health health))
        {
            if (health.isParrying)
            {
                deflectingAudio.Play();
                Health parentHealth = myCollider.GetComponent<Health>();
                parentHealth.posture += _damage;
                parentHealth.postureSlider.value = parentHealth.posture;
                parentHealth.mirrorPostureSlider.value = parentHealth.posture;
                parentHealth.DealDamage(0);
            }
            else
            {
                health.DealDamage(_damage);
            }

        }
        if(other.TryGetComponent<ForceReceiver>(out ForceReceiver forceReceiver)) 
        {
            Vector3 direction = (other.transform.position - myCollider.transform.position).normalized;
            forceReceiver.AddForce(direction * KnockBack);
        }
    }
    public void SetAttack(int damage, float KnockBack)//GONNA ADD A KNOCKBACK LATER SAME LOGIC AS DAMAGE
    {
        this._damage = damage;
        this.KnockBack = KnockBack;
    }
}
