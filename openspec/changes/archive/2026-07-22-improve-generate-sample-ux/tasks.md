## 1. Remove RE-CAL-22 feature

- [x] 1.1 Delete `Cohere/Views/Dialogs/GenerateRecallDialog.xaml` and `GenerateRecallDialog.xaml.cs`
- [x] 1.2 Remove `containerRegistry.RegisterDialog<GenerateRecallDialog>();` from `Cohere/CohereModule.cs`
- [x] 1.3 Remove `EnableRecall` property and `GenerateRecall(...)` method from `Cohere/ViewModels/GenerateSampleViewModel.cs`, and stop calling `GenerateRecall(...)` from `ClosingDialog()`
- [x] 1.4 Remove the `ReportTemplate` entry from `Main/Settings.yaml`
- [x] 1.5 Remove the V-9 requirement entry for `GenerateRecallDialog` from `REQUIREMENTS.md`
- [x] 1.6 Grep the repo for `GenerateRecallDialog`, `ReportTemplate`, and `RE-CAL-22` to confirm no remaining references

## 2. Simplify action buttons

- [x] 2.1 Remove the "Cancelar" button from `Cohere/Views/Dialogs/GenerateSample.xaml`
- [x] 2.2 Rename the "Aceptar" button's `Content` to "Imprimir" and change its `DockPanel.Dock` to `Left`
- [x] 2.3 Verify `CloseDialogCommand` binding on the renamed button still triggers `ClosingDialog()` (print, then close) with `GenerateRecall` no longer part of that flow

## 3. Conditional CMD command visibility

- [x] 3.1 Add `IsCmdVisible` computed bool property to `GenerateSampleViewModel`, true when `Printer == TO_PNG`
- [x] 3.2 Raise `PropertyChanged` for `IsCmdVisible` in the `Printer` property setter
- [x] 3.3 Bind the CMD `DockPanel`'s `Visibility` in `GenerateSample.xaml` to `IsCmdVisible` via `BooleanToVisibilityConverter`
- [x] 3.4 Manually verify: selecting "To PNG" shows the CMD row; selecting any other printer hides it

## 4. CMD command MRU history

- [x] 4.1 Add `AfterCommandHistory` (`ObservableCollection<string>` or similar) property to `GenerateSampleViewModel`
- [x] 4.2 Replace `LoadCachedAfterCommand()` with JSON deserialization of `after_command.cache` into the history list; treat missing file or parse failure (e.g. old plain-text format) as empty history
- [x] 4.3 Replace `SaveCachedAfterCommand()` with MRU logic: remove existing equal entry (if any), insert the current `AfterCommand` at index 0, truncate to 10 entries, serialize and write back to `after_command.cache`
- [x] 4.4 Change the CMD `TextBox` in `GenerateSample.xaml` to `ComboBox IsEditable="True"` bound to `ItemsSource="{Binding AfterCommandHistory}"` and `Text="{Binding AfterCommand}"`
- [x] 4.5 Manually verify: run 3+ distinct commands across dialog sessions, confirm history shows most-recent-first, no duplicates, and caps at 10 entries

## 5. Final verification

- [x] 5.1 Build `Canelary.sln` and confirm no compile errors from removed types/members
- [x] 5.2 Manually exercise the "Generar Muestra" dialog end-to-end: printer selection, CMD visibility toggle, command history reuse, and "Imprimir" closing the dialog after a successful print
