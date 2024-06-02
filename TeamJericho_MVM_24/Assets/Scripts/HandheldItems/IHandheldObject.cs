using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHandheldObject : Controls.IPlayerActions
{
    void OnAttachedCarrier(CarrierSystem attachedCarrier);
    void OnAttachedAim(PlayerAim attachedAim);
    void OnAttachedMeleeCollider(CapsuleCollider MeleeCollider);
    void OnAttachedHUD(PlayerHUD playerHUD);
    void OnEquip();
    void OnUnequip();
    void OnStartInterruption(bool isGrenade);
    void OnStopInterruption();
}
