using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI highScoreUI;
    public string newGameScene;

    public AudioClip bg_music;
    public AudioSource main_channel;

    void Start()
    {
        main_channel.clip = bg_music;
        main_channel.Play();

        // Set the high score text
        int highScore = SessionManager.Instance.LoadHighScore();
        highScoreUI.text = $"Top Wave Survived: {highScore}";
    }

    public void StartNewGame() {
        SceneManager.LoadScene(newGameScene);
    }

    public void ExitApplication() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
