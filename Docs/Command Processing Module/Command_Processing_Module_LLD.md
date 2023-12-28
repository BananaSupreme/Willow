# Command Processing Module LLD Documentation

---

## Table of Contents

- [Summary](#summary)
- [Architecture Rationals](#architecture-rationale)
- [Components Overview](#components-overview)
- [Interface Definitions](#interface-definitions)
- [Abstract Class Definitions](#abstract-class-definitions)
- [Class Definitions](#class-definitions)
- [Struct Definitions](#struct-definitions)
- [Enum Definitions](#enum-definitions)

---

## Summary

The Command Processing Module is designed to interpret voice-based commands. This module handles the workflow from
receiving transcribed audio to parsing it into actionable commands. It starts by
tokenizing text into discrete units (tokens) and then parses these tokens against predefined command patterns using a
Trie.
The `Trie` class walks through all of the commands in a DFS manner and checks if the command becomes satisfied at some
point. The core functionality of this module includes identifying and validating voice commands, and upon successful
parsing, it emits a `CommandParsedEvent`.

---

## Architecture Rationale

### Trie Traversal and Weighing System for Command Matching

- **Decision**: Implementing a Trie structure with a systematic weighing system for command matching.
- **Rationale**:
    - The Trie structure is structured to organize command patterns hierarchically. It allows for efficient searching
      and matching of command phrases, following a specific traversal order.
    - Command specificity in relation to the environment is evaluated at runtime, with nodes tagged and ordered based on
      how well they match the current context. This dynamic assessment enhances the precision of command matching.
    - Processor weights are determined based on the specificity of the tokens they can match. Nodes that match exact
      words or numbers are given higher weights compared to nodes handling wildcards or optional elements. This ensures
      that specific commands are prioritized over more general ones, effectively handling overlapping command patterns.
    - **Rationale for Not Parallelizing Trie Traversal**:
        - The order in which nodes are traversed in the Trie is critical for ensuring the correct interpretation of
          commands, especially in cases where command patterns may overlap or be similar.
        - Parallelizing the traversal could disrupt this carefully established order, potentially leading to incorrect
          or less optimal command matches.
        - The weighing system ensures that the most relevant or likely command is matched first, which is essential for
          the system’s accuracy and efficiency.
    - **Guidelines for Weighing Decisions**:
        - Weights are assigned based on token specificity: fewer tokens a node can match, the higher its weight should
          be.

### Flexibility with Interfaces for Tokens and Node Processors

- **Decision**: Utilizing interfaces and abstract classes for tokens, node processors and their parsers.
- **Rationale**:
    - This design allows for modular and extendable architecture. New types of tokens and node processors can be added
      seamlessly, catering to evolving requirements.
    - External modules can modify the system's parsing capabilities through Dependency Injection, enhancing adaptability
      without core architecture changes.
    - **Guidelines for Implementing Weights and Syntax**:
        - New command parsers should adhere to the long syntax format (`NodeProcessorType:CapturedVariableName`) for
          consistency, and avoid using symbol notation, as this could be used internally in the core system.

---

## Components Overview

The Command Processing Module comprises several key components that work together to interpret and parse voice commands:

- **Tokenization**: This process involves breaking down the spoken input into tokens, such as words and numbers.

- **Command Parsing**: Parses the raw command data into `Node` objects that the tokens would be evaluated against.

- **Trie Structure**: Used for efficient parsing of the tokenized input. It organizes command patterns in a hierarchical
  manner.

- **Nodes**: Various types of nodes within the Trie (like WordNode, NumberNode, etc.) represent different segments of a
  command.

- **Command Builder**: Responsible for assembling the parsed data into a structured `Command` object, which encapsulates
  the necessary information for command execution.

---

## Interface Definitions

### 1. ITokenizer

**Purpose**: Interface for converting raw input strings into a series of tokens.

**Methods**:

- `List<Token> Tokenize(string input)`: Breaks down the input string into tokens.

### 2. ISpecializedTokenProcessor

**Purpose**: Interface for processing a specific type of token.

**Methods**:

- `TokenProcessingResult Process(ReadOnlySpan<char> input)`: Attempts to parse the input string into a token.

### 3. ICommandCompiler

**Purpose**: Interface for compiling preprocessed commands into a sequence of nodes.

**Methods**:

- `NodeProcessor[] Compile(PreProcessedCommand command)`: Converts a `PreProcessedCommand` into a corresponding list of
  node processors.

### 4. ISpecializedCommandProcessor

**Purpose**: Interface for processing a specific type of `NodeProcessor`.

**Methods**:

- ` (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord, IDictionary<string, object> capturedValues, IEnumerable<ISpecializedCommandParser> specializedCommandParsers)`:
  Attempts to parse the input span into a `NodeProcessor`.

### 5. ITrieFactory

**Purpose**: Interface for creating and managing Trie instances for command parsing.

**Properties**:

- `bool IsLeaf`: Does the node represent a leaf in Trie. Considering the Trie should always involve some action on the
  CommandBuilder in the end only certain processors can be represented as a leaf node. We should also avoid finalizing
  the command if a more specific command can fit, so leaf nodes are read last.
- `uint Weight`: Defines the weight of the node where lower is higher ranked.

**Methods**:

- `void Set(Function function, List<PreProcessedCommand> commands)`: Constructs and caches a Trie for a specific
  function and set of commands.
- `Trie Get(Function function)`: Retrieves a cached Trie based on the specified function.

### 6. INodeProcessor

**Purpose**: Represents a step in the command parsing process.

**Public Methods**:

- `NodeProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder, Tag[] environmentTags)`:
  Abstract method to process tokens and advance in the Trie.

---

## Abstract Class Definitions

### 1. Token

**Purpose**: Represents a piece of processed input.

**Methods**:

- `T Get<T>()`: Retrieves the token's value as a strongly typed value.

### 2. CapturingNodeProcessor

**Purpose**: Base class for nodes that capture specific values from tokens.

**Inherits From**: `Node`

**Protected Properties**:

- `string Name`: The name used to identify the captured parameter.

**Protected Methods**:

- `abstract bool IsTokenMatch(Token token)`: Checks if the token matches, if it does adds it as a parameter and
  continues processing.

---

## Class Definitions

### Tokens

#### 1. WordToken

**Role**: Represents a word in the user's input.

**Inherits From**: `Token`

#### 2. NumberToken

**Role**: Represents a numerical value in the user's input.

**Inherits From**: `Token`

### NodeProcessors

#### 1. RootNodeProcessor

**Role**: Serves as the starting point for Trie processing, holding the initial set of child nodes.

**Inherits From**: `NodeProcessor`

#### 2. ModifierNodeProcessor

**Role**: Represents a command modifier in the Trie structure.

**Inherits From**: `NodeProcessor`

#### 3. CommandSuccessNodeProcessor

**Role**: Marks a successful command recognition point in the Trie.

**Inherits From**: `NodeProcessor`

#### 4. WordNodeProcessor

**Role**: Processes word tokens in the Trie.

**Inherits From**: `NodeProcessor`

#### 5. OptionalNodeProcessor

**Role**: Manages optional elements in a command structure.

**Inherits From**: `NodeProcessor`

#### 6. NumberNodeProcessor

**Role**: Processes number tokens in the Trie.

**Inherits From**: `CapturingNodeProcessor`

#### 7. WildcardNodeProcessor

**Role**: Handles wildcard inputs in command patterns.

**Inherits From**: `CapturingNodeProcessor`

#### 8. OneOfNodeProcessor

**Role**: Handles a choice of various words.

**Inherits From**: `CapturingNodeProcessor`

#### 9. RepeatingWildcardNodeProcessor

**Role**: Processes sequences of wildcard inputs for variable command parts.

**Inherits From**: `CapturingNodeProcessor`

### Functional

#### 1. CommandBuilder

**Role**: Constructs `Command` objects during the parsing process.

**Properties**

- `bool IsSuccessful`: Marked when the command can be successfully built.

**Methods**:

- `CommandBuilder Create()`: Creates a new instance of the builder.
- `CommandBuilder AddParameter(string name, Token value)`: Adds a parameter to the current command.
- `CommandBuilder Success(Guid commandId)`: Marks the command as successfully built and stores the command ID.
- `CommandBuilder Success()`: Marks the command as successfully built, without a command ID.
- `CommandBuilder Chain(Command command)`: Adds another command to the chain.
- `CommandBuilder AsFailed()`: Returns a builder clone with the success status set to false.
- `(bool IsSuccessful, Command[] Commands) TryBuild()`: Finalizes the building process and returns the constructed
  commands and success status.

#### 2. Trie

**Role**: Represents a node in the Trie structure, actual processing gets delegated to a `NodeProcessor` instance.

**Methods**:

- `(bool IsSuccessful, Command[] Commands) TryTraverse(ReadOnlyMemory<Token> tokens, IEnumerable<Tag> ActiveTags)`:
  Processes the tokens and active tags to construct commands.

### 3. Node

**Purpose**: Represents each step in the command parsing process.

**Public Methods**:

- `public (CommandBuilder Builder, ReadOnlyMemory<Token> RemainingTokens) ProcessToken(
  ReadOnlyMemory<Token> tokens, CommandBuilder builder, Tag[] environmentTags)`: Abstract
  method to process tokens and advance in the Trie, returning how many tokens were parsed as well as the builder, we're
  returning the tokens so modifiers can be parsed afterwords.
- `bool IsSatisfyingTagRequirements(IEnumerable<Tag> tags)`: Method to check if the node should be processed in the
  current environment.
-

#### 4. NodeBuilder

**Role**: Constructs `Node` objects from a set of processors.

**Methods**:

- `NodeBuilder Create()`: Creates a new instance of the builder.
- `NodeBuilder AddChild(NodeBuilder child)`: Adds a child `Node` to the current `Node`.
- `NodeBuilder AddTagRequirements(TagRequirement tagRequirement)`: Adds tag requirements to the `Node`.
- `NodeBuilder SetNodeProcessor(INodeProcessor nodeProcessor)`: Sets the NodeProcessor for the `Node`.
- `Node Build()`: Finalizes the building process and returns the constructed commands and success status.

### Event Handlers

#### 1. AudioTranscribedEventHandler

**Role**: Handles the `AudioTranscribedEvent` and initiates the command processing pipeline.

**Methods**:

- `Task Handle(AudioTranscribedEvent @event)`: Processes the transcribed audio text and starts the command parsing
  process.

#### 2. CommandsModifiedEventHandler

**Role**: Manages updates to the command definitions, affecting the Trie structure.

**Methods**:

- `Task Handle(CommandsModifiedEvent @event)`: Responds to changes in command definitions and updates the Trie
  accordingly.

### Interceptors

#### 1. PunctuationRemoverInterceptor

**Role**: Processes the `AudioTranscribedEvent` to remove punctuation and special characters from the transcribed text.

**Methods**:

- `AudioTranscribedEvent Intercept(AudioTranscribedEvent event)`: Modifies the event's content by removing punctuation
  and special characters.

---

## Struct Definitions

### 1. Command

**Purpose**: Represents a parsed command ready for execution.

**Properties**:

- `Guid Id`: The unique identifier of the command.
- `Dictionary<string, object> Parameters`: Captures parameters associated with the command.

### 2. PreProcessedCommand

**Purpose**: Represents a command before it is added to the Trie, containing necessary information for Trie
construction.

**Properties**:

- `Guid Id`: A unique identifier for the command.
- `string CommandString`: The actual command string representing the command pattern.
- `Tag[] Tags`: Tags associated with the command for context-specific applicability.
- `Dictionary<string, object> CapturedValues`: Captured values from the command pattern.
- `Function CommandType`: Indicates whether the command is a regular command or a modifier.

### 3. CommandParsedEvent

**Purpose**: Wraps a command to be executed by the pipeline.

**Properties**:

- `Command Command`: The `Command` object.

### 4. TokenProcessingResult

**Purpose**: Result of attempting to parse a string into a token.

**Properties**:

- `bool IsSuccessful`: Indicates whether the token was successfully processed.
- `Token Token`: The processed token.
- `int CharsProcessed`: The number of characters processed.

### 5. NodeProcessingResult

**Purpose**: Result of attempting to process a token from a `NodeProcessor`.

**Properties**:

- `bool IsSuccessful`: Indicates whether the token was successfully processed.
- `CommandBuilder Builder`: The modified builder.
- `ReadOnlyMemory<Token> RemainingTokens`: The remaining tokens to be processed.

---

## Enum Definitions

### 1. Function

**Purpose**: Categorizes different types of Trie functions or contexts within the command processing system.

**Members**:

- `Command`: Represents the Trie function for standard command processing.
- `Modifier`: Signifies the Trie function for handling command modifiers.

### 1. TokenType

**Purpose**: Categorizes different types of tokens.

**Members**:

- `Empty`: Represents an empty token.
- `Word`: Represents a word token.
- `Number`: Signifies a number token.
