## Why

The "Generar Muestra" dialog (`Cohere/Views/Dialogs/GenerateSample.xaml`) has accumulated UX friction: a redundant Cancelar button, a CMD command textbox that's always visible even when irrelevant, a single-slot command cache that discards history, and a "Generar RE-CAL-22" checkbox for a feature that was never fully implemented and isn't going to be.

## What Changes

- Remove the "Cancelar" button entirely (no replacement affordance needed).
- Rename "Aceptar" to "Imprimir" and move it to the left side of the button row (`DockPanel.Dock="Left"`).
- **BREAKING**: The CMD command textbox is now only visible when the selected printer is `"To PNG"`; it's hidden for all other printers via a computed `IsCmdVisible` view-model property.
- Replace the single-command `after_command.cache` text file with a JSON-based history of up to 10 most-recently-used commands, exposed via an editable dropdown (`ComboBox IsEditable="True"`) instead of a plain textbox. Selecting or re-entering an existing command moves it to the top (MRU); no duplicate entries.
- **BREAKING**: Remove the "Generar RE-CAL-22" feature entirely:
  - The checkbox and its `EnableRecall` property/logic in `GenerateSampleViewModel`.
  - The `GenerateRecallDialog` view, code-behind, and its registration in `CohereModule.cs`.
  - The `ReportTemplate` setting in `Settings.yaml`.
  - The corresponding requirement entry in `REQUIREMENTS.md` (V-9).

## Capabilities

### New Capabilities
- `generate-sample-dialog`: Behavior of the "Generar Muestra" dialog — printer selection, conditional CMD command input with MRU history, and the print action — as it stands after this UX change.

### Modified Capabilities
(none — no existing `openspec/specs/` capabilities were tracked for this dialog before this change)

## Impact

- `Cohere/Views/Dialogs/GenerateSample.xaml` and `GenerateSample.xaml.cs`
- `Cohere/ViewModels/GenerateSampleViewModel.cs`
- `Cohere/Views/Dialogs/GenerateRecallDialog.xaml` and `.xaml.cs` (deleted)
- `Cohere/CohereModule.cs` (dialog registration removed)
- `Main/Settings.yaml` (`ReportTemplate` entry removed)
- `REQUIREMENTS.md` (V-9 entry removed)
- `after_command.cache` file format changes from plain text to JSON array (existing single-command cache files are not migrated; first read simply finds no valid history)
