using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class CameraController : MonoBehaviour
{
    private Transform playerTransform;
    private float smoothSpeed = 0.05f;
    private Vector3 offset;

    void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
        offset = this.transform.position - playerTransform.position;
    }

    void FixedUpdate()
    {
        Vector3 desiredPos = playerTransform.position + offset;
        Vector3 smoothedPos = Vector3.Lerp(this.transform.position, desiredPos, smoothSpeed);
        this.transform.position = smoothedPos;
    }
}
