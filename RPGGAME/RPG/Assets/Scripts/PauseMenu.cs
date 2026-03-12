using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseUI;
    private bool paused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        paused = !paused;
        if (pauseUI != null) pauseUI.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    public void Resume()
    {
        paused = false;
        if (pauseUI != null) pauseUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }
}
