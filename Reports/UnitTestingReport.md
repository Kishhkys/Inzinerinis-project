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

The gameplay scripts were also grouped under a dedicated runtime assembly definition in `Assets/Scripts/InzinerinisProject.Runtime.asmdef`.

This separation keeps simple logic tests fast, keeps runtime interaction tests isolated, and allows the test assemblies to reference gameplay code reliably.

## Implemented Unit Tests

### 1. PathFinder tests

File: `Assets/Tests/EditMode/PathFinderEditModeTests.cs`

Covered behaviors:

- path is found when graph nodes are connected
- `null` is returned when the target node is unreachable
- a single-node path is returned when start and target resolve to the same node
- parameterized start and target positions are checked with NUnit `TestCase`

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

- `InventoryController` creates the configured number of UI slots from a slot prefab
- the first slot is selected on startup and receives the active slot sprite
- item prefabs are added to the first available slots
- item ID and name data are copied from the source prefab to the runtime inventory item
- using the selected item calls the item logic
- removing the selected item clears only that selected slot
- adding another item fails and logs a message when all slots are full

Reason for selection:

This test verifies interaction between `InventoryController`, `Slot`, UI images, TextMesh Pro slot labels, and `Item` prefabs. It was updated after the inventory scripts changed from the older ScriptableObject-based inventory path to the newer prefab/slot-based controller used by current gameplay interactions.

### 6. Player health tests

File: `Assets/Tests/PlayMode/PlayerHealthPlayModeTests.cs`

Covered behaviors:

- `Start` initializes current health and health bar values
- healing cannot raise health above maximum health
- fatal damage disables the player controller, sprite renderer, collider, inventory panel, and popup panel
- respawn restores health, UI panels, renderer, collider, and start position

Reason for selection:

`PlayerHealth` is a central gameplay component. These tests cover both normal state updates and a higher-risk death/respawn workflow that affects several connected components.

### 7. Item pickup popup tests

File: `Assets/Tests/PlayMode/ItemPickupUIControllerPlayModeTests.cs`

Covered behaviors:

- the pickup UI controller assigns its singleton instance
- pickup popups display the expected item name and icon
- the oldest popup is removed when the configured popup limit is exceeded

Reason for selection:

The pickup popup is a small but visible UI integration point. These tests verify the interaction between `ItemPickupUIController`, generated popup prefabs, TextMesh Pro text, and UI image components.

## Mocks, Stubs, and Drivers

The assignment requested research and usage of testing doubles. The created tests use simplified doubles appropriate for Unity:

- Stub steering behaviour:
  `ContextSolverEditModeTests` uses a fake `SteeringBehaviour` that returns predefined danger and interest arrays.
- Fake inventory item:
  `InventoryIntegrationPlayModeTests` uses a custom `Item` subclass to record whether `UseItem` was called.
- Generated UI prefabs:
  `InventoryIntegrationPlayModeTests` and `ItemPickupUIControllerPlayModeTests` create lightweight slot and popup prefabs during setup instead of depending on scene assets.
- Test driver setup:
  The tests create temporary `GameObject` instances and wire required components manually to drive the system under test.

These doubles reduce dependency on scenes, prefabs, and player input, making the tests stable and repeatable.

## Parameterized Tests

The assignment requested research and usage of parameterized tests. `PathFinderEditModeTests` uses NUnit `TestCase` data for the single-node path scenario, so the same expected behavior is checked against multiple start and target positions without duplicating the test body.

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

The automated test suite was updated and Unity successfully recompiled the changed Play Mode test assembly after adding the missing `Unity.TextMeshPro` reference to `Assets/Tests/PlayMode/InzinerinisProject.PlayModeTests.asmdef`.

Command-line test execution was attempted with Unity `6000.3.6f1`, but no XML test result was produced from batch mode in this environment. The first run was blocked by Unity's scene backup recovery prompt caused by `Assets/Scenes/SampleScene.unity.backup`. After temporarily moving that backup out of `Assets`, Unity compiled the tests successfully. Later batch test runner attempts failed before execution because the Unity Package Manager local server did not connect within 30 seconds.

The tests were also run from the active Unity Editor session. The generated result file at `C:/Users/Martynas/AppData/LocalLow/DefaultCompany/Ward13/TestResults.xml` reported the Edit Mode suite as passed: 8 total, 8 passed, 0 failed. The newly added Play Mode tests for `PlayerHealth` and `ItemPickupUIController` should be run from the Play Mode tab after Unity imports the new files.

The next verification step is to open the project in the Unity Editor and run:

- Edit Mode tests from `Window -> General -> Test Runner`
- Play Mode tests from the same Test Runner window

If the course requires screenshots or a formal pass/fail report, capture those after the tests run inside the normal editor session. Unity Code Coverage can also be enabled before that run if coverage evidence is required.

## Recommended Next Tests

The next useful tests for this project would be:

- loot pickup and event flow
- `EnemyAI` state transitions between patrol, chase, wait, and investigate
- locked interaction flows for `Door`, `Chest`, `Drain`, and `ElevatorPanel`
- drag-and-drop item movement in `ItemDragHandler`

## Conclusion

The project now contains a maintainable Unity test structure with both unit and integration tests. The chosen tests focus on deterministic gameplay logic and realistic component interactions, which gives better value than trying to test only isolated methods or only visual assets.
