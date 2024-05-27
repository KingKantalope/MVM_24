using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    private string actorID;

    public string GetID() { return actorID; }

    public virtual void OnSpawnActor(string assignedID)
    {
        actorID = assignedID;
    }

    public virtual void OnDeath()
    {
        // just destroy this actor
        Destroy(gameObject);

        // tell gamemode/GameManager that target has died

        // DO NOT CALL THIS BASE UNLESS YOU WANT TO DESPAWN THE ACTOR

        // player should detach camera from player and attach to
        // a hovering camera that focuses on the player model
        // while that model ragdolls

        // after a short time or on accepting a prompt to continue,
        // the world should be reloaded from the last resupply crate
        // or the initial spawn point at the parked hover craft
    }
}
