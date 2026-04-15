using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ContextSolverEditModeTests
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
    public void GetDirectionToMove_NormalizesCombinedInterest()
    {
        ContextSolver solver = CreateSolver();
        AIData aiData = CreateAIData();

        FakeSteeringBehaviour firstBehaviour = CreateBehaviour(
            new float[8],
            new[] { 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f });

        FakeSteeringBehaviour secondBehaviour = CreateBehaviour(
            new float[8],
            new[] { 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f });

        Vector2 direction = solver.GetDirectionToMove(
            new List<SteeringBehaviour> { firstBehaviour, secondBehaviour },
            aiData);

        Assert.That(direction.x, Is.EqualTo(0.7071f).Within(0.001f));
        Assert.That(direction.y, Is.EqualTo(0.7071f).Within(0.001f));
    }

    [Test]
    public void GetDirectionToMove_ReturnsZero_WhenDangerCancelsInterest()
    {
        ContextSolver solver = CreateSolver();
        AIData aiData = CreateAIData();

        FakeSteeringBehaviour behaviour = CreateBehaviour(
            new[] { 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f },
            new[] { 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f });

        Vector2 direction = solver.GetDirectionToMove(new List<SteeringBehaviour> { behaviour }, aiData);

        Assert.That(direction, Is.EqualTo(Vector2.zero));
    }

    private ContextSolver CreateSolver()
    {
        GameObject gameObject = new("ContextSolver");
        createdObjects.Add(gameObject);
        return gameObject.AddComponent<ContextSolver>();
    }

    private AIData CreateAIData()
    {
        GameObject gameObject = new("AIData");
        createdObjects.Add(gameObject);
        return gameObject.AddComponent<AIData>();
    }

    private FakeSteeringBehaviour CreateBehaviour(float[] dangerContribution, float[] interestContribution)
    {
        GameObject gameObject = new("FakeSteering");
        createdObjects.Add(gameObject);

        FakeSteeringBehaviour behaviour = gameObject.AddComponent<FakeSteeringBehaviour>();
        behaviour.DangerContribution = dangerContribution;
        behaviour.InterestContribution = interestContribution;
        return behaviour;
    }

    private class FakeSteeringBehaviour : SteeringBehaviour
    {
        public float[] DangerContribution { get; set; }
        public float[] InterestContribution { get; set; }

        public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
        {
            for (int i = 0; i < 8; i++)
            {
                danger[i] += DangerContribution[i];
                interest[i] += InterestContribution[i];
            }

            return (danger, interest);
        }
    }
}
