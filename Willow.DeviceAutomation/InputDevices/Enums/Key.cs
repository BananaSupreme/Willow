namespace Willow.DeviceAutomation.InputDevices.Enums;

/// <summary>
/// Keyboard key.
/// </summary>
/// <remarks>
/// The system does not guarantee numbering and position of this enum and it should never be used by its underlying value.
/// </remarks>
//Keys commented off are not yet implemented in the package we are actually using to press keys
public enum Key
{
    None,

    //Letters
    A, B, C, D, E, F, G, H, I, J, K, L, M,
    N, O, P, Q, R, S, T, U, V, W, X, Y, Z,

    //Numbers
    Num0, Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9,

    //FunctionKeys
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, //F13, F14, F15, F16, F17, F18, F19, F20, F21, F22, F23, F24,

    //Control
    LeftShift, LeftControl, LeftAlt, LeftCommand, //RightShift,  RightControl,  RightAlt,  RightCommand,
    LeftOption, Tab, //LeftCommandOrControl, RightCommandOrControl,  RightOption, Windows,  Space, Function,

    //Navigation
    LeftArrow, RightArrow, UpArrow, DownArrow,
    Home, End, PageUp, PageDown,

    //Editing
    Enter, Backspace, Insert, Delete,

    //Numpad
    //Numpad0, Numpad1, Numpad2, Numpad3, Numpad4, Numpad5, Numpad6, Numpad7, Numpad8, Numpad9,
    //NumpadMultiply, NumpadAdd, NumpadEnter, NumpadSubtract, NumpadDecimal, NumpadDivide,

    //System
    Escape, PrintScreen, PauseBreak,

    //Multimedia
    //PlayPause, Stop, NextTrack, PreviousTrack,
    //VolumeUp, VolumeDown, Mute,

    //Special
    Dollar, Percent, //Tilde, Exclamation, At, Hash, Caret, Ampersand, Asterisk,
    OpenParenthesis, CloseParenthesis, Minus, Plus, Equal, //Underscore, 
    OpenBracket, CloseBracket, OpenBrace, CloseBrace, Backslash, //Pipe, Apostrophe,
    Semicolon, Colon, Quotation, Comma, LessThan, Dot, GreaterThan, Slash, Question,

    //Locks    
    CapsLock, NumLock, ScrollLock //, FnLock
}