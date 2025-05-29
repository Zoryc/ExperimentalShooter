using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveBaseSpawner : MonoBehaviour
{
    public int initialZombiePerWave = 5;
    public int currentZombiesPerWave;

    public float spawnDelay = 0.5f;

    public int currentWave = 0;
    public float waveCooldown = 10.0f;

    public bool inCooldown;
    public float cooldownCounter = 0.0f;

    public GameObject zombiePrefab;

    public List<Enemy> currentZombiesAlive;

    public Canvas waveOverUI;
    public TextMeshProUGUI cooldownCouterUI;

    public TextMeshProUGUI currentWaveUI;

    private void Start()
    {
        currentZombiesPerWave = initialZombiePerWave;
        GlobalReferences.Instance.waveNumer = 0;
        StartNextWave();
    }

    private void StartNextWave()
    {
        currentZombiesAlive.Clear();

        currentWave++;
        GlobalReferences.Instance.waveNumer = currentWave;
        currentWaveUI.text = "Wave: " + currentWave.ToString();

        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < currentZombiesPerWave; i++)
        {
            // Generate a random offset within a specified range
            Vector3 spawnOffset = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));
            Vector3 spawnPosition = this.transform.position + spawnOffset;

            // Instantiate the zombie
            GameObject zombie = Instantiate<GameObject>(zombiePrefab, spawnPosition, Quaternion.identity);

            // Get the enemy script
            Enemy enemyScript = zombie.GetComponent<Enemy>();

            // Track this zombie
            currentZombiesAlive.Add(enemyScript);

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void Update()
    {
        // Get all dead zombie
        List<Enemy> zombiesToRemove = new List<Enemy>();
        foreach (Enemy zombie in currentZombiesAlive) {
            if (zombie.isDead) {
                zombiesToRemove.Add(zombie);
            }
        }

        // Actually remove all dead zombies
        foreach (Enemy zombie in zombiesToRemove) {
            currentZombiesAlive.Remove(zombie);
        }

        zombiesToRemove.Clear();

        // Start Cooldown if all zombies are dead
        if (currentZombiesAlive.Count == 0 && inCooldown == false)
        {
            StartCoroutine(StartWaveCooldown());
        }

        if (inCooldown)
        {
            cooldownCounter -= Time.deltaTime;
        }
        else 
        {
            cooldownCounter = waveCooldown;
        }

        cooldownCouterUI.text = cooldownCounter.ToString("F0");
    }

    private IEnumerator StartWaveCooldown()
    {
        inCooldown = true;
        waveOverUI.gameObject.SetActive(true);

        yield return new WaitForSeconds(waveCooldown);

        inCooldown = false;
        waveOverUI.gameObject.SetActive(false);

        currentZombiesPerWave *= 2;
        StartNextWave();
    }
}
