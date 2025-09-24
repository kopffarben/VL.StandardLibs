# VL.ImNodes

ImNodes integration library for vvvv - provides node editor functionality using Dear ImGui.

## Overview

VL.ImNodes brings interactive node graph editing capabilities to VL applications by integrating the ImNodes.NET library. It allows you to create node editors with draggable nodes, connectable pins, and interactive link management within your ImGui-based interfaces.

## Features (MVP)

- Host an ImNodes editor inside an existing ImGui frame
- Create nodes with input/output attributes (pins)
- Create and destroy links interactively
- Query selection (nodes + links)
- Get/set node positions
- Basic style/theming
- Serialize/deserialize simple graphs (nodes, pins, links)

## Installation

Add `VL.ImNodes` as a dependency to your VL document or application.

## Quick Example

```csharp
// Coming soon - basic node editor usage example
```

## Architecture

VL.ImNodes follows the same architectural patterns as VL.ImGui, providing:
- Context management for multiple editors
- Style system integration
- Event-driven interaction model
- Resource management and lifecycle handling

## Compatibility

- ImGui.NET: 1.90.0.1
- ImNodes.NET: 0.3.1
- .NET: 8.0

## Future Features

- Copy/Paste between editors
- Undo/Redo stack
- Dynamic runtime creation of VL nodes
- Advanced layouts and auto-arrangement
- Complex context menu templating