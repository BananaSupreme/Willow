# Environment Module Low-Level Design (LLD) Document

---

## Summary

The Environment Module is a key component designed to monitor and adapt to the user's computing environment dynamically.
Its primary function is to track the active window and assess the overall system context, facilitating context-aware
responses within the broader system. This module includes interfaces for active window detection and the tagging system.

---

## Interface Definitions

### 1. IActiveWindowDetector

**Purpose**: Defines the interface for identifying the currently active window in the user's system.

**Methods**:

- `ActiveWindowInfo GetActiveWindow()`: Retrieves information about the current active window, such as its title, process ID, and other relevant details.

### 2. IEnvironmentStateProvider

**Purpose**: Provides an interface for accessing comprehensive details about the current state of the environment.

**Public Properties**:

- `IReadOnlyList<Tag> Tags`: An aggregate list of all tags present in the system.

**Iternal Properties**:

- `string OperatingSystem`: Identifies the operating system (e.g., Windows, macOS, Linux).
- `string ActivationMode`: Specifies the current mode of operation.
- `Tag[] EnvironmentTags`: A collection of tags representing various aspects of the current environment.
- `ActiveWindowInfo ActiveWindow`: Details about the currently active window.

---

## Struct Definitions

### 1. ActiveWindowInfo

**Purpose**: Represents detailed information about the currently active window.

**Properties**:
- `string Title`: The title of the active window.
- `int ProcessId`: The process ID of the application associated with the active window.
- `string ProcessName`: The name of the process the window belongs to.

### 2. ActiveWindowChangedEvent

**Purpose**: Signifies a change in the active window on the user's system.

**Properties**:

- `ActiveWindowInfo NewWindow`: Details of the new active window.