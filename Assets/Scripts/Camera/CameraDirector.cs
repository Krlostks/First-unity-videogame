using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CameraDirector : MonoBehaviour
{
    private Camera mainCam;
    private cameraFollow follow;
    private bool isCinematic = false;

    private Vector3 followOriginalOffset;
    private float originalZoom;

    void Awake()
    {
        mainCam = GetComponent<Camera>();
        follow = GetComponent<cameraFollow>();
    }

    /// <summary>
    /// Inicia una cinemática moviendo la cámara a una posición fija.
    /// </summary>
    public void MoveToPosition(Vector3 targetPos, float duration, float targetZoom )
    {
        if (!isCinematic)
            SaveOriginalFollowState();

        follow.enabled = false;
        StartCoroutine(MoveRoutine(targetPos, duration, targetZoom));
    }

    /// <summary>
    /// Restaura el seguimiento de cámara normal.
    /// </summary>
    public void RestoreFollow()
    {
        isCinematic = false;

        follow.offset = followOriginalOffset;
        mainCam.orthographicSize = originalZoom;

        follow.enabled = true;
    }

    // --------------------------
    // INTERNAL
    // --------------------------

    private void SaveOriginalFollowState()
    {
        isCinematic = true;
        followOriginalOffset = follow.offset;
        originalZoom = mainCam.orthographicSize;
    }

    private IEnumerator MoveRoutine(Vector3 targetPos, float duration, float targetZoom)
    {
        Vector3 startPos = transform.position;
        float startZoom = mainCam.orthographicSize;

        float time = 0;

        while (time < duration)
        {
            float t = time / duration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);

            if (targetZoom > 0)
                mainCam.orthographicSize = Mathf.Lerp(startZoom, targetZoom, t);

            time += Time.deltaTime;
            yield return null;
        }

        // Asegurar valores finales
        transform.position = targetPos;
        if (targetZoom > 0)
            mainCam.orthographicSize = targetZoom;
    }
}
