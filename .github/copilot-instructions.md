## Quick context

This repository is a collection of NinjaTrader NinjaScript source files (Indicators, Strategies, AddOns, WPF XAML UI). There are no Visual Studio solution (.sln) or project (.csproj) files in the repo — the files are authored as NinjaTrader components and are intended to be installed/edited via the NinjaTrader NinjaScript Editor or copied into NinjaTrader's Custom folders.

Key folders
- `AddOns/` — application-level add-ons and windows (examples: `TradeManagerAddOn.cs`, `TradeManagerUI.xaml`).
- `Indicators/` — chart indicators (examples: `WBMWoodiePivots.cs`, `SampleWPFModifications.cs`).
- `Strategies/` — automated strategies (examples: `PivotStrategy.cs`, `MyCustomStrategy.cs`).
- `DrawingTools/` — custom drawing tools.

## What an AI agent should know to be productive

- Namespace conventions are important and enforced by the scripts. Keep the folder-to-namespace mapping: `NinjaTrader.NinjaScript.Indicators`, `NinjaTrader.NinjaScript.Strategies`, `NinjaTrader.Gui.NinjaScript` etc. Example: `WBMWoodiePivots.cs` begins with `namespace NinjaTrader.NinjaScript.Indicators` and includes a header comment: "This namespace holds Indicators in this folder and is required. Do not change it." — do not rename or move namespaces.

- File/class naming: classes match filenames (e.g., `WoodiePivots` in `WBMWoodiePivots.cs`, `MyCustomStrategy` in `MyCustomStrategy.cs`). Preserve filenames when editing; changing them will break how NinjaTrader finds components.

- Lifecycle hooks and patterns to follow (use these exact overrides):
  - `protected override void OnStateChange()` — used to set defaults, configure DataSeries and initialize references (see `Indicators/WBMWoodiePivots.cs`, `Strategies/MyCustomStrategy.cs`).
  - `protected override void OnBarUpdate()` — main per-bar/per-tick logic (most indicators/strategies use guards like `if (BarsInProgress != 0) return;`).
  - `protected override void OnRender(ChartControl chartControl, ChartScale chartScale)` — used by components that draw directly on charts (see `WBMWoodiePivots.cs`).

- UI / threading: any WPF / ChartControl modifications must be performed on the chart/UI thread. The codebase uses `ChartControl.Dispatcher.InvokeAsync(...)` or `Core.Globals.RandomDispatcher.BeginInvoke(...)` for this. See `Indicators/SampleWPFModifications.cs` and `AddOns/TradeManagerAddOn.cs` for examples — mirror these patterns when adding or disposing UI elements.

- Configuration flags and metadata: some classes contain embedded wizard-settings / metadata blocks (XML regions at the bottom of strategy files). Avoid removing or changing these regions (they are used by the NinjaTrader editor/wizard). Example: the `/*@ ... */` "Wizard settings" block in `Strategies/MyCustomStrategy.cs`.

- Rendering vs calculation modes: pay attention to `Calculate` settings in `OnStateChange` (`Calculate.OnBarClose` vs `Calculate.OnEachTick`) and to `BarsInProgress`/`BarsRequiredToTrade` guards in strategies. Changing Calculate or bar checks changes runtime behaviour and performance.

- Example: enabling render events
  - `MyCustomStrategy.cs` contains `<UseOnRenderEvent>true</UseOnRenderEvent>` in its wizard metadata; when render events are required, follow that pattern and ensure `OnRender` is implemented safely.

## Build / test / debug workflow (repo-specific notes)

- There is no .sln/.csproj here; this repo stores NinjaScript source files. Typical local workflow (what to expect):
  - Edit files in this repo or in the NinjaTrader NinjaScript Editor. The NinjaTrader platform compiles the scripts at runtime.
  - To test changes, copy the `.cs`/`.xaml` into your local NinjaTrader `bin\\Custom` folder (or use the platform's Import/NinjaScript Editor) and reload/compile within NinjaTrader.

- For debugging, prefer the NinjaTrader logging and chart/trade simulation. Files include `Log(...)` and `Draw.TextFixed(...)` calls — use those to surface runtime information when running inside NinjaTrader.

## Conventions & gotchas (concrete)

- Do not change the folder-level namespace (see file headers). Many files explicitly note this.
- Follow the UI-thread patterns for any ChartControl or WPF interactions: use Dispatcher or RandomDispatcher as shown in `SampleWPFModifications.cs` and `TradeManagerAddOn.cs`.
- Preserve wizard/metadata regions in Strategy files. They are machine-readable and used by the editor.
- Match Calculate & BarsRequired settings; tests and behavior rely on them.

## Files to inspect for examples (quick references)
- `Indicators/WBMWoodiePivots.cs` — full-featured indicator; shows `OnStateChange`, `OnBarUpdate`, `OnRender` and how to add plots.
- `Indicators/SampleWPFModifications.cs` — canonical examples of ChartControl Dispatcher usage and adding WPF controls to chart areas.
- `AddOns/TradeManagerAddOn.cs` & `AddOns/TradeManagerUI.xaml` — how to create windows, tabs and menu items; `OnWindowCreated`/`OnWindowDestroyed` patterns.
- `Strategies/MyCustomStrategy.cs` & `Strategies/PivotStrategy.cs` — strategy wiring, wizard metadata and guard patterns.

## How the agent should behave when editing code

- Make minimal, local-scope changes; preserve namespaces, file names and wizard metadata blocks.
- When adding UI code, copy the Dispatcher usage pattern rather than inventing a new threading approach.
- When adding indicators/strategies, ensure the `Calculate` mode and `BarsRequiredToTrade` are explicitly set in `OnStateChange` defaults.
- When unsure, prefer leaving comments and small, well-scoped refactors rather than large structural changes — these scripts are tightly coupled to the NinjaTrader runtime.

---
Please review these instructions and tell me if you'd like me to add more examples (I can inline short snippets from the listed files), mention deployment paths for your local NinjaTrader install, or adopt a different tone/length. I'll iterate based on your feedback.
