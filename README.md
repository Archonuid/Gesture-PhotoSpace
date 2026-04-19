# Gesture Photo Space

A gesture-controlled 3D photo exploration system built in Unity.
The application allows users to navigate, morph, and interact with a spatial photo environment using real-time hand tracking.

---

## Overview

This project implements a dual-hand interaction model:

* **Left hand** controls spatial transformations (geometry morphing)
* **Right hand** controls navigation and interaction (browse, select, zoom)

The system transitions between multiple interaction states:

* Idle (particle space)
* Morph (geometry control)
* Ribbon (photo browsing interface)

All interactions are driven by hand tracking data processed externally and streamed into Unity.

---

## Features

### Spatial Photo System

* Photos loaded dynamically from a local folder
* Rendered as billboards in 3D space
* Idle state uses particle-based drifting motion

### Geometry Morphing

* Continuous morph between multiple shapes:

  * Sphere
  * Cube
  * Cylinder
  * Cone
  * Torus
  * Capsule
  * Tetrahedron
  * Icosahedron
* Morph driven by left-hand rotation gesture

### Fabric Shell

* Procedural grid membrane enclosing the photo system
* Built using latitude/longitude line rendering
* Deforms and morphs in sync with layout geometry

### Ribbon Navigation (Right Hand)

* Cylindrical ribbon layout anchored to camera
* Multi-row image browsing interface
* Smooth scroll via gesture input
* Grid-based selection system

### Image Interaction

* Selection highlight with scale + emission
* Focus mode using thumb gesture
* Pinch-based zoom with aspect-ratio preservation
* Navigation lock during focus mode

### State Management

* Idle → Morph → Ribbon transitions
* Gesture stability thresholding
* Frame-based detection filtering

---

## Architecture

### Core Systems

* `InteractionManager`

  * State machine controlling system modes

* `LayoutController`

  * Handles geometric layout generation and morphing

* `RibbonLayout`

  * Manages cylindrical browsing interface

* `PinchZoom`

  * Controls focused image view and scaling

* `AtomMotion`

  * Particle motion system for idle state

* `FabricShellController`

  * Procedural membrane generation and deformation

* `UDPReceiver`

  * Receives gesture data from external tracker

---

### Data Flow

Webcam → Hand Tracking → Gesture Processing → UDP → Unity → Interaction System

---

## Controls

### Left Hand

* Rotation controls geometry morphing

### Right Hand

* Presence activates ribbon mode
* Index movement navigates selection
* Thumb gesture enters focus mode
* Pinch gesture controls zoom

---

## Setup

### Requirements

* Unity (URP project)
* Python (for gesture tracking)
* Webcam

### Steps

1. Clone the repository
2. Open the project in Unity
3. Ensure URP is configured
4. Run the Python gesture tracking script
5. Press Play in Unity

---

## Project Structure

```
Assets/
 ├── Scripts/
 │    ├── Core/
 │    ├── Systems/
 │    └── Layouts/
 ├── Prefabs/
 ├── Materials/
 ├── Scenes/
 └── Resources/

Packages/
ProjectSettings/
```

---

## Notes

* Image dataset is not included in the repository
* Gesture tracking runs as a separate Python process
* Communication is handled via UDP

---

## Future Work

* Gesture classification refinement
* Physics-based motion system
* Depth-of-field and post-processing
* Image clustering and semantic grouping
* VR / AR integration
