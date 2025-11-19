using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraControlada : MonoBehaviour
{
    public Transform objetivo;
    public float velocidadCamara = 0.205f;
    public Vector3 desplazamiento;

    public void LateUpdate()
    {
        Vector3 posicionDeseada = objetivo.position + desplazamiento;
        Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, velocidadCamara);
        transform.position = posicionSuavizada;
    }
}
