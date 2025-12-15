using UnityEngine;

public class PlayerPositionSetter : MonoBehaviour
{
    [Header("Posición al cargar")]
    public Vector3 spawnPosition = new Vector3(0, 0, 0);
    
    void Start()
    {
        // Teletransportar al jugador a la posición especificada
        transform.position = spawnPosition;
        
        Debug.Log($"[PlayerPositionSetter] Jugador teletransportado a: {spawnPosition}");
    }
}