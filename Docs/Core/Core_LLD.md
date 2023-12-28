# Core Documentation

## Interface Definitions

### 1. IEventDispatcher

**Purpose**: Manages the dispatching and interception of events within the system.
**Methods**:

- `void Dispatch<TEvent>(TEvent @event)`: Dispatches an event to be processed by the appropriate handler.
- `void RegisterHandler<IEventHandler<TEvent>>()`: Registers a handler for a specific type of event.
- `void RegisterInterceptor<IEventInterceptor<TEvent>>()`: Adds an interceptor that processes an event and then calls
  the next handler in the chain.
- `void RegisterGenericInterceptor(Type interceptor)`: Adds an open generic interceptor to handle cross-cutting
  concerns.

- #### Supporting Interfaces:
    - `IEventHandler<TEvent>`
        - **Purpose**: Represents a method that handles an event of the specified type, registering for classes allows
          for DI.
        - **Methods**: `Task HandleAsync(TEvent @event)`
    - `IEventInterceptor<TEvent>`
        - **Purpose**: Represents a method that intercepts an event of the specified type, allowing for processing
          before passing to the next handler
        - **Methods**: `Task<TEvent> HandleAsync(TEvent @event, Func<TEvent, Task> next)`

## Struct Definitions

### 1. Tag

**Purpose**: Represents a tag associated with a command.

**Properties**:

- `string Name`: The name of the tag.

### 2. TagRequirement

**Purpose**: Represents a set of tags that are required for a command to be processed.

**Properties**:

- `Tag[] Tags`: The list of tags.

**Methods**:

- `bool IsSatisfied(IEnumerable<Tag> tags)`: Checks if the specified tags satisfy the required tags.