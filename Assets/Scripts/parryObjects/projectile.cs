using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 1f;
    private Vector3 direction = Vector3.right;

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    public void Redirect(Vector3 newDirection)
    {
        direction = newDirection.normalized;
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }
}
