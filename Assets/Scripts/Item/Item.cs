using UnityEngine;

public class Item : MonoBehaviour
{
    public int scoreValue = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(scoreValue);
            Destroy(gameObject);
            Debug.Log("Item recogido! Puntos: " + scoreValue);
        }
    }
}
