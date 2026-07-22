## Why

In the Editor module, the label preview (image, current label index/count, rotation, and zoom/pan) is stored as a single shared state on `PreviewViewModel`, disconnected from which document tab is selected. Switching tabs leaves the previous tab's preview visible (or shows nothing if a preview was never generated), instead of showing the preview that belongs to the tab now in focus. This is confusing when comparing or iterating across multiple open labels.

## What Changes

- Preview state (rendered label images, current label index, total label count, rotation angle, zoom/pan) becomes owned per-tab instead of being a single global state on `PreviewViewModel`.
- Selecting a tab restores that tab's previously generated preview (image, label index, rotation, zoom/pan) if one exists, or clears the preview panel if none was generated yet for that tab.
- Regenerating a preview for a tab (via the existing preview command) keeps that tab's previously applied rotation and zoom/pan instead of resetting them.
- `IEditorPreviewMediator` gains a new command to notify `PreviewViewModel` when the selected tab changes.
- `GeneratePreview` mediator command now carries tab identity (not just raw text) so results are written to the correct tab.
- `Core/Controls/ZoomBorder` gains `GetState()`/`ApplyState(...)` methods so its internal scale/pan transform can be read and restored imperatively from the Preview view's code-behind.

## Capabilities

### New Capabilities
- `editor-tab-preview`: Per-tab ownership and restoration of label preview state (image/labels, rotation, zoom/pan) in the Editor module, keyed to tab selection.

### Modified Capabilities
- (none — no existing specs in this repo yet)

## Impact

- `Editor/Models/TabItem.cs`: add per-tab preview state (labels raw data, current label index, rotation angle, zoom state).
- `Editor/Services/IEditorPreviewMediator.cs` / `EditorPreviewMediator.cs`: add tab-selection-changed command; change `GeneratePreview` command payload to include tab identity.
- `Editor/ViewModels/TextEditorViewModel.cs`: raise the new mediator command when `CurrentTabIndex` changes; pass tab identity into `GeneratePreview`.
- `Editor/ViewModels/PreviewViewModel.cs`: read/write preview state on the active `TabItem` instead of local fields; restore/clear on tab-selection-changed.
- `Editor/Views/Preview.xaml.cs` (code-behind): apply/capture `ZoomBorder` state in response to tab selection and preview regeneration.
- `Core/Controls/ZoomBorder.cs`: expose `GetState()`/`ApplyState(...)`.
