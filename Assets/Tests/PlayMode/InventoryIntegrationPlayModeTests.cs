using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class InventoryIntegrationPlayModeTests
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
    public IEnumerator AddItem_PlacesItemInFirstEmptySlot_AndUseConsumesSelectedItem()
    {
        InventoryManager inventoryManager = CreateInventoryManager(out InventorySlot firstSlot, out InventorySlot secondSlot, out GameObject player);
        TestConsumableItem firstItem = ScriptableObject.CreateInstance<TestConsumableItem>();
        TestConsumableItem secondItem = ScriptableObject.CreateInstance<TestConsumableItem>();
        createdObjects.Add(firstItem);
        createdObjects.Add(secondItem);

        yield return null;

        inventoryManager.AddItem(firstItem);
        inventoryManager.AddItem(secondItem);

        Assert.That(firstSlot.itemSO, Is.EqualTo(firstItem));
        Assert.That(secondSlot.itemSO, Is.EqualTo(secondItem));
        Assert.That(firstSlot.itemImage.gameObject.activeSelf, Is.True);
        Assert.That(secondSlot.itemImage.gameObject.activeSelf, Is.True);

        inventoryManager.SelectSlot(firstSlot);
        Assert.That(firstSlot.selectionHighlight.gameObject.activeSelf, Is.True);
        Assert.That(secondSlot.selectionHighlight.gameObject.activeSelf, Is.False);

        InvokeUseSelectedItem(inventoryManager);

        Assert.That(firstItem.UseCount, Is.EqualTo(1));
        Assert.That(firstItem.LastUser, Is.EqualTo(player));
        Assert.That(firstSlot.itemSO, Is.Null);
        Assert.That(firstSlot.itemImage.gameObject.activeSelf, Is.False);
        Assert.That(secondSlot.itemSO, Is.EqualTo(secondItem));
    }

    private InventoryManager CreateInventoryManager(out InventorySlot firstSlot, out InventorySlot secondSlot, out GameObject player)
    {
        GameObject managerObject = new("InventoryManager");
        createdObjects.Add(managerObject);

        InventoryManager inventoryManager = managerObject.AddComponent<InventoryManager>();
        player = new GameObject("Player");
        createdObjects.Add(player);

        firstSlot = CreateSlot("Slot1");
        secondSlot = CreateSlot("Slot2");
        inventoryManager.itemSlots = new[] { firstSlot, secondSlot };

        FieldInfo playerField = typeof(InventoryManager).GetField("player", BindingFlags.Instance | BindingFlags.NonPublic);
        playerField.SetValue(inventoryManager, player);
        return inventoryManager;
    }

    private InventorySlot CreateSlot(string name)
    {
        GameObject slotObject = new(name);
        createdObjects.Add(slotObject);

        InventorySlot slot = slotObject.AddComponent<InventorySlot>();
        slot.itemImage = CreateImageObject($"{name}_ItemImage").GetComponent<Image>();
        slot.selectionHighlight = CreateImageObject($"{name}_Selection").GetComponent<Image>();
        slot.selectionHighlight.gameObject.SetActive(false);
        return slot;
    }

    private GameObject CreateImageObject(string name)
    {
        GameObject imageObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        createdObjects.Add(imageObject);
        return imageObject;
    }

    private static void InvokeUseSelectedItem(InventoryManager inventoryManager)
    {
        MethodInfo useSelectedItemMethod = typeof(InventoryManager).GetMethod("UseSelectedItem", BindingFlags.Instance | BindingFlags.NonPublic);
        useSelectedItemMethod.Invoke(inventoryManager, null);
    }

    private class TestConsumableItem : ItemSO
    {
        public int UseCount { get; private set; }
        public GameObject LastUser { get; private set; }

        public override void Use(GameObject user)
        {
            UseCount++;
            LastUser = user;
        }
    }
}
