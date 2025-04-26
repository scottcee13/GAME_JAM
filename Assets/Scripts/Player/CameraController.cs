using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] 
    private GameObject target;
    [SerializeField]
    private float radius = 1.0f; //radius for camera
    [SerializeField] 
    private float smoothSpeed = 1.5f;

    private PlayerControllerTG controller;

    private Vector2 offset, newPosition, targetPos;
    private Vector2 velocity = Vector2.zero;

    private void Start()
    {
        controller = target.GetComponent<PlayerControllerTG>();
        offset = transform.position;
    }

    void LateUpdate()
    {
        targetPos = target.transform.position;

        offset = controller.getMouseWorldPos() - targetPos;
        var norm = offset.normalized;
        newPosition = norm * radius + targetPos;

        transform.position = Vector2.SmoothDamp(transform.position, newPosition, ref velocity, smoothSpeed);
        
    }
}
