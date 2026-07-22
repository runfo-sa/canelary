## Canelary — Requirements Document

### Architecture & Cross-Cutting Concerns

| #   | Requirement                                                                                                                                                                                                                                                                           |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The application is a WPF desktop app targeting `net10.0-windows`.                                                                                                                                                                                                                     |
| A-2 | All modules are Prism `IModule` implementations loaded on demand (`OnDemand = true`) except `Version.Git`/`Version.Database` which load at startup.                                                                                                                                   |
| A-3 | The three swappable service abstractions (`IBackend`, `IVersion`, `IPreview`) are resolved by name-string from `Settings.yaml` via their respective static `ServiceProvider` classes. Misconfiguration raises `NoServiceException`.                                                   |
| A-4 | Global settings are stored in a `Settings.yaml` file (YAML, `YamlDotNet`) loaded as a singleton. Configurable fields: `SqlConnection`, `Theme` (Dark/Light), `Culture`, `Extension`, `VirtualDirectories`, `Preview`, `Backend`, `Version`, `Modules`, `UpdateUrl`. |
| A-5 | An internal SQLite/SQL Server database (`IdeDbContext`) stores IDE-owned data: `Rule`, `RuleAttributes`, `RuleLabel`. An external SQL Server database (`ServiceDbContext`) is used by `Backend.Twins` to query ERP product data.                                                      |
| A-6 | Errors are surfaced through a shared `Logger` and an `ExceptionPopUp` dialog.                                                                                                                                                                                                         |

---

### Module 1 — Editor (`Editor` project)

| #    | Requirement                                                                                                                                                                                                                                                                                                                                                            |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| E-1  | Display a multi-tab ZPL text editor (AvalonEditB) supporting simultaneous editing of multiple label files.                                                                                                                                                                                                                                                             |
| E-2  | Open files from the file system via a dialog, filtering by the configured label extensions (default `e01`, `e02`).                                                                                                                                                                                                                                                     |
| E-3  | Open files by clicking them in the file tree panel (left pane). Already-open files focus their existing tab.                                                                                                                                                                                                                                                           |
| E-4  | Create a new blank ZPL file with default boilerplate (`^XA\r\n\r\n^XZ`). New-file tabs are numbered sequentially.                                                                                                                                                                                                                                                      |
| E-5  | Save the current tab (Ctrl+S equivalent). Prompt to save unsaved changes when closing a tab or closing all tabs. Middle-click on a tab closes it.                                                                                                                                                                                                                      |
| E-6  | Save As — clears the tab's path so the next save triggers a file dialog.                                                                                                                                                                                                                                                                                               |
| E-7  | Save All — saves every open tab.                                                                                                                                                                                                                                                                                                                                       |
| E-8  | ZPL **syntax highlighting** in the editor.                                                                                                                                                                                                                                                                                                                             |
| E-9  | ZPL **intellisense/autocomplete**: typing `^` or `~` opens a completion window with matching ZPL commands.                                                                                                                                                                                                                                                             |
| E-10 | ZPL **linter**: underlines problematic code segments with tooltip error messages. Linting is triggered on tab open and content change. Can be toggled on/off.                                                                                                                                                                                                          |
| E-11 | **Label preview**: renders the current tab's ZPL as a bitmap image via the configured `IPreview` service, displayed in a side panel. Supports configurable DPI and label size (from `SizeList.xml`). Supports rotation (±90°) and size cycling. Multi-label ZPL (multiple `^XA…^XZ` blocks) shows a paged navigation (Previous/Next). Optionally auto-preview on save. |
| E-12 | Preview surfaces backend/rendering errors in a dismissible error window.                                                                                                                                                                                                                                                                                               |
| E-13 | **Print**: sends the processed ZPL to a selected printer via `PrinterHelper`. Printer list is shown in the toolbar.                                                                                                                                                                                                                                                    |
| E-14 | **Resize label**: a dialog lets the user convert a label from one DPI to another. A new tab is created with the rescaled content, named `<original>_<targetDpi>.<ext>`.                                                                                                                                                                                                |
| E-15 | A **command reference menu** provides hover-tooltip documentation for ZPL commands directly in the editor.                                                                                                                                                                                                                                                             |

---

### Module 2 — Comparator (`Comparator` project)

| #   | Requirement                                                                                                |
| --- | ---------------------------------------------------------------------------------------------------------- |
| C-1 | Display a side-by-side diff view between two label files or two versions of the same file.                 |
| C-2 | A selector panel (provided by the active `IVersion` module) allows choosing the files/versions to compare. |
| C-3 | Differences are highlighted using a `Highlighter` helper.                                                  |
| C-4 | An **image mode** view (`ImageMode`) renders both labels as images for visual comparison.                  |
| C-5 | A `SelectLabelsDialog` is used to pick the labels to compare.                                              |
| C-6 | Commands: `GenerateDiff`, `ChangeFiles`, `Refresh`.                                                        |

---

### Module 3 — Verificador / Cohere (`Cohere` project)

| #    | Requirement                                                                                                                                                                                                                                                                          |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| V-1  | Display a file tree on the left. Selecting a label fetches all ERP products associated with that label (via `IBackend.GetProducts`).                                                                                                                                                 |
| V-2  | A **rule** system (stored in `IdeDbContext`) allows the user to define sets of required attributes for a label. A rule has a `Name`, optional `Description`, and one or more `RuleAttributes` (each with `Name`, optional `FixedValue` as a regex pattern, and optional `Comments`). |
| V-3  | Each label can be assigned a rule via a `SelectRuleDialog`. Rules can also be removed from a label. Rules can be created (`CreateRuleDialog`) and edited (`AlterRuleDialog`).                                                                                                        |
| V-4  | When a label has a rule assigned, the module validates every associated product against all rule attributes: fetches attribute values from `IBackend`, then classifies each attribute per product as `None` / `Incomplete` (null/empty value) / `Incoherent` (value fails regex).    |
| V-5  | The **products list** displays each product with a color-coded error state (via `ProductErrorToBackgroundBrushConverter` / `ProductErrorToForegroundBrushConverter`).                                                                                                                |
| V-6  | Clicking a product shows a **product report** panel listing each required attribute, its value, and its error state.                                                                                                                                                                 |
| V-7  | A summary error counter shows the total number of products with errors. A warning indicator appears when no rule is assigned to the selected label.                                                                                                                                  |
| V-8  | A **Generate Sample** dialog (`GenerateSample`) allows the user to select a label and one or more products to print a physical sample label. Only enabled when products exist and a rule is assigned.                                                                                |
| V-10 | A `RefreshListCommand` re-runs validation for the current label.                                                                                                                                                                                                                     |

---

### Module 4 — Publisher (`Publish` project)

| #   | Requirement                                                                                                                                    |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| P-1 | Provides a publish button that fires a `PublishEvent` consumed by the active `IVersion` module.                                                |
| P-2 | The publisher region (`Publicar#Region`) is populated by the active version module (e.g. `Version.Git` registers its own `PublishView` there). |

---

### Version Service — Git (`Version.Git` project)

| #   | Requirement                                                                                                                                                                                                                                                                            |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| G-1 | On startup, validates that the configured label directory is a valid Git repository. If not but a repo URL is set, clones it.                                                                                                                                                          |
| G-2 | `IVersion.ListFiles` returns label files from the local Git working directory, filtered by configured extensions and virtual directories.                                                                                                                                              |
| G-3 | `IVersion.ListVersions` returns Git tags or commit refs for a given file.                                                                                                                                                                                                              |
| G-4 | A status bar widget (`VersionView`) shows the latest Git tag, and detects if the local branch is behind the remote (via `git fetch` + HEAD comparison). Provides an **Update** button that performs `git reset --hard origin/<branch>`.                                                |
| G-5 | A **Push dialog** (`PushView`) shows modified files (from `git status --porcelain`), a branch selector, a tag field, and a commit message. The user selects which files to stage. On confirm: `git add`, `git commit`, `git tag -a`, `git push origin -u <branch>`, `git push --tags`. |
| G-6 | A **Create Branch** dialog allows creating new branches during the push flow.                                                                                                                                                                                                          |
| G-7 | The Comparator's selector (`CompareSelectorView`) is registered by this module to list Git-versioned label files and versions.                                                                                                                                                         |

---

### Version Service — Database (`Version.Database` project)

| #   | Requirement                                                                                                                                                                                                     |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| D-1 | Label files are stored in a SQL Server database. Reading invokes stored procedure `[etiquetas].[GenerarCodigo] @idEtiqueta, @version`. Writing invokes `[etiquetas].[ActualizarEtiqueta] @idEtiqueta, @codigo`. |
| D-2 | Creating a new label invokes `[etiquetas].[CrearEtiqueta] @nombre, @codigo`.                                                                                                                                    |
| D-3 | `ListVersions` returns all stored version numbers for a given label ID.                                                                                                                                         |
| D-4 | A `VirtualFile` wraps each database label as an `IFile`, presenting a `Path` of the form `<name>@<id>`.                                                                                                         |

---

### Preview Service — Labelary (`Preview.Labelary` project)

| #   | Requirement                                                                                                             |
| --- | ----------------------------------------------------------------------------------------------------------------------- |
| L-1 | Calls the Labelary API to render ZPL as PNG byte arrays.                                                                |
| L-2 | Supports a **metadata** block embedded in the ZPL that configures preview behavior (language, extra rendering options). |
| L-3 | `Linting` returns an array of error descriptors (offset, length, message) for use by the Editor linter.                 |
| L-4 | `LoadVariables` replaces ZPL variable placeholders with real product data from the active `IBackend`.                   |

---

### Backend Service — Twins (`Backend.Twins` project)

| #   | Requirement                                                                                                                                                                                  |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| T-1 | Implements `IBackend` against the Twins ERP SQL Server database.                                                                                                                             |
| T-2 | `GetProducts(label)` returns all products linked to a label name.                                                                                                                            |
| T-3 | `GetValues(id)` returns all attribute key-value pairs for a single product.                                                                                                                  |
| T-4 | `GetValues(products, attributes)` returns attribute values for a list of products, filtered to the specified attributes.                                                                     |
| T-5 | `GetAttributes()` returns the full list of available attribute names.                                                                                                                        |
| T-6 | `GetTranslation(languageId, description)` returns the translated string for a language.                                                                                                      |
| T-7 | `LoadVariables` replaces ZPL variable placeholders with product data and reports substitution errors via a `StringBuilder`. Supports a generic `extraData` parameter for extended scenarios. |

---

### Settings & Configuration

| #   | Requirement                                                                                                                       |
| --- | --------------------------------------------------------------------------------------------------------------------------------- |
| S-1 | First-run or missing `Settings.yaml` raises a deserialization error; no auto-generated default file exists.                       |
| S-2 | `VirtualDirectories` defines how label files are grouped into a logical tree (filters by directory prefix patterns).              |
| S-3 | `Modules` list controls which Prism modules are loaded.                                                                           |
| S-4 | `UpdateUrl` points to a release endpoint checked by `Version.Git` for update notifications.                                       |
| S-5 | A **Settings view** (`Core/Views/Settings.xaml`) allows in-app editing of settings, which are serialized back to `Settings.yaml`. |
| S-6 | An **About view** (`Core/Views/About.xaml`) is available.                                                                         |
