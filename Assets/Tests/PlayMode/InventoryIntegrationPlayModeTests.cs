using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
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
    public IEnumerator AddsItemsToOpenSlotsAndUsesSelectedItem()
    {
        InventoryController inventoryController = CreateInventoryController(out GameObject inventoryPanel);
        TestInventoryItem firstItemPrefab = CreateItemPrefab("First item", 7);
        TestInventoryItem secondItemPrefab = CreateItemPrefab("Second item", 8);

        yield return null;

        Assert.That(inventoryPanel.transform.childCount, Is.EqualTo(2));

        Slot firstSlot = inventoryPanel.transform.GetChild(0).GetComponent<Slot>();
        Slot secondSlot = inventoryPanel.transform.GetChild(1).GetComponent<Slot>();

        Assert.That(firstSlot.GetComponent<Image>().sprite, Is.EqualTo(firstSlot.activeSprite));
        Assert.That(secondSlot.GetComponent<Image>().sprite, Is.EqualTo(secondSlot.regularSprite));

        Assert.That(inventoryController.AddItem(firstItemPrefab.gameObject), Is.True);
        Assert.That(inventoryController.AddItem(secondItemPrefab.gameObject), Is.True);

        TestInventoryItem firstRuntimeItem = firstSlot.currentItem.GetComponent<TestInventoryItem>();
        TestInventoryItem secondRuntimeItem = secondSlot.currentItem.GetComponent<TestInventoryItem>();

        Assert.That(firstRuntimeItem.ID, Is.EqualTo(firstItemPrefab.ID));
        Assert.That(firstRuntimeItem.Name, Is.EqualTo(firstItemPrefab.Name));
        Assert.That(secondRuntimeItem.ID, Is.EqualTo(secondItemPrefab.ID));
        Assert.That(inventoryController.GetSelectedItem(), Is.EqualTo(firstRuntimeItem));

        inventoryController.UseSelectedItem();

        Assert.That(firstRuntimeItem.UseCount, Is.EqualTo(1));

        inventoryController.RemoveSelectedItem();
        yield return null;

        Assert.That(firstSlot.currentItem, Is.Null);
        Assert.That(secondSlot.currentItem, Is.EqualTo(secondRuntimeItem.gameObject));
    }

    [UnityTest]
    public IEnumerator RefusesItemsWhenInventoryIsFull()
    {
        InventoryController inventoryController = CreateInventoryController(out _);
        TestInventoryItem firstItemPrefab = CreateItemPrefab("First item", 1);
        TestInventoryItem secondItemPrefab = CreateItemPrefab("Second item", 2);
        TestInventoryItem overflowItemPrefab = CreateItemPrefab("Overflow item", 3);

        yield return null;

        Assert.That(inventoryController.AddItem(firstItemPrefab.gameObject), Is.True);
        Assert.That(inventoryController.AddItem(secondItemPrefab.gameObject), Is.True);

        LogAssert.Expect(LogType.Log, "Inventory is full");
        Assert.That(inventoryController.AddItem(overflowItemPrefab.gameObject), Is.False);
    }

    private InventoryController CreateInventoryController(out GameObject inventoryPanel)
    {
        GameObject controllerObject = new("InventoryController");
        controllerObject.SetActive(false);
        createdObjects.Add(controllerObject);

        inventoryPanel = new GameObject("InventoryPanel", typeof(RectTransform));
        createdObjects.Add(inventoryPanel);

        InventoryController inventoryController = controllerObject.AddComponent<InventoryController>();
        inventoryController.inventoryPanel = inventoryPanel;
        inventoryController.slotPrefab = CreateSlotPrefab();

        FieldInfo slotCountField = typeof(InventoryController).GetField("slotCount", BindingFlags.Instance | BindingFlags.NonPublic);
        slotCountField.SetValue(inventoryController, 2);

        controllerObject.SetActive(true);
        inventoryController.enabled = false;
        return inventoryController;
    }

    private GameObject CreateSlotPrefab()
    {
        GameObject slotPrefab = new("SlotPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slot));
        createdObjects.Add(slotPrefab);

        Slot slot = slotPrefab.GetComponent<Slot>();
        slot.slotNum = CreateSlotNumber(slotPrefab.transform);
        slot.regularSprite = CreateSprite(Color.gray);
        slot.activeSprite = CreateSprite(Color.green);
        slotPrefab.GetComponent<Image>().sprite = slot.regularSprite;

        return slotPrefab;
    }

    private TextMeshProUGUI CreateSlotNumber(Transform parent)
    {
        GameObject labelObject = new("SlotNumber", typeof(RectTransform), typeof(TextMeshProUGUI));
        createdObjects.Add(labelObject);
        labelObject.transform.SetParent(parent, false);
        return labelObject.GetComponent<TextMeshProUGUI>();
    }

    private TestInventoryItem CreateItemPrefab(string itemName, int id)
    {
        GameObject itemPrefab = new(itemName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TestInventoryItem));
        createdObjects.Add(itemPrefab);

        TestInventoryItem item = itemPrefab.GetComponent<TestInventoryItem>();
        item.ID = id;
        item.Name = itemName;
        return item;
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

    private class TestInventoryItem : Item
    {
        public int UseCount { get; private set; }

        public override void UseItem()
        {
            UseCount++;
        }
    }
}
