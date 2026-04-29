using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class ItemPickupUIControllerPlayModeTests
{
    private readonly List<Object> createdObjects = new();

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        ResetSingleton();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        ResetSingleton();

        foreach (Object createdObject in createdObjects)
        {
            if (createdObject != null)
            {
                Object.Destroy(createdObject);
            }
        }

        createdObjects.Clear();
        yield return null;
        ResetSingleton();
    }

    [UnityTest]
    public IEnumerator BecomesTheActivePickupUiInstance()
    {
        ItemPickupUIController controller = CreateController();

        yield return null;

        Assert.That(ItemPickupUIController.Instance, Is.EqualTo(controller));
    }

    [UnityTest]
    public IEnumerator ShowsPickedUpItemNameAndIcon()
    {
        ItemPickupUIController controller = CreateController();
        Sprite icon = CreateSprite(Color.red);

        yield return null;

        controller.ShowItemPickup("Rusty key", icon);
        yield return null;

        Assert.That(controller.transform.childCount, Is.EqualTo(1));

        Transform popup = controller.transform.GetChild(0);
        Assert.That(popup.GetComponentInChildren<TMP_Text>().text, Is.EqualTo("Rusty key"));
        Assert.That(popup.Find("ItemIcon").GetComponent<Image>().sprite, Is.EqualTo(icon));
    }

    [UnityTest]
    public IEnumerator RemovesOldestPopupWhenLimitIsReached()
    {
        ItemPickupUIController controller = CreateController();
        controller.maxPopups = 1;

        yield return null;

        controller.ShowItemPickup("First", CreateSprite(Color.blue));
        yield return null;
        GameObject firstPopup = controller.transform.GetChild(0).gameObject;

        controller.ShowItemPickup("Second", CreateSprite(Color.green));
        yield return null;

        Assert.That(firstPopup == null, Is.True);
        Assert.That(controller.transform.childCount, Is.EqualTo(1));
        Assert.That(controller.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text, Is.EqualTo("Second"));
    }

    private ItemPickupUIController CreateController()
    {
        GameObject controllerObject = new("ItemPickupUIController", typeof(RectTransform));
        createdObjects.Add(controllerObject);

        ItemPickupUIController controller = controllerObject.AddComponent<ItemPickupUIController>();
        controller.popupPrefab = CreatePopupPrefab();
        controller.popupDuration = 10f;
        return controller;
    }

    private GameObject CreatePopupPrefab()
    {
        GameObject popup = new("PopupPrefab", typeof(RectTransform), typeof(CanvasGroup));
        createdObjects.Add(popup);

        GameObject textObject = new("ItemName", typeof(RectTransform), typeof(TextMeshProUGUI));
        createdObjects.Add(textObject);
        textObject.transform.SetParent(popup.transform, false);

        GameObject iconObject = new("ItemIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        createdObjects.Add(iconObject);
        iconObject.transform.SetParent(popup.transform, false);

        return popup;
    }

    private Sprite CreateSprite(Color color)
    {
        Texture2D texture = new(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        createdObjects.Add(texture);

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
        createdObjects.Add(sprite);
        return sprite;
    }

    private static void ResetSingleton()
    {
        FieldInfo field = typeof(ItemPickupUIController).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        field.SetValue(null, null);
    }
}
