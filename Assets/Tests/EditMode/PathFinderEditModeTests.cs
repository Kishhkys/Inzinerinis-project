using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class PathFinderEditModeTests
{
    private readonly List<Object> createdObjects = new();

    [TearDown]
    public void TearDown()
    {
        foreach (Object createdObject in createdObjects.Where(obj => obj != null))
        {
            Object.DestroyImmediate(createdObject);
        }

        createdObjects.Clear();
    }

    [Test]
    public void FindPath_ReturnsOrderedPath_WhenNodesAreConnected()
    {
        PathNode start = CreateNode("Start", new Vector2(0f, 0f));
        PathNode middle = CreateNode("Middle", new Vector2(1f, 0f));
        PathNode target = CreateNode("Target", new Vector2(2f, 0f));

        ConnectBidirectional(start, middle);
        ConnectBidirectional(middle, target);

        PathFinder pathFinder = CreatePathFinder(start, middle, target);

        List<PathNode> path = pathFinder.FindPath(new Vector2(-0.1f, 0f), new Vector2(2.1f, 0f));

        Assert.That(path, Is.Not.Null);
        CollectionAssert.AreEqual(new[] { start, middle, target }, path);
    }

    [Test]
    public void FindPath_ReturnsNull_WhenTargetCannotBeReached()
    {
        PathNode start = CreateNode("Start", Vector2.zero);
        PathNode connected = CreateNode("Connected", Vector2.right);
        PathNode isolated = CreateNode("Isolated", Vector2.up * 5f);

        ConnectBidirectional(start, connected);

        PathFinder pathFinder = CreatePathFinder(start, connected, isolated);

        List<PathNode> path = pathFinder.FindPath(Vector2.zero, isolated.transform.position);

        Assert.That(path, Is.Null);
    }

    [Test]
    public void FindPath_ReturnsSingleNode_WhenStartAndTargetResolveToSameNode()
    {
        PathNode onlyNode = CreateNode("OnlyNode", new Vector2(4f, 4f));
        PathFinder pathFinder = CreatePathFinder(onlyNode);

        List<PathNode> path = pathFinder.FindPath(new Vector2(4.1f, 4f), new Vector2(3.9f, 4f));

        Assert.That(path, Is.Not.Null);
        Assert.That(path.Count, Is.EqualTo(1));
        Assert.That(path[0], Is.EqualTo(onlyNode));
    }

    private PathFinder CreatePathFinder(params PathNode[] nodes)
    {
        GameObject gameObject = new("PathFinder");
        createdObjects.Add(gameObject);

        PathFinder pathFinder = gameObject.AddComponent<PathFinder>();
        FieldInfo allNodesField = typeof(PathFinder).GetField("allNodes", BindingFlags.Instance | BindingFlags.NonPublic);
        allNodesField.SetValue(pathFinder, nodes.ToList());
        return pathFinder;
    }

    private PathNode CreateNode(string name, Vector2 position)
    {
        GameObject gameObject = new(name);
        createdObjects.Add(gameObject);
        gameObject.transform.position = position;
        return gameObject.AddComponent<PathNode>();
    }

    private static void ConnectBidirectional(PathNode first, PathNode second)
    {
        first.neighbours.Add(second);
        second.neighbours.Add(first);
    }
}
