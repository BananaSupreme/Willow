﻿using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.DeviceAutomation.InputDevices.Windows.Exceptions;

namespace Willow.DeviceAutomation.InputDevices.Windows.Extensions;

internal static class KeyExtensions
{
    public static char GetAssociatedChar(this Key key)
    {
        return key switch
        {
            Key.A => 'a',
            Key.B => 'b',
            Key.C => 'c',
            Key.D => 'd',
            Key.E => 'e',
            Key.F => 'f',
            Key.G => 'g',
            Key.H => 'h',
            Key.I => 'i',
            Key.J => 'j',
            Key.K => 'k',
            Key.L => 'l',
            Key.M => 'm',
            Key.N => 'n',
            Key.O => 'o',
            Key.P => 'p',
            Key.Q => 'q',
            Key.R => 'r',
            Key.S => 's',
            Key.T => 't',
            Key.U => 'u',
            Key.V => 'v',
            Key.W => 'w',
            Key.X => 'x',
            Key.Y => 'y',
            Key.Z => 'z',
            Key.Num0 => '0',
            Key.Num1 => '1',
            Key.Num2 => '2',
            Key.Num3 => '3',
            Key.Num4 => '4',
            Key.Num5 => '5',
            Key.Num6 => '6',
            Key.Num7 => '7',
            Key.Num8 => '8',
            Key.Num9 => '9',
            Key.Tab => '\t',
            Key.Space => ' ',
            Key.Dollar => '$',
            Key.Percent => '%',
            Key.Tilde => '~',
            Key.Exclamation => '!',
            Key.At => '@',
            Key.Hash => '#',
            Key.Caret => '^',
            Key.Ampersand => '&',
            Key.Asterisk => '*',
            Key.OpenParenthesis => '(',
            Key.CloseParenthesis => ')',
            Key.Minus => '-',
            Key.Plus => '+',
            Key.Equal => '=',
            Key.Underscore => '_',
            Key.OpenBracket => '[',
            Key.CloseBracket => ']',
            Key.OpenBrace => '{',
            Key.CloseBrace => '}',
            Key.Backslash => '\\',
            Key.Pipe => '|',
            Key.Apostrophe => '\'',
            Key.Semicolon => ';',
            Key.Colon => ':',
            Key.Quotation => '"',
            Key.Comma => ',',
            Key.LessThan => '<',
            Key.GreaterThan => '>',
            Key.Dot => '.',
            Key.Slash => '/',
            Key.Question => '?',
            _ => throw new InvalidVirtualKeyException(key)
        };
    }

    public static ushort GetVirtualKeyCode(this Key key)
    {
        return key switch
        {
            Key.A => 0x41,
            Key.B => 0x42,
            Key.C => 0x43,
            Key.D => 0x44,
            Key.E => 0x45,
            Key.F => 0x46,
            Key.G => 0x47,
            Key.H => 0x48,
            Key.I => 0x49,
            Key.J => 0x4A,
            Key.K => 0x4B,
            Key.L => 0x4C,
            Key.M => 0x4D,
            Key.N => 0x4E,
            Key.O => 0x4F,
            Key.P => 0x50,
            Key.Q => 0x51,
            Key.R => 0x52,
            Key.S => 0x53,
            Key.T => 0x54,
            Key.U => 0x55,
            Key.V => 0x56,
            Key.W => 0x57,
            Key.X => 0x58,
            Key.Y => 0x59,
            Key.Z => 0x5A,
            Key.Num0 => 0x30,
            Key.Num1 => 0x31,
            Key.Num2 => 0x32,
            Key.Num3 => 0x33,
            Key.Num4 => 0x34,
            Key.Num5 => 0x35,
            Key.Num6 => 0x36,
            Key.Num7 => 0x37,
            Key.Num8 => 0x38,
            Key.Num9 => 0x39,
            Key.F1 => 0x70,
            Key.F2 => 0x71,
            Key.F3 => 0x72,
            Key.F4 => 0x73,
            Key.F5 => 0x74,
            Key.F6 => 0x75,
            Key.F7 => 0x76,
            Key.F8 => 0x77,
            Key.F9 => 0x78,
            Key.F10 => 0x79,
            Key.F11 => 0x7A,
            Key.F12 => 0x7B,
            Key.F13 => 0x7C,
            Key.F14 => 0x7D,
            Key.F15 => 0x7E,
            Key.F16 => 0x7F,
            Key.F17 => 0x80,
            Key.F18 => 0x81,
            Key.F19 => 0x82,
            Key.F20 => 0x83,
            Key.F21 => 0x84,
            Key.F22 => 0x85,
            Key.F23 => 0x86,
            Key.F24 => 0x87,
            Key.LeftShift => 0xA0,
            Key.RightShift => 0xA1,
            Key.LeftCommandOrControl => 0xA2,
            Key.RightCommandOrControl => 0xA3,
            Key.LeftAltOrOption => 0xA4,
            Key.RightAltOrOption => 0xA5,
            Key.Tab => 0x09,
            Key.Windows => 0x5B,
            Key.Space => 0x20,
            Key.LeftArrow => 0x25,
            Key.RightArrow => 0x27,
            Key.UpArrow => 0x26,
            Key.DownArrow => 0x28,
            Key.Home => 0x24,
            Key.End => 0x23,
            Key.PageUp => 0x21,
            Key.PageDown => 0x22,
            Key.Enter => 0x0D,
            Key.Backspace => 0x08,
            Key.Insert => 0x2D,
            Key.Delete => 0x2E,
            Key.Escape => 0x1B,
            Key.PrintScreen => 0x2C,
            Key.Minus => 0x6C,
            Key.Asterisk => 0x6A,
            Key.Plus => 0x6B,
            Key.Comma => 0xBC,
            Key.Dot => 0xBE,
            Key.Slash => 0x6F,
            Key.CapsLock => 0x14,
            Key.NumLock => 0x90,
            Key.ScrollLock => 0x91,
            _ => 0
        };
    }
}
