## ADDED Requirements

### Requirement: Single print action
The "Generar Muestra" dialog SHALL present a single primary action, labeled "Imprimir", docked to the left of the button row. The dialog SHALL NOT provide a separate "Cancelar" button.

#### Scenario: User confirms printing
- **WHEN** the user clicks "Imprimir"
- **THEN** the system prints/generates the selected products using the current printer, CMD command (if applicable), and then closes the dialog

### Requirement: Conditional CMD command input
The CMD command input SHALL be visible only when the selected printer is `"To PNG"`. For any other printer selection, the CMD command input SHALL be hidden and its value SHALL NOT be applied during printing.

#### Scenario: "To PNG" printer selected
- **WHEN** the user selects "To PNG" as the printer
- **THEN** the CMD command input becomes visible

#### Scenario: Physical printer selected
- **WHEN** the user selects a printer other than "To PNG"
- **THEN** the CMD command input is hidden and no post-print command is executed

### Requirement: CMD command history
The CMD command input SHALL be an editable dropdown backed by a most-recently-used (MRU) history of up to 10 commands, persisted across sessions. Selecting an entry from the dropdown SHALL populate the input with that command; the user MAY also type a new command directly.

#### Scenario: Command history populated
- **WHEN** the dialog opens and a command history exists
- **THEN** the dropdown lists up to 10 previously used commands, most recently used first

#### Scenario: New command runs and is saved
- **WHEN** the user enters a command not present in history and it runs successfully after printing
- **THEN** the command is added to the top of the history; if the history exceeds 10 entries, the oldest entry is dropped

#### Scenario: Reusing an existing command
- **WHEN** the user selects or re-enters a command that already exists in history
- **THEN** the history is reordered so that command becomes the most recent entry, without creating a duplicate

## REMOVED Requirements

### Requirement: Generar RE-CAL-22 checkbox
**Reason**: The RE-CAL-22 recall report feature was never fully implemented and will not be pursued; the dialog should not offer this option.
**Migration**: None — no replacement is provided. Any workflow depending on generating a RE-CAL-22 report from this dialog is discontinued.

#### Scenario: Checkbox no longer present
- **WHEN** the "Generar Muestra" dialog is opened
- **THEN** no "Generar RE-CAL-22" checkbox or related recall-report action is shown
