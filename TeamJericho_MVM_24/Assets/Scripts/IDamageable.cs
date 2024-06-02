using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    DamageEnd MainDamage(Damage damage, Transform damageSource);
    void Heal(float amount);
    void Fortify(float amount);
    void Charge(float amount);
    void OffsetPoise(int stagger);
    void OffsetRadiation(int radiation);
    void OffsetFrost(int frost);
    void SetHemorrhage(Hemorrhage level);
    void SetShock(bool newShocked);
    string GetActorID();
    (float, float) GetHealth();
    (float, float, int) GetArmor();
    (float, float) GetShields();
    float GetPoise();
    float GetRadiation();
    float GetFrost();
    Hemorrhage GetHemorrhage();
    bool GetShocked();
}

public enum Hemorrhage
{
    none,
    minor,
    major,
    size
}

public enum DamageEnd
{
    health,
    armor,
    shields,
    kill,
    crit,
    none
}

[Serializable]
public struct Damage
{
    public float baseDamage;
    public float shieldMulti;
    public float armorMulti;
    public float critMulti;
    public int penetrationLevel;
    public bool isCrit;
    public bool canInstakill;
    public int critBonus;
}
