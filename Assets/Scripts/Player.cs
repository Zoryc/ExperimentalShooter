using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public int HP = 100;
    public GameObject bloodyScreen;

    public TextMeshProUGUI healthText;
    public GameObject gameOverUI;

    public bool isDead = false;

    private void Start()
    {
        healthText.text = $"Health: {HP}";
    }

    public void TakeDamage(int damageAmount) {
        HP -= damageAmount;

        if (HP <= 0)
        {
            Debug.LogWarning("Player is dead");
            PlayerDeath();
            isDead = true;
        }
        else 
        {
            Debug.LogWarning("Player hit");
            StartCoroutine(DisplayBloodyScreen());
            healthText.text = $"Health: {HP}";
        }
    }

    private void PlayerDeath()
    {
        this.GetComponent<MouseMovement>().enabled = false;
        this.GetComponent<PlayerMovement>().enabled = false;

        // Dying animation
        Camera.main.GetComponent<Animator>().enabled = true;
        healthText.gameObject.SetActive(false);

        gameOverUI.gameObject.SetActive(true);
        GetComponent<ScreenFader>().StartFade();

        int waveSurvived = GlobalReferences.Instance.waveNumer;
        if (waveSurvived - 1 > SessionManager.Instance.LoadHighScore()) 
        {
            SessionManager.Instance.SaveHighScore(waveSurvived - 1);
        }

        StartCoroutine(ReturnToMainMenu());

        SoundManager.Instance.shootingChannel.clip = SoundManager.Instance.gameOver;
        SoundManager.Instance.shootingChannel.PlayDelayed(1f);
    }

    private IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(4.0f);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator DisplayBloodyScreen()
    {
        if (bloodyScreen.activeInHierarchy == false)
            bloodyScreen.SetActive(true);

        var image = bloodyScreen.GetComponentInChildren<Image>();

        // Set the initial alpha value to 1 (fully visible).
        Color startColor = image.color;
        startColor.a = 1f;
        image.color = startColor;

        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the new alpha value using Lerp.
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            // Update the color with the new alpha value.
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;

            // Increment the elapsed time.
            elapsedTime += Time.deltaTime;

            yield return null; ; // Wait for the next frame.
        }

        if (bloodyScreen.activeInHierarchy)
            bloodyScreen.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (!isDead && other.gameObject.CompareTag("EnemyHand"))
        {
            SoundManager.Instance.shootingChannel.clip = SoundManager.Instance.playerHit;
            SoundManager.Instance.shootingChannel.Play();

            TakeDamage((int)other.gameObject.GetComponent<ZombieHand>().damage);
        }
    }
}
