# Copilot Instructions

## Project Guidelines
- User wants all WPF pages stored under the existing folder \src\page.
- Use CommunityToolkit.Mvvm only where needed for WPF binding, keeping code usage as close as possible to normal variables and very simple.
- Favor a simple shared-access pattern when appropriate, avoiding repeatedly passing shared objects through every function.
- Prefer compact, readable class definitions with minimal spacing.
- Declare all classes nested in Set.cs as partial for future extensions while keeping definitions compact and readable.
- Keep code very short and simple; make members public and avoid extra access-modifier or abstraction complexity when implementing features.
- Keep new code short and simple; avoid private members and unnecessary getters/setters unless strictly needed.
- Prefer exposing Data.Archive directly as a public readonly contiguous array when that keeps the acquisition/archive code simpler; avoid a separate private backing field and public wrapper unless encapsulation is needed.
- Strictly separate instrument/domain classes (e.g., Display, future Step) from WPF GUI classes (e.g., DisplayPage); instrument classes must not reference WPF or ScottPlot UI controls.
- For configuration selectors backed by numeric indices, use zero-based integer properties with UI labels translated to one-based names; this also applies to Set.Step.Loop.
- Keep code short and simple, following the existing project style with minimal spacing and abstraction.
- Prefer shorter, simpler code and minimal ceremony when implementing features.
- The data model must support plotting and analysis from both Archive slices and the temporary slice. Archive owns complete slices divided into pages; Temp is one temporary slice overwritten after consumption. Prefer a simple architecture with copying only at clear storage boundaries and attention to performance.
- Display.LogPlot() must directly invoke Updated with the shared current slice, avoiding local variables and null-check boilerplate when Run.data is guaranteed initialized before plotting.
- When modifying files, keep code compact: one instruction per line where practical, minimal blank lines, and avoid unnecessary vertical spacing or ceremony. Follow the existing concise project style.
- Use compact C# formatting: keep each instruction on one line, no blank lines except between class/function definitions, put opening braces on the declaration line and align closing braces with the body indentation, following Store.cs.
- Prefer simple, uniform settings naming in SetData: use the `Data` suffix for nested GUI/settings types and singular property names for collections, such as `Loop`, `Step`, `Trim`, and `Roi`, while keeping separate non-Data runtime classes for machinery and analysis.

## GUI Control Requirements
- Create a reusable table-like group control for the GUI with the following specifications:
  - Fixed constant column count per instance
  - Variable row/column counts between instances
  - Left/top labels only
  - No spacing between input cells
  - Mixed repeated row control types across columns
  - Bound to simple Set classes like Set.Loop[index].Home
  - Maintain a very simple overall structure with minimal insertion effort for new rows/controls.
- For WPF settings pages, place Boolean setting labels on the left and bind a separate CheckBox control on the right, following ParmPage's ParmRow pattern.

## Visualization Behavior
- Implement specialized visualization behavior, such as CommunityToolkit On...Changed callbacks, grouped at the end of Set.cs.
- For plot curve colors, use named ScottPlot colors (such as Yellow, Red, Blue) rather than hexadecimal colors, with the first curve using Yellow.