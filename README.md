# Flight Simulator

A Unity-based flight simulator focused on semi-realistic aircraft movement and flight physics.  
The project simulates core flight controls and aircraft state in real time, including throttle, pitch, roll, yaw, speed, and altitude, inside a 3D mountainous environment.

## Example

![Flight simulator gameplay](game_example.png)

### Unity Editor

![Flight simulator in the Unity Editor](unity_editor_example.png)

> Place `game_example.png` and `unity_editor_example.png` in the root of the repository for the images above to display correctly on GitHub.

## Features

- Real-time aircraft flight simulation
- Throttle control
- Pitch, roll, and yaw controls
- Aircraft speed and altitude tracking
- Physics-based movement using Unity
- Third-person chase camera
- Real-time input and state display
- 3D terrain and environment
- Aircraft model with animated flight behavior

## Project Goal

The goal of this project is to explore how real-world flight behavior can be approximated inside a game engine.

Rather than moving the aircraft with simple scripted translations, the simulator is designed around physics-based movement and aircraft controls. Player input affects the state of the aircraft, while the simulation continuously tracks values such as speed and altitude.

The project is intended as an educational and experimental flight simulator rather than a full commercial-grade aviation simulator.

## Controls

The simulator supports the main aircraft control axes:

| Control | Description |
|---|---|
| Throttle | Controls engine thrust |
| Pitch | Rotates the aircraft nose up or down |
| Roll | Banks the aircraft left or right |
| Yaw | Rotates the aircraft around its vertical axis |

The exact keyboard/controller bindings can be viewed or changed through Unity's input configuration used by the project.

## Requirements

To open and run the project you need:

- Unity Hub
- The Unity Editor version used by the project
- Git LFS for repositories containing large terrain or texture assets

The exact Unity version is stored in:

```text
ProjectSettings/ProjectVersion.txt
```

Unity Hub will normally detect the required editor version when the project is opened.

## Cloning the Repository

Because the project contains large Unity assets, install Git LFS before cloning:

```bash
git lfs install
```

Clone the repository:

```bash
git clone https://github.com/jusosojnik/Flight-Simulator.git
cd Flight-Simulator
```

Download any LFS-managed assets:

```bash
git lfs pull
```

## Opening the Project

1. Open **Unity Hub**.
2. Click **Add** or **Open**.
3. Select the cloned `Flight-Simulator` folder.
4. Open the project with the Unity version listed in `ProjectSettings/ProjectVersion.txt`.
5. Wait for Unity to import the project assets.
6. Open the main flight scene from the `Assets` folder.
7. Press **Play** in the Unity Editor.

The first launch may take longer because Unity needs to rebuild the local `Library` folder.

## Unity Project Structure

The important repository folders are:

```text
Flight-Simulator/
├── Assets/
├── Packages/
├── ProjectSettings/
├── .gitignore
├── .gitattributes
├── game_example.png
└── unity_editor_example.png
```

### `Assets/`

Contains the project source content, including scenes, scripts, models, materials, textures, terrain data, UI elements, and other Unity assets.

### `Packages/`

Contains the Unity package configuration required by the project.

### `ProjectSettings/`

Contains project-wide Unity settings, including the editor version and physics/input configuration.

## Files Not Stored in Git

Unity generates several large local folders that should not be committed:

```text
Library/
Temp/
Logs/
Obj/
Build/
Builds/
UserSettings/
```

These folders are regenerated automatically by Unity and are excluded through `.gitignore`.

## Git LFS

Some terrain and image assets used by the project are large enough that they should be stored using **Git Large File Storage (Git LFS)** rather than normal Git objects.

If large assets are missing after cloning, run:

```bash
git lfs install
git lfs pull
```

To see which files are currently managed by LFS:

```bash
git lfs ls-files
```

## Physics Simulation

The simulator attempts to reproduce the main behavior expected from an aircraft rather than using purely arcade-style movement.

The simulation takes player input for:

- throttle
- pitch
- roll
- yaw

and updates the aircraft state using Unity's physics system.

The on-screen debug display provides immediate feedback for both the current control input and aircraft state, including speed and altitude.

## Screenshots

### Gameplay

![Gameplay example](game_example.png)

### Editor View

![Unity editor example](unity_editor_example.png)

## Possible Future Improvements

Possible extensions to the simulator include:

- More detailed aerodynamic modeling
- Lift and drag curves based on angle of attack
- Stall behavior
- Wind and turbulence
- Landing gear and ground handling
- Cockpit camera
- Multiple aircraft
- Improved instrumentation
- Controller / joystick support
- Weather effects
- Missions and checkpoints
- Larger environments
- Improved terrain optimization

## Notes

This project was created as an experimental Unity flight simulator and as an exploration of aircraft physics, real-time simulation, and 3D game development.

It is not intended to reproduce the accuracy of a professional certified flight simulator, but rather to provide a more physically motivated flight model than simple arcade-style aircraft movement.
