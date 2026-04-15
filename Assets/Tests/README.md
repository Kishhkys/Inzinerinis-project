# Automated Test Structure

This project uses Unity Test Framework with separate folders for:

- `Assets/Tests/EditMode`: fast unit tests for gameplay logic and helper components
- `Assets/Tests/PlayMode`: integration-style tests for MonoBehaviours, UI state, and component interaction

Both test assemblies reference the main gameplay assembly through `Assembly-CSharp`.
