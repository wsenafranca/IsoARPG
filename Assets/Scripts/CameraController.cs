using UnityEngine;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    private Vector3 _velocity;

    private void Update()
    {
        if (!target) return;

        var trans = transform;
        trans.position = Vector3.SmoothDamp(trans.position, target.transform.position, ref _velocity, 120);
    }
}
