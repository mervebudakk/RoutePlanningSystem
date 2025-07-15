# 🧭 Route Planning System
A Windows Presentation Foundation (WPF)-based public transportation route planner for Kocaeli, Turkey.  
The application uses **Leaflet** maps and visualizes optimal paths using selected transport types (e.g., bus, tram, taxi).

## 🔧 Features
- 📍 Calculate and display the shortest or cheapest route based on user preferences.
- 🚌 Multi-transfer route visualization on map.
- 📊 Displays estimated travel time, cost, and distance.
- 🧠 Uses Dijkstra's algorithm for optimal pathfinding.
- 🌐 Leaflet map integration (with optional OpenRouteService API support).
- 🔄 Alternative transport options: only buses, only trams, taxi, or mixed.

## 🛠️ Technologies
- **C#** (WPF - .NET Framework 4.7.2)
- **Leaflet.js** (Map rendering)
- **HTML/JavaScript** (embedded in WPF WebBrowser control)
- **JSON** (static transport data)

## 🚀 Setup
1. Clone this project:
git clone https://github.com/mervebudakk/RoutePlanningSystem.git
2. Open it in Visual Studio and run it.
3. Make sure the following files and folders are placed correctly:
veri.json
Map.html
Icons/ folder (with required images such as location.png, flag.png)
