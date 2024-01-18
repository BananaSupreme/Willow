- Source generator that autogenerates the IVoiceCommand from functions, it should copy constructor and private fields
  from the enclosing class to allow for DI
- some notification when two equivilant commands are added, just to make sure the user is aware that the resolution of
  those commands is undetermined and either will execute, also it will always be the same one
- It will be nice if we can inject a version of `IServiceCollection` that can also work in runtime, that way we can
  allow
  modules to register things into the the DI, this will allow us to support complex plugins like adding new systems,
  or adding a new speech engine.
- **Commands that should be built**
    - zoom zoom [in|out|reset]
    - move keys [go|move] [left|right|up|down]
    - copy, paste, cut [copy|paste|cut] that
    - tags
    - abbreviating abbreviate [_list]
    - homophones phone *word
    - press keys [_specialchars], char *any,
    - mode changing
    - repetition and voice command history
    - write formatted
    - write text
    - dictation mode editing
    - move around text