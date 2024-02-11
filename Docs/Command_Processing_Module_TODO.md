- Theres probably a lot of performance tuning that can be done here
- Some analyzer can be nice, for example for known fail states that can be derived at compile time (known nodes that
  will fail to compile, invalid states like "something **rest more stuff" as the rest would absorb the rest of the
  tokens and the command could never be executed)
- Expand on the issues and exceptions that the command compiler throws, our users will certainly appreciate the issues
  floated
- A warning for the user that commands have become equivilant could be nice, but we can also apply a general warning
  when the user 