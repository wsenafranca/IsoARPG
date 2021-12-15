using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    void Update()
    {
        if (!target) return;

        Vector3 position = Vector3.MoveTowards(transform.position, target.position, 30.0f * Time.deltaTime);
        position.y = transform.position.y;

        transform.position = position;
    }
}
