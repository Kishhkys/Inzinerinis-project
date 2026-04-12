//using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using System;

public class PlayerHealth : MonoBehaviour
{
    private float health = 0f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float respawnDelay = 5f;
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private GameObject bloodPrefab;
    [SerializeField] private CinemachineCamera virtualCamera;

    private Vector2 startPosition;

    void Start()
    {
        health = maxHealth;
        startPosition = transform.position;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = maxHealth;
        }
    }

    public void UpdateHealth(float mod, Vector2 hitPosition = default)
    {
        if (health <= 0) return;
        health += mod;
        if (mod < 0)
        {
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.NotifyDamaged();
            }

            float volume = Mathf.Clamp01(Mathf.Abs(mod) / 10f);
            SoundEffectManager.PlayClip("Health", "Hit", volume);

            if (bloodPrefab != null)
            {
                Vector2 spawnPos = hitPosition == default ? (Vector2)transform.position : hitPosition;
                GameObject blood = Instantiate(bloodPrefab, spawnPos, Quaternion.identity);
                Destroy(blood, 0.5f);
            }
        }

        if (health > maxHealth) health = maxHealth;
        else if (health <= 0f)
        {
            health = 0f;
            Die();
        }
        if (healthBar != null) healthBar.value = health;
        Debug.Log($"Health: {health}");
    }


    private void Die()
    {
        GetComponent<PlayerController>().StopFootsteps();
        GetComponent<PlayerController>().enabled = false;
        GetComponent<PlayerController>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        SoundEffectManager.PlayClip("Health", "Player_death");
        if (virtualCamera != null)
        {
            virtualCamera.Follow = null;
            virtualCamera.LookAt = null;
        }
        if (deathScreen != null) deathScreen.SetActive(true);
        Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        health = maxHealth;
        transform.position = startPosition;
        if (virtualCamera != null)
        {
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
        }
        GetComponent<PlayerController>().enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        if (healthBar != null) healthBar.value = health;
        if (deathScreen != null) deathScreen.SetActive(false);
        ResetEnemies();
    }

    private void ResetEnemies()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            enemy.ResetToStart();
        }
    }

}
