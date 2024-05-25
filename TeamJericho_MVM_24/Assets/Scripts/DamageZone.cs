using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageZone : MonoBehaviour
{
    private List<IDamageable> targets;

    public Damage damage;

    private void Start()
    {
        targets = new List<IDamageable>();
    }

    private void Update()
    {
        Damage dmgToDeal = damage;
        dmgToDeal.baseDamage *= Time.deltaTime;

        foreach (var target in targets)
        {
            if (target.MainDamage(dmgToDeal, transform.position) == DamageEnd.kill)
            {
                targets.Remove(target);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        targets.Add(other.gameObject.GetComponent<IDamageable>());
    }

    private void OnTriggerExit(Collider other)
    {
        targets.Remove(other.gameObject.GetComponent<IDamageable>());
    }
}
