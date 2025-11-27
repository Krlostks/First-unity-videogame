using UnityEngine;
using System;

public class GameManagerCollectibles : MonoBehaviour
{
    public static GameManagerCollectibles Instance { get; private set; }

    [Header("Recolección")]
    public int totalToCollect = 5;
    private int collected = 0;

    public event Action<int,int> OnCollectChanged;
    public event Action OnAllCollected;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);  // ← si quieres que sobreviva a cambios de escena
    }

    public void AddCollectible(int amount = 1)
    {
        collected += amount;
        if (collected > totalToCollect) collected = totalToCollect;

        Debug.Log($"[GameManager] AddCollectible -> {collected}/{totalToCollect}");

        OnCollectChanged?.Invoke(collected, totalToCollect);

        if (collected >= totalToCollect)
        {
            Debug.Log("[GameManager] All collected - invoking OnAllCollected");
            OnAllCollected?.Invoke();
        }
    }

    public int GetCollected() => collected;
    public int GetTotalToCollect() => totalToCollect;
}
