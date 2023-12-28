- Give the parser the ability to have words that are known homophones to also be interpreted by converting word tokens
  to OneOf tokens, Let's also consider that whisper tries to make the command into actual propper english, so sometimes
  it will not find a homophone but it will find a variation of the word (not sure how its called) that is correct in the
  grammar for example `take` get transformed into `taking` this can also be added with the homophones.
- Give the user the ability to add other words that are difficult to parse for them, a custom homophone library
- Theres probably a lot of performance tuning that can be done here
- Some analyzer can be nice, for example for known fail states that can be derived at compile time (known nodes that
  will fail to compile, invalid states like "something **rest more stuff" as the rest would absorb the rest of the
  tokens and the command could never be executed)
- Expand on the issues and exceptions that the command compiler throws, our users will certainly appreciate the issues
  floated
- We should certainly look into caching repeated commands, no point in processing a sentatnce twice if the user said the
  same thing.
- Errors should not bring down the entire compilation process but just eliminate the specific command that failed.
