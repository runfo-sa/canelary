## 1. ZoomBorder state API

- [x] 1.1 Add a `ZoomState` type (scale X/Y, translate X/Y) to `Core/Controls/ZoomBorder.cs` or a nearby location.
- [x] 1.2 Add `ZoomBorder.GetState()` returning the current `ZoomState` from the child's `ScaleTransform`/`TranslateTransform`.
- [x] 1.3 Add `ZoomBorder.ApplyState(ZoomState state)` that sets the child's `ScaleTransform`/`TranslateTransform` from a given state (falling back to identity/1.0/0.0 when no state is provided).

## 2. Per-tab preview model

- [x] 2.1 Add a `PreviewState` model (e.g. under `Editor/Models/`) holding `RawLabelsData: List<byte[]>`, `CurrentLabel: int`, `PreviewAngle: double`, `Zoom: ZoomState`.
- [x] 2.2 Add a nullable `Preview` property (`PreviewState?`) to `Editor/Models/TabItem.cs`.

## 3. Mediator changes

- [x] 3.1 Add a `TabSelected` `CompositeCommand` to `IEditorPreviewMediator` and `EditorPreviewMediator`.
- [x] 3.2 Change `GeneratePreview`'s registered command payload type from `string` to `TabItem` (update both the interface usage and the `DelegateCommand<T>` registration in `PreviewViewModel`).

## 4. TextEditorViewModel wiring

- [x] 4.1 In the `CurrentTabIndex` setter, execute `Mediator.TabSelected.Execute(...)` with the newly selected `TabItem` (or `null` when `TabsList` is empty / index invalid).
- [x] 4.2 Update `SendToPreview()` to pass the active `TabItem` (not `.Content.Text`) to `Mediator.GeneratePreview.Execute(...)`.

## 5. PreviewViewModel: write preview results to the tab

- [x] 5.1 Update `GeneratePreview(TabItem tab)` to read `tab.Content.Text` for generation, and on success write `RawLabelsData`/`CurrentLabel` into `tab.Preview` (creating a new `PreviewState` if one doesn't exist), preserving the existing `PreviewAngle`/`Zoom` on that `PreviewState` if it already existed.
- [x] 5.2 Ensure `PreviewImage` displayed after generation reflects the tab's rotation angle (reapply `PreviewAngle` via `TransformedBitmap` if non-zero) so regeneration doesn't visually reset rotation.
- [x] 5.3 Update `PreviousLabel`/`NextLabel` to read/write `CurrentLabel` on the active tab's `PreviewState` instead of the old shared field.
- [x] 5.4 Update `RotateRightCommand`/`RotateLeftCommand` to persist the resulting angle onto the active tab's `PreviewState.PreviewAngle`.

## 6. PreviewViewModel: react to tab selection

- [x] 6.1 Register a handler for `Mediator.TabSelected` in `PreviewViewModel`'s constructor.
- [x] 6.2 On tab selected with `tab.Preview == null` (or `tab == null`): clear `PreviewImage`, reset `CurrentLabel`/`TotalLabel` to 0, and signal the view to reset `ZoomBorder` (see task 7).
- [x] 6.3 On tab selected with `tab.Preview != null`: decode `PreviewImage` from `RawLabelsData[CurrentLabel]`, reapply `PreviewAngle` via `TransformedBitmap`, set `CurrentLabel`/`TotalLabel`, and signal the view to apply the stored `Zoom` state (see task 7).

## 7. Preview view zoom sync (code-behind)

- [x] 7.1 In `Editor/Views/Preview.xaml.cs`, subscribe to the signal from `PreviewViewModel` (task 6.2/6.3) that indicates zoom state should be reset or applied.
- [x] 7.2 On reset signal, call `ZoomBorder.ApplyState(default)` (or `Reset()`).
- [x] 7.3 On apply signal, call `ZoomBorder.ApplyState(tab's stored ZoomState)`.
- [x] 7.4 Capture current zoom state via `ZoomBorder.GetState()` and store it onto the active tab's `PreviewState.Zoom` when the user finishes a zoom/pan interaction (e.g. on mouse wheel / mouse up), so it's available next time task 6.3 fires.

## 8. Verification

- [x] 8.1 Build the solution (`dotnet build Canelary.sln`) and confirm no compile errors from the mediator payload type change.
- [x] 8.2 Manually verify: open two label files, generate a preview for each, switch between tabs and confirm each shows its own image/label count.
- [x] 8.3 Manually verify: switch to a third tab with no generated preview and confirm the panel shows nothing.
- [x] 8.4 Manually verify: rotate and zoom a tab's preview, switch away and back, confirm rotation/zoom/pan are restored.
- [x] 8.5 Manually verify: rotate/zoom a tab's preview, regenerate the preview for that tab, confirm rotation/zoom/pan are preserved (not reset).
