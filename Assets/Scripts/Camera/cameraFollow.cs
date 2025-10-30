using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.2f, -10f);
    public float smoothSpeed = 0.12f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        Vector3 smoothed = Vector3.Lerp(transform.position, desired, smoothSpeed);
        transform.position = new Vector3(smoothed.x, smoothed.y, transform.position.z);
    }
}
