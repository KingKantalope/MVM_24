using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Handheld", menuName = "Handheld", order = 1)]

public class HandheldScriptableObject : ScriptableObject
{
    public GameObject HandheldPrefab;
    public RuntimeAnimatorController ArmController;
    public RuntimeAnimatorController HandheldController;
}
