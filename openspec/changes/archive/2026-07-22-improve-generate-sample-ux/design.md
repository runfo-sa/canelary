## Context

`Cohere/Views/Dialogs/GenerateSample.xaml` + `Cohere/ViewModels/GenerateSampleViewModel.cs` implement the "Generar Muestra" dialog (a Prism `IDialogAware` dialog). Current state:

- Bottom `DockPanel` (`LastChildFill="False"`) holds, left-to-right in dock order: `EnableRecall` checkbox (Left), then Cancelar (Right, `IsCancel="True"`, no command), then Aceptar (Right, bound to `CloseDialogCommand`).
- The CMD textbox (`Text="{Binding AfterCommand}"`) is always visible in its own `DockPanel` row, with no visibility logic.
- `after_command.cache` is a plain-text file holding exactly one command, read in the constructor and overwritten in `SaveCachedAfterCommand()` after a successful run.
- `EnableRecall`, when checked, causes `ClosingDialog()` to call `GenerateRecall(...)`, which opens `GenerateRecallDialog` (a separate `.docx`-generating dialog registered in `CohereModule.cs`, using the `ReportTemplate` path from `Settings.yaml`).

## Goals / Non-Goals

**Goals:**
- Simplify the action row to a single "Imprimir" button, left-docked.
- Hide the CMD command input unless printer `"To PNG"` is selected.
- Replace the single-slot command cache with a 10-entry MRU JSON history surfaced via an editable combobox.
- Remove the RE-CAL-22 checkbox and the entire `GenerateRecallDialog` feature it triggers, including its module registration and settings entry.

**Non-Goals:**
- No migration of existing `after_command.cache` plain-text content into the new JSON format — the old file is simply ignored/replaced.
- No changes to the actual printing/label-generation pipeline (`PrintLabelsAsync`) beyond gating the CMD command by printer selection.
- No changes to other Cohere dialogs or modules outside what's needed to remove the RE-CAL-22 feature's registration points.

## Decisions

**CMD visibility via VM property, not XAML converter.** Expose `bool IsCmdVisible` on `GenerateSampleViewModel`, recomputed whenever `Printer` changes (raise `PropertyChanged` for `IsCmdVisible` inside the `Printer` setter). Bind the CMD `DockPanel`'s `Visibility` with a standard `BooleanToVisibilityConverter`. Rationale: keeps the "To PNG" comparison logic in the VM (testable, matches existing `TO_PNG` const usage) instead of a bespoke XAML value converter.

**Command history storage: JSON array, one file, capped at 10, MRU order.** Reuse the existing `after_command.cache` filename (still a JSON array now, not plain text) rather than introducing a new filename — avoids adding a new settings/const surface. On load: `JsonSerializer.Deserialize<List<string>>`; if the file is missing or fails to parse (e.g., still holds the old plain-text single command), treat history as empty rather than attempting migration. On save: remove any existing equal entry, insert the new/reused command at index 0, truncate to 10, write back with `JsonSerializer.Serialize`.

**Editable ComboBox over textbox + separate dropdown.** `ComboBox IsEditable="True" ItemsSource="{Binding AfterCommandHistory}" Text="{Binding AfterCommand}"` gives one control for both typing a new command and picking from history, matching standard WPF MRU patterns and requiring no new custom control.

**Full removal of RE-CAL-22, not a feature flag.** Since the proposal confirms the feature was never implemented and won't be pursued, delete `GenerateRecallDialog.xaml`/`.xaml.cs`, its `RegisterDialog<GenerateRecallDialog>()` call in `CohereModule.cs`, the `EnableRecall` property and `GenerateRecall(...)` method in `GenerateSampleViewModel`, the `ReportTemplate` entry in `Settings.yaml`, and the V-9 entry in `REQUIREMENTS.md`. No dead code or commented-out remnants left behind.

## Risks / Trade-offs

- **[Risk]** Existing `after_command.cache` files on user machines are plain text from the old format; the new JSON parser will fail to read them. → **Mitigation**: Treat parse failure as "no history" (empty list) rather than crashing; the file is silently replaced with valid JSON on next save. No data loss beyond the single old cached command, which is acceptable per proposal (no migration required).
- **[Risk]** Removing `IsCancel="True"` removes the Escape-to-close affordance. → **Mitigation**: Explicitly accepted in scoping — user confirmed this doesn't matter.
- **[Risk]** Deleting `GenerateRecallDialog` and its `Settings.yaml` entry could break if some other module references `ReportTemplate` or the dialog name string elsewhere. → **Mitigation**: Grep the full repo for `GenerateRecallDialog`, `ReportTemplate`, and `RE-CAL-22` before deleting, to confirm no other call sites exist (only `CohereModule.cs`, `Settings.yaml`, and `REQUIREMENTS.md` were found during exploration).

## Migration Plan

Not applicable — this is a local desktop app UI change with no deployment/rollback pipeline beyond a normal build and release. The `after_command.cache` format change is self-healing on first write, as described above.

## Open Questions

None outstanding — all ambiguities (Escape key, history format/limit/dedup, RE-CAL-22 scope) were resolved during exploration.
