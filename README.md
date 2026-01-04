🌱 The Growth Tree
The Growth Tree is a Unity 3D environmental simulation game where players restore nature by planting and maintaining trees while managing pollution, resources, and quests. This project was developed as an academic game project focusing on modular system design, persistent game state, and interactive gameplay mechanics.

🎮 Gameplay Overview
Players can:
- Plant different types of trees
- Water and maintain tree health
- Harvest fruits and manage inventory
- Complete NPC-driven quests
- Reduce environmental pollution through sustainable actions
- The environment reacts dynamically to player actions, including pollution-based fog effects and real-time tree health visualization.

🧩 Core Systems

🌳 Tree & Environment System
- Tree growth stages and health management
- Water consumption and refill mechanics
- Pollution system affecting environment visuals
- Fog system dynamically driven by pollution level

📜 Quest System
- ScriptableObject-based quest data
- Quest queues with difficulty tiers
- NPC quest interaction and progression tracking
- Quest UI with timers, progress, and completion states

👥 NPC System
- Dynamic NPC spawning
- Quest interaction logic
- Waypoint-based patrol and movement
- NPC reuse system for chained quests

🎒 Inventory & Economy
- Inventory system for seeds and fruits
- Buy and sell shop interactions
- Resource validation and UI synchronization

💾 Save & Load System
- JSON-based save data
- Persistent player progress
- World state restoration (trees, quests, resources)
- Automatic save/load handling during scene transitions

🎓 Tutorial System
- Event-driven tutorial flow
- Step-based progression
- Contextual guidance using arrows and narrative UI
- Persistent tutorial state across sessions

🖥️ UI System
- Modular UI architecture
- Prompt system using unified IInteractable interface
- Quest UI, inventory UI, and loading screens
- Game result panels and pause menu

🔄 Scene Management
- Asynchronous scene loading using coroutines
- Progress-based loading stages
- Safe system initialization checks
- Seamless transition between Main Menu and Gameplay scenes

🛠️ Technical Details
- Engine: Unity 3D (URP)
- Programming Language: C#
- Architecture: Modular, event-driven
- Data Storage: JSON serialization
- Input System: Unified interaction interface
- Rendering: Universal Render Pipeline (URP)
