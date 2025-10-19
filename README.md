# SGD240_Task3
Task 2 and 3 for SGD240 University of the Sunshine Coast 

A Unity-based project combining procedural terrain generation, real-time erosion, and AI agent interaction. This project demonstrates how dynamically generated environments can evolve through player and AI movement, creating immersive and naturally changing worlds.

To play a demo of this feature, please visit my Itch.io project, Hiking Erosion Game. https://m-m337.itch.io/hiking-erosion-game 

<br>

## Overview
This project explores both procedural map generation using Perlin noise and erosion simulation caused by the player and AI agents. Through multiple development stages, scenes, and branches, the project evolved from basic terrain creation to a fully integrated system with NavMesh components and a realistic, dynamic terrain.

<br>

## Branches
Each branch reflects a different sprint or milestone during this project's development.
| Branch | Description |
| --- | --- |
| main | A basic foundation of the project, used as a stable fallback for testing. |
| Advanced_Perlin_Noise_Generation | Builds upon the main branch, expanding terrain generation methods (as seen in the ProceduralGenerationScene). |
| First-Person_Player | Introduces a controllable player with working first-person movement to explore the terrain. |
| Player_Deformation | Implements erosion when the player moves and tests AI agent erosion in a static scene (as seen in the ErosionAgentsScene). |
| Map_Erosion | The final branch combining all the features, with testing and final touches completed in the FinalPrototypeScene. |

<br>

## Scenes
**1. ProceduralGenerationScene**

Displays a procedurally generated map that is fully customisable and shows multiple Perlin noise characteristics. This was achieved by following Sebastian Lague's YouTube tutorials.

<br>

**2. ErosionAgentsScene**

Demonstrates how both the player and AI agents can erode a static map in real time.

<br>

**3. FinalPrototypeScene**

This scene is the final prototype version, which merges procedural map generation with erosion mechanics, allowing for a fully customisable map with natural erosion at the start and during gameplay.

<br>

## Research and Documentation
All research, testing logs, and development reflections are available in the wiki, which outlines my thought process, testing steps, and sprints for this project. 
### I would like to acknowledge the following resources, which made this project possible:
Sebastian Lague's YouTube tutorials: https://www.youtube.com/watch?v=wbpMiKiSKm8&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3 

The NavMesh components that I used: https://github.com/Unity-Technologies/NavMeshComponents/tree/2020.2/Assets 

<br>

## Technical Details
**Unity Version:** 6.2 (6000.2.0f1)

Universal Render Pipeline

**Size:** 2.25GB

**Language:** C#

<br>

## Reflection
With further development, this system could lead to realistic, self-adapting worlds that evolve with gameplay. This framework could be expanded for future games or even integrated into Unity as a built-in feature, thus helping developers, especially beginners, in creating a dynamic environment with minimal setup that is ever-changing. These features can significantly enhance immersion, gameplay depth, and environmental storytelling. 

There are still quite a few bugs and small issues, however I am limited with time and skill, perhaps in the future I can come back and improve on it or someone else can take this project and further build upon it.

<br>

## Screenshots
<img width="1919" height="837" alt="1" src="https://github.com/user-attachments/assets/59355d4f-2dd8-4ef3-9802-ee941d7627b4" />

<img width="1498" height="658" alt="2" src="https://github.com/user-attachments/assets/262cbdeb-e414-4692-b385-5adff8104057" />

<img width="1502" height="659" alt="3" src="https://github.com/user-attachments/assets/9cd579a2-8ca2-45a8-9d8f-1ba4767756fb" />

<img width="1231" height="604" alt="4" src="https://github.com/user-attachments/assets/4f9ec608-0128-48b0-ae5c-0f750b3f2854" />

<img width="1234" height="608" alt="5" src="https://github.com/user-attachments/assets/d62468b8-6528-4345-ba44-0190c1728065" />


