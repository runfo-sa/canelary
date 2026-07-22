## Context

The Editor module's tab list (`TextEditorViewModel.TabsList`) and its label preview panel (`PreviewViewModel`) are separate Prism VMs wired only through `IEditorPreviewMediator` (a hub of `CompositeCommand`s — `GeneratePreview`, `SendErrors`, `GenerateLinter`, `SendData`). Today `PreviewViewModel` holds one shared set of preview fields (`_labelsRawData`, `PreviewImage`, `CurrentLabel`, `TotalLabel`, `PreviewAngle`) regardless of which tab is selected, and `GeneratePreview(string content)` only receives raw ZPL text — it has no notion of which `TabItem` the text came from. `TextEditorViewModel.CurrentTabIndex` changing today only re-evaluates `CanExecute` on a few commands; it never touches the mediator or preview state. Zoom/pan is handled entirely inside `Core/Controls/ZoomBorder`, manipulating `RenderTransform` directly from mouse events, with no bindable properties or exposed state.

## Goals / Non-Goals

**Goals:**
- Preview (image/labels, current label index, rotation angle, zoom/pan) is owned per tab and follows tab selection.
- Switching to a tab with no generated preview shows an empty preview panel.
- Switching to a tab with a previously generated preview restores it exactly as left (including rotation and zoom/pan).
- Regenerating a preview for a tab preserves that tab's existing rotation and zoom/pan (does not reset to 0°/1x).

**Non-Goals:**
- Changing how preview generation itself works (`PreviewServiceProvider`/`IPreview`/Labelary integration untouched).
- Persisting preview state across app restarts or to disk — in-memory only, scoped to the `TabItem` instance lifetime (cleared when the tab is closed).
- Making `ZoomBorder` fully MVVM-bindable via DependencyProperties (see Decisions).

## Decisions

**1. Per-tab preview state lives on `TabItem`, not a side dictionary in `PreviewViewModel`.**
`TabItem` already owns other per-tab UI state (`LintingData`, `HasUnsavedChanges`). Adding a `Preview` property (nullable, e.g. `PreviewState?`) keeps ownership consistent with that existing pattern and avoids a second parallel lookup structure (`Dictionary<TabItem, ...>`) that needs manual lifecycle cleanup when tabs close. Alternative considered: dictionary in `PreviewViewModel` keyed by `TabItem` — rejected because it duplicates tab lifecycle management (removal on tab close) that `TabItem`'s own lifetime already handles for free.

`PreviewState` holds: `RawLabelsData: List<byte[]>`, `CurrentLabel: int`, `PreviewAngle: double`, `Zoom: ZoomState` (scale/translate). `PreviewImage` (the decoded `BitmapSource` for `CurrentLabel`) is not stored — it's cheap to re-decode from `RawLabelsData[CurrentLabel]` whenever displayed, avoiding keeping both the compressed bytes and decoded bitmap alive per tab.

**2. Tab-selection change is communicated via a new mediator command, `TabSelected`.**
`TextEditorViewModel` and `PreviewViewModel` currently have zero direct references to each other — all interop goes through `IEditorPreviewMediator`. Keeping that pattern (vs. giving `PreviewViewModel` a direct reference to `TextEditorViewModel.TabsList`/`CurrentTabIndex`) preserves the existing decoupling and matches how linting/error data already flows. `TextEditorViewModel` executes `Mediator.TabSelected.Execute(tab)` (nullable `TabItem`) whenever `CurrentTabIndex` changes (including when it becomes invalid, e.g. all tabs closed).

**3. `GeneratePreview` mediator command payload changes from `string` to carry tab identity.**
`PreviewViewModel.GeneratePreview` needs to write results onto the correct `TabItem`. Since `DelegateCommand<T>` supports one payload type, the payload changes from `string content` to the `TabItem` itself (reading `.Content.Text` internally) rather than adding a second command or a tuple. This is a breaking change to the mediator interface — acceptable since it's internal to the Editor module (no external consumers).

**4. `ZoomBorder` exposes state via `GetState()`/`ApplyState(ZoomState)` methods, not DependencyProperties.**
`ZoomBorder` currently manipulates its `RenderTransform` (`ScaleTransform`/`TranslateTransform`) directly and imperatively in mouse-event handlers; there's no existing binding surface. Converting to full two-way-bindable DependencyProperties would touch every mutation site (mouse wheel, drag, `Reset()`) for a control that otherwise works correctly. Two plain public methods (get current scale/translate as a small struct/record `ZoomState`, and apply one) are the minimal surface needed. The Preview view's code-behind (`Preview.xaml.cs`) becomes responsible for calling these in response to a ViewModel-raised event (or by reading `PreviewViewModel` state directly) when the tab selection changes or a preview is regenerated, since this can't be pure XAML binding.

**5. `PreviewViewModel` reacts to `TabSelected` by restoring or clearing displayed state; it does not auto-regenerate.**
Selecting a tab with `Preview == null` clears `PreviewImage`/label counters (shows nothing), it does not trigger a new preview generation (unchanged from today — preview generation stays an explicit user action via the existing preview command).

## Risks / Trade-offs

- **[Risk] Changing `GeneratePreview`'s payload type is a breaking change to `IEditorPreviewMediator`.** → Mitigation: the interface is internal to the `Editor` project/module; only `TextEditorViewModel` (producer) and `PreviewViewModel` (consumer) reference it — both updated together in this change.
- **[Risk] Code-behind involvement in `Preview.xaml.cs` for zoom sync breaks strict MVVM.** → Mitigation: scoped narrowly to reading/writing `ZoomBorder`'s transform state, which was already outside the ViewModel's reach; no business logic moves into code-behind.
- **[Risk] Memory growth from retaining `RawLabelsData` (raw label image bytes) per open tab indefinitely.** → Mitigation: data is cleared when a tab is closed (`TabItem` goes out of scope); typical open-tab counts and label image sizes are small enough this is not expected to be significant, consistent with existing behavior where a single generated preview was already held in memory.

## Open Questions

None outstanding — rotation and zoom/pan persistence-per-tab confirmed in scope; `ZoomBorder` API shape (methods vs. DependencyProperties) confirmed as methods.
