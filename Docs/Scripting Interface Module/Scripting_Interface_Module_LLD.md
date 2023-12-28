# Scripting Interface Module Documentation

---

## Table of Contents

- [Summary](#summary)
- [Interface Definitions](#interface-definitions)
- [Attribute Definitions](#attribute-definitions)
- [Struct Definitions](#struct-definitions)

---

## Summary

The Scripting Interface Module is a critical component within the system, primarily focused on defining and processing
voice commands. Its core function is to provide a structured framework for defining commands in a way that can be
interpreted and executed by the system. This module acts as the primary layer for command creation, enabling developers
to define commands using `IVoiceCommand` and `IVoiceCommandModifier` interfaces.

The module leverages a range of custom attributes, such as `TagAttribute`, `ModeAttribute`, `DescriptionAttribute`,
and `AliasAttribute`, to facilitate detailed command definitions. The central component of the module,
the `ICommandInterpreter`, processes these definitions, converting them into `RawCommand` objects for them to be
incorperated in the system.

A key functionality of the Scripting Interface Module is its ability to dynamically incorporate new interfaces into the
system, achieved through the `IInterfaceRegistrar`. This component enables the dynamic discovery and registration of
interfaces within the system, thereby enhancing the module's adaptability to changing system requirements. This
capability aligns with the systems' design goal of extensibility, ensuring readiness for future system enhancements and
compatibility with new requirements.

---

## Interface Definitions

### 1. IVoiceCommand

**Purpose**: Defines the structure for voice commands within the system.

**Properties**:

- `string InvocationPhrase`: Specifies the phrase that triggers the command.

**Methods**:

- `Task ExecuteAsync(CommandContext context)`: Executes the command logic using the provided `CommandContext`.

### 2. IVoiceCommandModifier

**Purpose**: Alters or extends the behavior of voice commands.

**Properties**:

- `string InvocationPhrase`: Identifies the phrase that triggers the command modifier.

**Methods**:

- `CommandBuilder Modify(CommandBuilder builder)`: Modifies a succefully constructed command.

### 3. ICommandInterpreter

**Purpose**: Translates voice commands and command modifiers into executable `RawCommand` objects.

**Methods**:

- `RawCommand InterpretCommand(IVoiceCommand voiceCommand)`: Converts an `IVoiceCommand` instance into a `RawCommand`.
- `RawCommand InterpretCommand(IVoiceCommandModifier voiceCommand)`: Converts an `IVoiceCommandModifier` instance into a `RawCommand`.

### 4. IInterfaceRegistrar\<T>

**Purpose**: Handles the dynamic registration of interfaces within the system.

**Methods**:

- `void RegisterInterfaces(Assembly[] assemblies)`: Scans provided assemblies for interfaces of type T and registers them within the system.

---

## Attribute Definitions

### 1. TagAttribute

**Purpose**: Used to assign tag requirements to voice commands or command modifiers. These tags are defined by the environment or activated using commands.

### 2. DescriptionAttribute

**Purpose**: Provides a human-readable description of a voice command or modifier.

### 3. AliasAttribute

**Purpose**: Defines alternative phrases or keywords that can be used to invoke a voice command.

---

## Struct Definitions

This section outlines the key structures used in the Scripting Interface Module, describing their roles and compositions.

### 1. RawCommand

**Purpose**: Represents a command definition to be processed internally.

**Properties**:

- `Guid Id`: A unique identifier for the command.
- `string[] InvocationPhrases`: An array of phrases that can trigger the command.
- `TagRequirement TagRequirement`: Specifies the environmental or contextual requirements for the command's execution.
- `Dictionary<string, object> CapturedValues`: Key-value pairs representing parameters or data captured during command interpretation.
- `Function Function`: An enum indicating the type of function the command performs
- `string Description`: A human-readable description of the command's purpose and functionality.

### 2. CommandModifiedEvent

**Purpose**: Used to signal that the set of command definitions within the system has been updated, typically triggered when new commands are added, removed, or modified.

**Composition**:
- `RawCommand[] Commands`: An array of `RawCommand` objects representing the set of commands following the change.