using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoil : MonoBehaviour
{
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [Header("Recoil Stuff")]
    [SerializeField] private Transform cameraContainer;
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
        cameraContainer.localRotation = Quaternion.Euler(currentRotation);
    }

    public void AddRecoil(Vector3 recoil, Vector3 variance)
    {
        targetRotation += new Vector3(
            Random.Range(recoil.x - variance.x, recoil.x + variance.x),
            Random.Range(recoil.y - variance.y, recoil.y + variance.y),
            Random.Range(recoil.z - variance.z, recoil.z + variance.z));
    }
}
