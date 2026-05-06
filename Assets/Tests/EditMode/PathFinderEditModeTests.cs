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
    public void FindsPathThroughConnectedNodes()
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
    public void ReturnsNoPathForUnreachableTarget()
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
    public void ReturnsNoPathWhenNoNodesAreConfigured()
    {
        PathFinder pathFinder = CreatePathFinder();

        List<PathNode> path = pathFinder.FindPath(Vector2.zero, Vector2.one);

        Assert.That(path, Is.Null);
    }

    [Test]
    public void IgnoresNullNodesAndNullNeighbours()
    {
        PathNode start = CreateNode("Start", Vector2.zero);
        PathNode target = CreateNode("Target", Vector2.right);
        start.neighbours.Add(null);
        ConnectBidirectional(start, target);

        PathFinder pathFinder = CreatePathFinder(start, null, target);

        List<PathNode> path = pathFinder.FindPath(Vector2.zero, Vector2.right);

        CollectionAssert.AreEqual(new[] { start, target }, path);
    }

    [Test]
    public void CanFindPathWithoutLineOfSightStartFiltering()
    {
        PathNode start = CreateNode("Start", Vector2.zero);
        PathNode target = CreateNode("Target", Vector2.right);
        ConnectBidirectional(start, target);

        PathFinder pathFinder = CreatePathFinder(start, target);
        SetPrivateField(pathFinder, "requireLineOfSightToStartNode", false);

        List<PathNode> path = pathFinder.FindPath(Vector2.zero, Vector2.right);

        CollectionAssert.AreEqual(new[] { start, target }, path);
    }

    [Test]
    public void SkipsAlreadyOpenNeighbourWhenNewPathIsNotBetter()
    {
        PathNode start = CreateNode("Start", new Vector2(0f, 0f));
        PathNode middle = CreateNode("Middle", new Vector2(1f, 0f));
        PathNode alreadyOpen = CreateNode("AlreadyOpen", new Vector2(2f, 0f));
        PathNode target = CreateNode("Target", new Vector2(1f, 1f));

        start.neighbours.Add(alreadyOpen);
        start.neighbours.Add(middle);
        middle.neighbours.Add(alreadyOpen);
        middle.neighbours.Add(target);
        alreadyOpen.neighbours.Add(target);

        PathFinder pathFinder = CreatePathFinder(start, middle, alreadyOpen, target);

        List<PathNode> path = pathFinder.FindPath(Vector2.zero, target.transform.position);

        CollectionAssert.AreEqual(new[] { start, middle, target }, path);
    }

    [Test]
    public void FallsBackToClosestNodeWhenLineOfSightIsBlocked()
    {
        PathNode start = CreateNode("Start", Vector2.right * 2f);
        PathNode target = CreateNode("Target", Vector2.right * 3f);
        ConnectBidirectional(start, target);

        GameObject wall = new("Wall");
        createdObjects.Add(wall);
        wall.layer = LayerMask.NameToLayer("Ignore Raycast");
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.2f, 5f);
        wall.transform.position = Vector2.right;
        Physics2D.SyncTransforms();

        PathFinder pathFinder = CreatePathFinder(start, target);
        LayerMask obstacleMask = LayerMask.GetMask("Ignore Raycast");
        SetPrivateField(pathFinder, "obstacleMask", obstacleMask);

        List<PathNode> path = pathFinder.FindPath(Vector2.zero, target.transform.position);

        CollectionAssert.AreEqual(new[] { start, target }, path);
    }

    [TestCase(4.1f, 4f, 3.9f, 4f)]
    [TestCase(3.7f, 4.2f, 4.3f, 3.8f)]
    public void ReturnsSingleNodeWhenStartAndTargetAreTheSame(
        float startX,
        float startY,
        float targetX,
        float targetY)
    {
        PathNode onlyNode = CreateNode("OnlyNode", new Vector2(4f, 4f));
        PathFinder pathFinder = CreatePathFinder(onlyNode);

        List<PathNode> path = pathFinder.FindPath(new Vector2(startX, startY), new Vector2(targetX, targetY));

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

    private static void SetPrivateField<T>(PathFinder pathFinder, string fieldName, T value)
    {
        FieldInfo field = typeof(PathFinder).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(pathFinder, value);
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
