using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class PlayerHealthPlayModeTests
{
    private readonly List<Object> createdObjects = new();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (Object createdObject in createdObjects)
        {
            if (createdObject != null)
            {
                Object.Destroy(createdObject);
            }
        }

        createdObjects.Clear();
        yield return null;
    }

    [UnityTest]
    public IEnumerator StartsWithFullHealth()
    {
        PlayerHealth playerHealth = CreatePlayerHealth(includeDeathDependencies: false, out Slider healthBar);

        yield return null;

        Assert.That(GetPrivateFloat(playerHealth, "health"), Is.EqualTo(100f));
        Assert.That(healthBar.maxValue, Is.EqualTo(100f));
        Assert.That(healthBar.value, Is.EqualTo(100f));
    }

    [UnityTest]
    public IEnumerator HealingStopsAtMaximumHealth()
    {
        PlayerHealth playerHealth = CreatePlayerHealth(includeDeathDependencies: false, out Slider healthBar);

        yield return null;

        playerHealth.UpdateHealth(-30f);
        playerHealth.UpdateHealth(75f);

        Assert.That(GetPrivateFloat(playerHealth, "health"), Is.EqualTo(100f));
        Assert.That(healthBar.value, Is.EqualTo(100f));
    }

    [UnityTest]
    public IEnumerator FatalDamageDisablesPlayerUntilRespawn()
    {
        PlayerHealth playerHealth = CreatePlayerHealth(includeDeathDependencies: true, out Slider healthBar);
        GameObject playerObject = playerHealth.gameObject;
        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        SpriteRenderer spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
        Collider2D collider = playerObject.GetComponent<Collider2D>();

        playerObject.transform.position = new Vector3(3f, 4f, 0f);

        yield return null;
        playerController.enabled = false;

        playerHealth.UpdateHealth(-150f);

        Assert.That(GetPrivateFloat(playerHealth, "health"), Is.EqualTo(0f));
        Assert.That(playerController.enabled, Is.False);
        Assert.That(spriteRenderer.enabled, Is.False);
        Assert.That(collider.enabled, Is.False);
        Assert.That(playerHealth.inventoryPanel.activeSelf, Is.False);
        Assert.That(playerHealth.popupPanel.activeSelf, Is.False);
        Assert.That(healthBar.value, Is.EqualTo(0f));

        InvokePrivateMethod(playerHealth, "Respawn");
        playerController.enabled = false;

        Assert.That(GetPrivateFloat(playerHealth, "health"), Is.EqualTo(100f));
        Assert.That(playerObject.transform.position, Is.EqualTo(new Vector3(3f, 4f, 0f)));
        Assert.That(spriteRenderer.enabled, Is.True);
        Assert.That(collider.enabled, Is.True);
        Assert.That(playerHealth.inventoryPanel.activeSelf, Is.True);
        Assert.That(playerHealth.popupPanel.activeSelf, Is.True);
        Assert.That(healthBar.value, Is.EqualTo(100f));
    }

    private PlayerHealth CreatePlayerHealth(bool includeDeathDependencies, out Slider healthBar)
    {
        GameObject playerObject = new("Player");
        playerObject.SetActive(false);
        createdObjects.Add(playerObject);

        PlayerHealth playerHealth = playerObject.AddComponent<PlayerHealth>();
        healthBar = CreateSlider("HealthBar");
        SetPrivateField(playerHealth, "healthBar", healthBar);
        SetPrivateField(playerHealth, "maxHealth", 100f);
        SetPrivateField(playerHealth, "respawnDelay", 0.05f);

        if (includeDeathDependencies)
        {
            playerObject.AddComponent<Rigidbody2D>();
            playerObject.AddComponent<BoxCollider2D>();
            playerObject.AddComponent<SpriteRenderer>();
            PlayerController playerController = playerObject.AddComponent<PlayerController>();
            playerController.enabled = false;

            playerHealth.inventoryPanel = CreatePanel("InventoryPanel");
            playerHealth.popupPanel = CreatePanel("PopupPanel");
        }

        playerObject.SetActive(true);
        return playerHealth;
    }

    private Slider CreateSlider(string name)
    {
        GameObject sliderObject = new(name, typeof(RectTransform), typeof(Slider));
        createdObjects.Add(sliderObject);
        return sliderObject.GetComponent<Slider>();
    }

    private GameObject CreatePanel(string name)
    {
        GameObject panel = new(name);
        createdObjects.Add(panel);
        panel.SetActive(true);
        return panel;
    }

    private static void SetPrivateField<T>(PlayerHealth playerHealth, string fieldName, T value)
    {
        FieldInfo field = typeof(PlayerHealth).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(playerHealth, value);
    }

    private static float GetPrivateFloat(PlayerHealth playerHealth, string fieldName)
    {
        FieldInfo field = typeof(PlayerHealth).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return (float)field.GetValue(playerHealth);
    }

    private static void InvokePrivateMethod(PlayerHealth playerHealth, string methodName)
    {
        MethodInfo method = typeof(PlayerHealth).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(playerHealth, null);
    }
}
