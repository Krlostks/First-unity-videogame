using UnityEngine;

public class CanvasMenu : MonoBehaviour, IPausable
{
    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.enabled = false; // que inicie oculto
        GamePauseManager.Instance.RegisterPausable(this);
    }

    void OnDestroy()
    {
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.UnregisterPausable(this);
    }

    public void OnPause()
    {
        canvas.enabled = true; // mostrar canvas al pausar
    }

    public void OnResume()
    {
        canvas.enabled = false; // ocultar canvas al reanudar
    }
}
