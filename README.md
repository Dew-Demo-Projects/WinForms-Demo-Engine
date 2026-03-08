# DemoSystem

A C# WinForms demo/effects player built on .NET 8. The application sequences through a series of animated visual "
Moments," each containing one or more graphical effects with option for real-time mouse input.

---

> [!NOTE]
> Created at October 2025

## Features

- **Sequenced demo playback** — A `Demo` chains multiple `Moment`s together; when one finishes, the next begins (looping
  back to the start after the last).
- **Interactive effects** — Several effects respond to mouse position, left/right click, and scroll wheel in real time.
- **Factory pattern** — The demo composition is decoupled from the runner via `IDemoFactory`/`MyDemoFactory`, making it
  easy to swap or extend demos.
- **Double-buffered rendering** — The WinForms window is double-buffered and driven by a 50 ms timer (~20 FPS).

---

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows (WinForms dependency)

---

## Getting Started

### 1. Clone / Download

```bash
git clone https://github.com/Dew-Demo-Projects/arduino-uno-r4-game-engine.git
```

### 2. Run

```bash
cd DemoSystem
dotnet run
```

Or open `DemoSystem.csproj` and start within IDE.

---

## Demo Sequence

The default sequence (defined in `Factories/MyDemoFactory.cs`) plays through the following Moments in order:

| # | Frames | Effect                                                            | Interactive |
|---|--------|-------------------------------------------------------------------|-------------|
| 1 | 60     | `ElectricArcs(2)` — midpoint-displacement lightning bolts         | No          |
| 2 | 200    | `RotatingGravityBox(500)` — spinning box with physics balls       | No          |
| 3 | 300    | `MagneticParticles(200)` — particles attracted/repelled by cursor | Yes         |
| 4 | 400    | `NeuralNetworkFireworks` — interactive growing neural network     | Yes         |
| 5 | 300    | `InteractiveFluid(200)` — SPH fluid simulation                    | Yes         |

After Moment 5, playback loops back to Moment 1.

---

## Interactive Controls

| Effect                 | Left Click         | Right Click              | Scroll Wheel         |
|------------------------|--------------------|--------------------------|----------------------|
| MagneticParticles      | Attract particles  | Repel particles          | Adjust strength      |
| NeuralNetworkFireworks | Add / drag neurons | Stimulate nearby neurons | Adjust learning rate |
| InteractiveFluid       | Push fluid         | Pull fluid               | —                    |

---

## Project Structure

```
DemoSystem/
├── DemoSystem.cs               # Main WinForms form; input plumbing + render loop
├── Program.cs                  # Entry point
├── Factories/
│   ├── IDemoFactory.cs         # Factory interface
│   └── MyDemoFactory.cs        # Concrete demo composition
└── Library/
    ├── Demo.cs                 # Sequences Moments; advances on frame exhaustion
    ├── Moment.cs               # Time-bounded container for effects; local frame counter
    ├── InputState.cs           # Immutable struct: mouse pos, buttons, wheel delta
    └── Effects/
        ├── IEffect.cs                  # Base interface (no input)
        ├── IInteractiveEffect.cs       # Base interface (with InputState)
        ├── ElectricArcs.cs             # Midpoint-displacement lightning
        ├── GrowingCircle.cs            # Simple expanding circle
        ├── PulsingRings.cs             # Concentric animated rings
        ├── RotatingGravityBox.cs       # Rotating box + ball physics (non-interactive)
        ├── InteractiveGravityBox.cs    # Rotating box + mouse-influenced gravity
        ├── MagneticParticles.cs        # Particle system with magnetic forces
        ├── NeuralNetworkFireworks.cs   # Dynamic neural network visualization
        └── InteractiveFluid.cs         # SPH particle fluid simulation
```

---

## Adding a New Effect

1. Create a class implementing `IEffect` or `IInteractiveEffect` in `Library/Effects/`.
2. Instantiate it in `MyDemoFactory.CreateDemo()`.
3. Wrap it in a `Moment` and call `AddEffect()` or `AddInteractiveEffect()`.

```csharp
var myMoment = new Moment(frameCount: 120, timeMultiplier: 1f);
myMoment.AddEffect(new MyNewEffect());
demo.AddMoment(myMoment);
```
