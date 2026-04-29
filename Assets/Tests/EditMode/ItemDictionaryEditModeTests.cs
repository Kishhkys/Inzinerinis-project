using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ItemDictionaryEditModeTests
{
    private readonly List<Object> createdObjects = new();

    [TearDown]
    public void TearDown()
    {
        foreach (Object createdObject in createdObjects)
        {
            if (createdObject != null)
            {
                Object.DestroyImmediate(createdObject);
            }
        }

        createdObjects.Clear();
    }

    [Test]
    public void AssignsItemIdsWhenDictionaryStarts()
    {
        Item firstItem = CreateItem("First Item");
        Item secondItem = CreateItem("Second Item");
        ItemDictionary dictionary = CreateDictionary(firstItem, secondItem);

        InvokeAwake(dictionary);

        Assert.That(firstItem.ID, Is.EqualTo(1));
        Assert.That(secondItem.ID, Is.EqualTo(2));
        Assert.That(dictionary.GetItemPrefab(1), Is.EqualTo(firstItem.gameObject));
        Assert.That(dictionary.GetItemPrefab(2), Is.EqualTo(secondItem.gameObject));
    }

    [Test]
    public void WarnsWhenItemIdIsMissing()
    {
        ItemDictionary dictionary = CreateDictionary(CreateItem("Only Item"));
        InvokeAwake(dictionary);

        LogAssert.Expect(LogType.Warning, "Item with ID 99 is not found in the dictionary");

        GameObject prefab = dictionary.GetItemPrefab(99);

        Assert.That(prefab, Is.Null);
    }

    private ItemDictionary CreateDictionary(params Item[] items)
    {
        GameObject gameObject = new("ItemDictionary");
        createdObjects.Add(gameObject);

        ItemDictionary dictionary = gameObject.AddComponent<ItemDictionary>();
        dictionary.itemPrefabs = new List<Item>(items);
        return dictionary;
    }

    private Item CreateItem(string name)
    {
        GameObject gameObject = new(name);
        createdObjects.Add(gameObject);
        return gameObject.AddComponent<Item>();
    }

    private static void InvokeAwake(ItemDictionary dictionary)
    {
        MethodInfo awakeMethod = typeof(ItemDictionary).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
        awakeMethod.Invoke(dictionary, null);
    }
}
