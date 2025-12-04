using UnityEngine;
using TMPro;
using System.Collections;

public class UICollectibleHint : MonoBehaviour
{
    public static UICollectibleHint Instance { get; private set; }

    [Header("Referencias UI")]
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI counterText;
    public float messageDuration = 2f;

    Coroutine current;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }

    void Start()
    {
        if (GameManagerCollectibles.Instance != null)
        {
            GameManagerCollectibles.Instance.OnCollectChanged += UpdateCounter;
            GameManagerCollectibles.Instance.OnAllCollected += OnAllCollected;
            
            // Actualizar contador al iniciar
            UpdateCounter(
                GameManagerCollectibles.Instance.GetCollected(), 
                GameManagerCollectibles.Instance.GetTotalToCollect()
            );
            
            Debug.Log($"[UICollectibleHint] Suscrito a eventos. Total: {GameManagerCollectibles.Instance.GetTotalToCollect()}");
        }
        else
        {
            Debug.LogWarning("[UICollectibleHint] GameManagerCollectibles.Instance es null");
        }
        
        if (messageText != null) 
            messageText.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameManagerCollectibles.Instance != null)
        {
            GameManagerCollectibles.Instance.OnCollectChanged -= UpdateCounter;
            GameManagerCollectibles.Instance.OnAllCollected -= OnAllCollected;
        }
    }

    public void ShowTemporaryMessage(string text)
    {
        if (messageText == null) return;
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(ShowForSeconds(text, messageDuration));
    }

    IEnumerator ShowForSeconds(string text, float seconds)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = text;
        yield return new WaitForSeconds(seconds);
        messageText.gameObject.SetActive(false);
        current = null;
    }

    void UpdateCounter(int col, int total)
    {
        if (counterText != null)
        {
            counterText.text = $"{col}/{total}";
            Debug.Log($"[UICollectibleHint] Counter actualizado: {col}/{total}");
        }
        else
        {
            Debug.LogWarning("[UICollectibleHint] counterText es null");
        }
    }

    void OnAllCollected()
    {
        Debug.Log("[UICollectibleHint] ¡Todos los collectibles recogidos!");
        ShowTemporaryMessage("¡Puerta desbloqueada!");
    }
}
