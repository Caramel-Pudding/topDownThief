# TopDownThief - Unity Prototype

This document provides essential context for the Gemini AI assistant regarding the "TopDownThief" Unity project.

## Project Overview

- **Project Name**: TopDownThief
- **Genre**: 2D Top-Down Stealth
- **Core Mechanics**: The game is a prototype focused on stealth. The player navigates levels while avoiding AI detection, interacting with objects (like chests), and utilizing mechanics such as lockpicking.

## Unity Version

- **Unity Editor Version**: 6000.1.11f1

## Getting Started & Running the Game

1.  Open the project's root folder (`C:\Users\Erik Kuznetsov\code\unityUltrahardcore\TopDownThief`) in the Unity Hub.
2.  The project will open in the Unity Editor.
3.  Load the main scene by navigating to `Assets/Scenes/TutorialScene.unity` in the Project window and double-clicking it.
4.  Press the **Play** button at the top of the editor to run the game.

## Key Project Structure

-   `Assets/Scripts/`: This directory contains all C# game logic, organized by feature.
    -   `Player/`: Scripts for player movement, controls, and state.
    -   `AI/`: Scripts governing enemy/NPC behavior and detection.
    -   `Interactions/`: Logic for how the player interacts with world objects.
    -   `Systems/`: Core game-wide systems.
    -   `UI/`: Scripts for managing the user interface.
-   `Assets/Prefabs/`: Contains reusable GameObjects.
    -   `Player.prefab`: The primary player object.
    -   `AI/`, `Objects/`, `Mechanics/`: Other important prefabs.
-   `Assets/Scenes/`: Contains all game levels.
    -   `TutorialScene.unity`: The main scene for development and testing.
-   `Assets/InputSystem_Actions.inputactions`: This file defines all player controls using Unity's new Input System.
-   `ProjectSettings/`: Stores all project-wide configuration.
    -   `ProjectVersion.txt`: Defines the Unity editor version.

## Architectural Principles & Conventions

- **Component-Based Design**: The project heavily utilizes a component-based architecture. Core functionalities are encapsulated in `MonoBehaviour` scripts attached to GameObjects.
- **Separation of Concerns**: Functionality is often split across multiple, focused components. For example, the `Door` is composed of `DoorController`, `DoorLockingSystem`, `DoorAudio`, and `DoorInteractionHandler`. When adding new features, prefer creating a new component rather than adding unrelated logic to an existing one.
- **Interaction System**:
    -   Player interactions with objects are managed through the `IInteractable` interface.
    -   Objects that can be interacted with should have a component that implements this interface.
    -   UI prompts are typically handled by a separate `PromptController` component.
- **Code Duplication**: Strive to keep the code DRY (Don't Repeat Yourself). If you find similar logic in multiple places (e.g., starting a lockpicking minigame for both chests and doors), refactor it into a reusable component. The `LockpickingInitiator` is a good example of this pattern.
- **Dependencies**: Use `[SerializeField]` for references to other components on the same or different GameObjects. Use `GetComponent` in `Awake()` or `OnValidate()` as a fallback to ensure required components are present. For newly created components, consider adding a `RequireComponent` attribute if it depends on another component on the same GameObject.
- **Input Handling**: Player input is managed via the `InputSystem_Actions.inputactions` file and the generated C# class. Input events are subscribed to in `OnEnable` and unsubscribed from in `OnDisable` or `OnDestroy`.

## Build & Test Instructions

-   **Primary Testing**: All testing is currently done by running the `TutorialScene` within the Unity Editor.
-   **Builds**: Builds can be created via the `File > Build Settings...` menu in the Unity Editor. For our purposes, running the game in the editor is the standard procedure.
