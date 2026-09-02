using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuScript : MonoBehaviour
{
    public string mainMenuScene;

    public void returnToGame() {
        PauseManager.Instance.Resume();
    }

    public void returnToMenu() {
        SceneManager.LoadScene(mainMenuScene);
    }
}
