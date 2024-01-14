- Give the parser the ability to have words that are known homophones to also be interpreted by converting word tokens
  to OneOf tokens, Let's also consider that whisper tries to make the command into actual propper english, so sometimes
  it will not find a homophone but it will find a variation of the word (not sure how its called) that is correct in the
  grammar for example `take` get transformed into `taking` this can also be added with the homophones.
- Give the user the ability to add other words that are difficult to parse for them, a custom homophone library
- Theres probably a lot of performance tuning that can be done here

---

Here is the initial idea on how to solve the problem of adding homophones into the system.

We will add the Match function to all tokens, this will take another token and ask whether the two hold the more complex
idea of equivilance that exists with the transcription parsing.

A new token will be added that will include a couple of fields, The original word recorded, and all of its equivilants
for the porpuse of parsing it will be named `SemanticVariantToken`, it would be able to be explicitly cast as
a `WordToken` and it's `ToString` function will return the originally captured word. but if the match function will be
invoked, it will be caught if the string is equal to any of the words represneted.

A new `ISpecializedNodeProcessor` will be created that knows to create this token using a dictionary of equivilants
recovered from DI.

`CapturingNodeProcessor` would add a new abstract method `GetTokenToCapture` that will return the correct token expected,
for example `OneOf` returns the token asked in the match, we might want to think how to avoid double checking this for
if the collection in `OneOf` is large it can be difficult. `WildCard` Can return the `WordToken` representation of the
new token.


---

- Some analyzer can be nice, for example for known fail states that can be derived at compile time (known nodes that
  will fail to compile, invalid states like "something **rest more stuff" as the rest would absorb the rest of the
  tokens and the command could never be executed)
- Expand on the issues and exceptions that the command compiler throws, our users will certainly appreciate the issues
  floated
- We should certainly look into caching repeated commands, no point in processing a sentatnce twice if the user said the
  same thing.