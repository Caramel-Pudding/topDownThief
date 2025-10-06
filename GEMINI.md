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

## Build & Test Instructions

-   **Primary Testing**: All testing is currently done by running the `TutorialScene` within the Unity Editor.
-   **Builds**: Builds can be created via the `File > Build Settings...` menu in the Unity Editor. For our purposes, running the game in the editor is the standard procedure.
