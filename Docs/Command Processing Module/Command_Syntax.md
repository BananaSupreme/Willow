# Command Syntax Documentation

## General Syntax Guidelines

- **Plain Word**:
    - Syntax: `"word"`

- **Node Processor with Captured Variable**:
    - Syntax: `NodeProcessorType:CapturedVariableName`

- **Wrapper Node Processor**:
    - Syntax: `WrapperNodeProcessorType[InnerNodeProcessorType:RequiredVariables]:CaturedVariableName`

- **Node with Multiple Variables**:
    - Syntax: `NodeProcessorType:CapturedVariableName{Var1,Var2,Var3}`

- **Alias Usage in Commands**:
    - Description: Symbols do not require the `:` seperator.
    - Example: `*wildcard`

## Known Aliases for Node Processors

- **Number**:
    - Alias: `Number`, `#`, `N`
    - Description: Processes numerical input.

- **Wildcard**:
    - Alias: `WildCard`, `*`
    - Description: Matches any single word.

- **Repeating Wildcard**:
    - Alias: `RepeatingWildCard`, `**`
    - Description: Matches a sequence of words.

- **One Of**:
    - Alias: `OneOf`
    - Special Syntax: `[Option1|Option2|Option3]:VariableName`
    - Special Syntax: `[_capturedListName]:VariableName`
    - Description: Allows selection from a list of predefined options.
    - **Note**: captured lists are enumerated when the command is created and not when it is executed.

- **Optional**:
    - Alias: `Optional`, `?`, `Opt`
    - Wrapper Node
    - This node also accepts a flag at the end, under this flag an `EmptyToken` would be registered in the captured
      values
    - Description: Indicates that the node is optional in a command.

## Examples

1. **Using Number and Wildcard Aliases**:
    - Command: `"increase volume by #amount for *duration"`
    - Breakdown:
        - `increase volume by` - Plain word(s)
        - `#amount` - Alias for Number, capturing the amount to increase
        - `for` - Plain word
        - `*duration` - Alias for Wildcard, capturing the duration term

2. **Using Optional and One Of Nodes**:
    - Command: `"send message to Optional[*recipient]:FlagName [email|phone|postal]:method"`
    - Breakdown:
        - `send message to` - Plain word(s)
        - `Optional[*recipient]:FlagName` - Optional node with Wildcard, capturing the recipient, if captured FlagName
          would be registered
        - `[email|phone|postal]:method` - One Of node, selecting the method of sending the message

3. **Command with Repeating Wildcard at the End**:
    - Command: `"create list titled *title **items"`
    - Breakdown:
        - `create list titled` - Plain word(s)
        - `*title` - Alias for Wildcard, capturing the list title
        - `**items` - Repeating Wildcard, capturing a list of items (rest of the sentence)

4. **Command with Multiple Inputs and Optional Node**:
    - Command: `"schedule meeting on #day at #time ?[*location]:FlagName"`
    - Breakdown:
        - `schedule meeting on` - Plain word(s)
        - `#day` - Alias for Number, capturing the day for the meeting
        - `at` - Plain word
        - `#time` - Alias for Number, capturing the time for the meeting
        - `?[*location]:FlagName` - Optional node with Wildcard, capturing the meeting location if provided

5. **Long-Form Command with Multiple Inputs**:
    - Command: `"convert #amount from [USD|EUR|GBP]:sourceCurrency to [USD|EUR|GBP]:targetCurrency"`
    - Breakdown:
        - `convert` - Plain word
        - `#amount` - Alias for Number, capturing the amount to convert
        - `from` - Plain word
        - `[USD|EUR|GBP]:sourceCurrency` - One Of node, selecting the source currency
        - `to` - Plain word
        - `[USD|EUR|GBP]:targetCurrency` - One Of node, selecting the target currency

6. **Command Using Optional and Number Nodes**:
    - Command: `"set timer for Optional[Number:minutes]:FlagName minutes"`
    - Breakdown:
        - `set timer for` - Plain word(s)
        - `Optional[Number:minutes]:FlagName` - Wrapper Node Processor indicating that the number of minutes is optional

7. **Command with Wildcard and Number Nodes**:
    - Command: `"call Wildcard:contact for Optional[Number:duration]:FlagName minutes"`
    - Breakdown:
        - `call` - Plain word
        - `Wildcard:contact` - Wildcard Node Processor capturing the contact name
        - `for` - Plain word
        - `Optional[Number:duration]:FlagName` - Optional Node Processor indicating an optional duration in minutes

8. **Command Using OneOf Node with Captured List**:
    - Command: `"file document as OneOf:format{_documentFormats}"`
    - Breakdown:
        - `file document as` - Plain word(s)
        - `OneOf:format{_documentFormats}` - OneOf Node Processor using a list variable `_documentFormats` to capture
          the document format

9. **Another Version of a Command Using OneOf Node with Captured List**:
    - Command: `"file document as [_documentFormats]:format"`
    - Breakdown:
        - `file document as` - Plain word(s)
        - `[_documentFormats]:format` - OneOf Node Processor using a list variable `documentFormats` to capture the
          document format, the `_` marks that this is a captured list rather than a plain word.
