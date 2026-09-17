# Copilot Instructions

## Project Guidelines
- User wants all WPF pages stored under the existing folder \src\page.
- Use CommunityToolkit.Mvvm only where needed for WPF binding, keeping code usage as close as possible to normal variables and very simple.

## GUI Control Requirements
- Create a reusable table-like group control for the GUI with the following specifications:
  - Fixed constant column count per instance
  - Variable row/column counts between instances
  - Left/top labels only
  - No spacing between input cells
  - Mixed repeated row control types across columns
  - Bound to simple Set classes like Set.Loop[index].Home
  - Maintain a very simple overall structure with minimal insertion effort for new rows/controls.