## Purpose

Behavior of preview state scoping across editor tabs — ensuring each tab retains its own generated preview image, label position, rotation, and zoom/pan independently of other open tabs.

## Requirements

### Requirement: Preview state is scoped to the owning tab
The system SHALL associate generated preview state (rendered label images, current label index, total label count, rotation angle, and zoom/pan) with the specific tab whose content produced it, rather than a single shared state across all tabs.

#### Scenario: Generating a preview writes to the active tab only
- **WHEN** the user generates a preview while tab A is selected
- **THEN** the generated preview state is stored on tab A and does not affect any other open tab's preview state

### Requirement: Selecting a tab restores its own preview
When the user switches the selected tab, the system SHALL display the preview state belonging to the newly selected tab, if one exists.

#### Scenario: Switching to a tab with a previously generated preview
- **WHEN** the user switches from tab A to tab B, and tab B has a previously generated preview
- **THEN** the preview panel displays tab B's image, current label index/total, rotation, and zoom/pan exactly as they were when tab B was last active

#### Scenario: Switching to a tab with no generated preview
- **WHEN** the user switches to a tab for which no preview has ever been generated
- **THEN** the preview panel shows no image and no label counters

#### Scenario: Switching back to a previously viewed tab
- **WHEN** the user switches from tab A to tab B and then back to tab A, without closing tab A
- **THEN** the preview panel shows tab A's preview state exactly as it was before switching away

### Requirement: Rotation and zoom/pan persist per tab across regeneration
The system SHALL preserve a tab's previously applied preview rotation angle and zoom/pan state when the preview for that tab is regenerated.

#### Scenario: Regenerating a preview after rotating it
- **WHEN** the user rotates a tab's preview image and then regenerates the preview for that same tab
- **THEN** the newly generated preview image is displayed at the same rotation angle the user had applied

#### Scenario: Regenerating a preview after zooming/panning it
- **WHEN** the user zooms and pans a tab's preview image and then regenerates the preview for that same tab
- **THEN** the newly generated preview image is displayed with the same zoom level and pan position the user had applied
