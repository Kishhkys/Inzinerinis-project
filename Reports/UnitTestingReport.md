# Unit and Integration Testing Report

## Project

- Project: `Inzinerinis-project`
- Engine: Unity `6000.3.6f1`
- Testing framework: Unity Test Framework with NUnit
- Test categories: Edit Mode unit tests and Play Mode integration tests

## Goal

The goal of this work is to create automated tests for the game project, verify the behavior of important gameplay systems, and demonstrate how unit testing and integration testing can be applied in a Unity-based application.

## Why Unity Test Framework

Unity Test Framework is the most suitable platform for this project because it supports:

- fast Edit Mode tests for logic-heavy scripts
- Play Mode tests for MonoBehaviours, UI, GameObjects, timing, and scene-style interactions
- NUnit assertions and Unity-specific coroutine-based tests
- future compatibility with Unity Code Coverage for coverage reports

## Implemented Test Structure

The project was organized into two test assemblies:

- `Assets/Tests/EditMode`
- `Assets/Tests/PlayMode`

This separation keeps simple logic tests fast and keeps runtime interaction tests isolated.

## Implemented Unit Tests

### 1. PathFinder tests

File: `Assets/Tests/EditMode/PathFinderEditModeTests.cs`

Covered behaviors:

- path is found when graph nodes are connected
- `null` is returned when the target node is unreachable
- a single-node path is returned when start and target resolve to the same node

Reason for selection:

`PathFinder` contains deterministic pathfinding logic and is one of the best candidates for automated unit testing.

### 2. ContextSolver tests

File: `Assets/Tests/EditMode/ContextSolverEditModeTests.cs`

Covered behaviors:

- movement direction is normalized when multiple steering interests are combined
- zero vector is returned when danger fully cancels interest

Reason for selection:

`ContextSolver` is a pure decision component and can be tested with stub steering behaviours.

### 3. ItemDictionary tests

File: `Assets/Tests/EditMode/ItemDictionaryEditModeTests.cs`

Covered behaviors:

- item IDs are assigned during initialization
- correct prefab is returned for known item IDs
- missing item IDs return `null` and log a warning

Reason for selection:

The dictionary is simple but important for item lookup and should behave predictably.

## Implemented Integration Tests

### 4. Pause menu tests

File: `Assets/Tests/PlayMode/PauseMenuPlayModeTests.cs`

Covered behaviors:

- pause menu start state hides the panel
- time scale is reset to `1`
- pause action shows the panel and stops time
- resume action hides the panel and restores time

Reason for selection:

This is a good Play Mode test because it verifies interaction between a MonoBehaviour, UI state, and Unity global time control.

### 5. Inventory interaction tests

File: `Assets/Tests/PlayMode/InventoryIntegrationPlayModeTests.cs`

Covered behaviors:

- items are added to the first available inventory slots
- selecting a slot enables the visual selection state
- using the selected item calls the item logic and clears the slot

Reason for selection:

This test verifies interaction between `InventoryManager`, `InventorySlot`, UI images, and `ItemSO`.

## Mocks, Stubs, and Drivers

The assignment requested research and usage of testing doubles. The created tests use simplified doubles appropriate for Unity:

- Stub steering behaviour:
  `ContextSolverEditModeTests` uses a fake `SteeringBehaviour` that returns predefined danger and interest arrays.
- Fake consumable item:
  `InventoryIntegrationPlayModeTests` uses a custom `ItemSO` subclass to record whether `Use` was called.
- Test driver setup:
  The tests create temporary `GameObject` instances and wire required components manually to drive the system under test.

These doubles reduce dependency on scenes, prefabs, and player input, making the tests stable and repeatable.

## Additional Improvements Made

During setup, a few accidental dependencies were removed from runtime scripts:

- removed `using NUnit.Framework;` from runtime gameplay scripts
- removed `using UnityEditor;` from `ItemDictionary`

These namespaces should not be left in production gameplay code because they belong to tests or editor-only code.

## What Is Covered and What Is Not Covered

Automated tests in this project should primarily cover:

- gameplay logic
- pathfinding and AI decision logic
- inventory rules
- health, pause, and UI state transitions
- interactions between components

They should not directly target:

- sprites, textures, sound files, or animations as isolated assets
- Unity engine internals
- third-party package code

Assets and presentation elements are usually validated through Play Mode tests or manual exploratory testing.

## Coverage Discussion

The implemented tests increase confidence in important systems, but they do not provide 100% coverage of the entire game. Reaching total coverage in a Unity game is usually unrealistic because many behaviors depend on scenes, physics, animation, audio, timing, and user interaction.

A more realistic goal is to cover:

- the most bug-prone gameplay logic
- systems with deterministic rules
- component interactions that are expensive to validate manually

For full course submission, Unity Code Coverage can be enabled in Package Manager and the generated report can be attached as evidence.

## Execution Status

The automated test suite was prepared in the project, but command-line execution could not be completed in the current environment because Unity batch mode reported that no valid headless editor license was available.

Because of that, the next step is to open the project in Unity Editor and run:

- Edit Mode tests from `Window -> General -> Test Runner`
- Play Mode tests from the same Test Runner window

If the course requires screenshots or a formal pass/fail report, those should be captured after running the tests locally in the licensed editor session.

## Recommended Next Tests

The next useful tests for this project would be:

- `PlayerHealth` death and respawn behavior
- loot pickup and event flow
- `EnemyAI` state transitions between patrol, chase, wait, and investigate
- UI controller tests for inventory hotbar and item pickup popups

## Conclusion

The project now contains a maintainable Unity test structure with both unit and integration tests. The chosen tests focus on deterministic gameplay logic and realistic component interactions, which gives better value than trying to test only isolated methods or only visual assets.
