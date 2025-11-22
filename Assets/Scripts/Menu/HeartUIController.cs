using UnityEngine;

public class HealthUI : MonoBehaviour
{
    public Animator[] heartAnimators;

    void Awake()
    {
        if (heartAnimators == null || heartAnimators.Length == 0)
        {
            Debug.LogError("[HealthUI] heartAnimators está vacío o no asignado.");
        }
        else
        {
            Debug.Log("[HealthUI] heartAnimators asignado con " + heartAnimators.Length + " corazones.");
            for (int i = 0; i < heartAnimators.Length; i++)
            {
                if (heartAnimators[i] == null)
                    Debug.LogError("[HealthUI] heartAnimators[" + i + "] está NULL.");
                else
                    Debug.Log("[HealthUI] heartAnimators[" + i + "] = " + heartAnimators[i].gameObject.name);
            }
        }
    }

    public void UpdateHearts(int currentHealth)
    {
        if (heartAnimators == null || heartAnimators.Length == 0)
        {
            Debug.LogError("[HealthUI] No hay hearts asignados en heartAnimators.");
            return;
        }

        Debug.Log("[HealthUI] UpdateHearts llamado. currentHealth = " + currentHealth);

        for (int i = 0; i < heartAnimators.Length; i++)
        {
            var anim = heartAnimators[i];
            if (anim == null)
            {
                Debug.LogError("[HealthUI] Animator en índice " + i + " es NULL.");
                continue;
            }

            bool broken = i >= currentHealth;
            anim.SetBool("broken", broken);
            Debug.Log("[HealthUI] Corazón " + i + " (" + anim.gameObject.name + ") broken = " + broken);
        }
    }
}
