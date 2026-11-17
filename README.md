# 🏹 BeatAppleVR
🎬 **[Watch the Full YouTube Demo Playlist](https://www.youtube.com/playlist?list=PLHxgRazWP1oI)**

**Balancing Accuracy and Sense of Agency: A Comparative Evaluation of Four Aim-Assist Techniques in VR Archery**

**BeatAppleVR** is a Virtual Reality archery research testbed developed in Unity for the **Meta Quest 3**. This repository contains the interactive VR environment designed to evaluate invisible aim-assist interventions across four sequential stages of an archery shot, examining the trade-off between shooting performance and the player's Sense of Agency (SoA).

## 🎯 Research Overview

Aiming a bow in VR is inherently challenging for casual players due to the lack of physical stabilization and stereoscopic depth perception limits (which prevent the use of traditional 2D screen-space crosshairs). While aim-assist can reduce frustration, overly aggressive assistance increases **perceptibility** and undermines the player's **Sense of Agency**—the feeling that they are truly in control of their shots.

This project investigates **four stage-specific aim-assist techniques** to identify which mechanism best improves accuracy while remaining subtle and preserving player agency:

1. **Pull & Aim Stage — *Input Smoothing*:** Filters controller hand tremor during the draw phase to stabilize aiming without inducing severe sensorimotor conflict.
2. **Release Stage — *Initial Ray Redirection*:** Subtly redirects the initial launch velocity vector toward the target.
3. **Flight Stage — *In-Flight Bullet Magnetism*:** Applies mid-air trajectory correction.
4. **Impact Stage — *Hitbox Expansion*:** Dynamically enlarges the target's effective collision boundary upon impact.


## ✨ Current Implementation Status

#### 🏃‍♂️ Player Locomotion & VR Interaction — [🎥 Watch Demo](https://www.youtube.com/watch?v=GRA8wolTh8o&list=PLHxgRazWP1oI)

* **Grab Interaction:** Object picking, holding, and manipulation via VR controllers (Grip button).
* **Locomotion System:** Smooth movement using the Left Thumbstick, Snap Turn rotation using the Right Thumbstick, and jumping via the Right Primary Button (`A` Button).

#### 🏹 Archery Mechanics & Spatial Audio — [🎥 Watch Demo](https://www.youtube.com/watch?v=MtU-tQ7Iins&list=PLHxgRazWP1oI)

* **Bow & Arrow Spawner:** Continuous spawning and equipping system for the bow (Grip button) and drawing the bowstring (Right Trigger).
* **Immersive Sound Effects (SFX):** Spatial audio feedback for key physical interactions, including **bowstring release** and **arrow impact** sounds.

#### 🎛️ Experimental UI & Controls — [🎥 Watch Demo](https://www.youtube.com/watch?v=yl7g6O1kF_E&list=PLHxgRazWP1oI)

* **Radial Menu System:** Controller-based radial menu for real-time (on-the-fly) system switching without pausing the session.
  * *Interaction Logic:* Hold **Left Trigger** to open the menu and use the **Left Thumbstick** to select an option (player movement is automatically disabled while the menu is open).
  * *Current State:* Functional UI prototype using mocked visual feedback (dynamically changes the arrow's color upon selection).
  * *Planned Integration:* Will be linked directly to switch between the **4 Aim-Assist Techniques (Settings A–E)** and **Assist Intensity Levels (Presets W–Z)**.

## 🗺️ Development Roadmap

* [x] Core VR locomotion (Move, Snap Turn, Jump) and Grab mechanics
* [x] Physics-based Bow & Arrow spawner with release and impact SFX
* [x] Radial Menu UI prototype (mocked with arrow color switching & movement lock while active)
* [ ] Implement the 4 Aim-Assist algorithms (*Input Smoothing, Initial Ray Redirection, Bullet Magnetism, Hitbox Expansion*)
* [ ] Hook up the Radial Menu to toggle experimental conditions (`Settings A–E` & `Presets W–Z`)
* [ ] Implement automated data logging for Objective Metrics (**Hit Rate** & **Radial Error**)
* [ ] Integrate in-VR 7-point Likert Scale questionnaires (**Sense of Agency**, **Perceptibility**, **Willingness to Use**)

## 📊 Evaluation Metrics

* **Objective Performance Metrics:**
  * **Hit Rate (%):** Percentage of arrows successfully hitting the target.
  * **Radial Error:** Euclidean distance from the arrow's impact point to the center of the target.
* **Subjective Experience Metrics** (7-Point Likert Scale & Forced Choice):
  * **Sense of Agency (SoA):** Perceived control over the arrow's trajectory and outcome.
  * **Perceptibility:** Noticeability of the system's subtle intervention.
  * **Willingness to Use:** Player satisfaction and preference for continued play.

## 🛠️ Tech Stack & Hardware Requirements

* **Engine:** Unity `6000.6.5f1` (Unity 6)
* **XR Framework:** Unity XR Interaction Toolkit (XRI) / OpenXR
* **Render Pipeline:** Universal Render Pipeline (URP)
* **Target Hardware:** Meta Quest 3

## 🎮 Controls (Meta Quest Touch Plus Controllers)

| Action | Controller Input |
| :--- | :--- |
| **Move** | Left Thumbstick *(Disabled while Radial Menu is open)* |
| **Rotate (Snap Turn)** | Right Thumbstick |
| **Jump** | Right Primary Button (`A` Button) |
| **Grab Bow** | Grip Button |
| **Draw Bow & Shoot** | Right Trigger |
| **Open Radial Menu** | Left Trigger |
| **Select Radial Menu Option** | Left Thumbstick *(While holding Left Trigger)* |

## 🚀 Getting Started

1. **Clone the repository:**
   ```bash
   git clone https://github.com/winithraKS/BeatAppleVR.git
   ```
2. **Open in Unity Hub:**
   * Click **Add > Add project from disk** and select the `BeatAppleVR` folder.
3. **Run in VR:**
   * Connect your **Meta Quest 3** via Quest Link / Air Link (or build the APK directly to the headset) and press **Play** in the Unity Editor.

## 📄 Publication, Media & Author

* **Video Demonstrations:** [BeatAppleVR YouTube Playlist](https://www.youtube.com/playlist?list=PLHxgRazWP1oI)
* **Author / Researcher:** Winithra Manolertthewan
