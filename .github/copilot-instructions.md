# Copilot Instructions

## Project Guidelines
- User wants all WPF pages stored under the existing folder \src\page.
- Use CommunityToolkit.Mvvm only where needed for WPF binding, keeping code usage as close as possible to normal variables and very simple.
- Favor a simple shared-access pattern when appropriate, avoiding repeatedly passing shared objects through every function.
- Prefer compact, readable class definitions with minimal spacing.
- Declare all classes nested in Set.cs as partial for future extensions while keeping definitions compact and readable.

## GUI Control Requirements
- Create a reusable table-like group control for the GUI with the following specifications:
  - Fixed constant column count per instance
  - Variable row/column counts between instances
  - Left/top labels only
  - No spacing between input cells
  - Mixed repeated row control types across columns
  - Bound to simple Set classes like Set.Loop[index].Home
  - Maintain a very simple overall structure with minimal insertion effort for new rows/controls.

## Visualization Behavior
- Implement specialized visualization behavior, such as CommunityToolkit On...Changed callbacks, grouped at the end of Set.cs.