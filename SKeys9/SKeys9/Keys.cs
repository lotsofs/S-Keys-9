using System.Collections.Generic;

namespace SKeys9 {
	public static class Keys {
		public static Dictionary<int, string> KeyNames = new Dictionary<int, string> {
			//0
			{ 0x1, "LMB" },
			{ 0x2, "RMB" }, 
			//3
			{ 0x4, "MMB" },
			{ 0x5, "XMB1" }, 
			{ 0x6, "XMB2" },
			//7
			{ 0x8, "Backspace" }, 
			{ 0x9, "Tab" },
			//A
			//B
			{ 0xC, "Clear" },
			{ 0xD, "Enter" },
			//E
			//F

			{ 0x10, "Shift" },
			{ 0x11, "Ctrl" },
			{ 0x12, "Alt" }, 
			{ 0x13, "Pause" }, 
			{ 0x14, "CapsLk" },
			{ 0x15, "Kana/HangulMode" },
			//16
			{ 0x17, "JunjaMode" },
			{ 0x18, "FinalMode" },
			{ 0x19, "Hanja/KanjiMode" },
			//1A
			{ 0x1B, "Esc" },
			{ 0x1C, "Convert" },
			{ 0x1D, "NonConvert" },
			{ 0x1E, "Accept" },
			{ 0x1F, "ModeChange" }, 

			{ 0x20, "Space" }, 
			{ 0x21, "PgUp" }, 
			{ 0x22, "PgDn" }, 
			{ 0x23, "End" }, 
			{ 0x24, "Home" },
			{ 0x25, "Left" },
			{ 0x26, "Up" }, 
			{ 0x27, "Right" },
			{ 0x28, "Down" },
			{ 0x29, "Select" },
			{ 0x2A, "Print" },
			{ 0x2B, "Execute" },
			{ 0x2C, "PrtScr" }, 
			{ 0x2D, "Ins" }, 
			{ 0x2E, "Del" },
			{ 0x2F, "Help" },

			{ 0x30, "0" },
			{ 0x31, "1" }, 
			{ 0x32, "2" }, 
			{ 0x33, "3" },
			{ 0x34, "4" },
			{ 0x35, "5" }, 
			{ 0x36, "6" }, 
			{ 0x37, "7" },
			{ 0x38, "8" }, 
			{ 0x39, "9" }, 
			//3A
			//3B
			//3C
			//3D
			//3E
			//3F

			//40
			{ 0x41, "A" },
			{ 0x42, "B" }, 
			{ 0x43, "C" }, 
			{ 0x44, "D" }, 
			{ 0x45, "E" }, 
			{ 0x46, "F" },
			{ 0x47, "G" }, 
			{ 0x48, "H" }, 
			{ 0x49, "I" }, 
			{ 0x4A, "J" }, 
			{ 0x4B, "K" }, 
			{ 0x4C, "L" }, 
			{ 0x4D, "M" }, 
			{ 0x4E, "N" },
			{ 0x4F, "O" }, 

			{ 0x50, "P" },
			{ 0x51, "Q" }, 
			{ 0x52, "R" }, 
			{ 0x53, "S" }, 
			{ 0x54, "T" }, 
			{ 0x55, "U" }, 
			{ 0x56, "V" },
			{ 0x57, "W" },
			{ 0x58, "X" },
			{ 0x59, "Y" }, 
			{ 0x5A, "Z" }, 
			{ 0x5B, "LWin" }, 
			{ 0x5C, "RWin" },
			{ 0x5D, "Apps" },
			//5E
			{ 0x5F, "Sleep" },

			{ 0x60, "Num0" }, 
			{ 0x61, "Num1" }, 
			{ 0x62, "Num2" }, 
			{ 0x63, "Num3" }, 
			{ 0x64, "Num4" }, 
			{ 0x65, "Num5" }, 
			{ 0x66, "Num6" }, 
			{ 0x67, "Num7" }, 
			{ 0x68, "Num8" }, 
			{ 0x69, "Num9" },
			{ 0x6A, "Num*" }, 
			{ 0x6B, "Num+" },
			{ 0x6C, "Separator" },
			{ 0x6D, "Num-" }, 
			{ 0x6E, "Num." },
			{ 0x6F, "Num/" }, 

			{ 0X70, "F1" }, 
			{ 0X71, "F2" }, 
			{ 0X72, "F3" }, 
			{ 0X73, "F4" }, 
			{ 0X74, "F5" }, 
			{ 0X75, "F6" }, 
			{ 0X76, "F7" }, 
			{ 0X77, "F8" }, 
			{ 0X78, "F9" }, 
			{ 0X79, "F10" },
			{ 0X7A, "F11" },
			{ 0X7B, "F12" },
			{ 0X7C, "F13" },
			{ 0X7D, "F14" },
			{ 0X7E, "F15" },
			{ 0X7F, "F16" },

			{ 0X80, "F17" },
			{ 0X81, "F18" },
			{ 0X82, "F19" },
			{ 0X83, "F20" },
			{ 0X84, "F21" }, 
			{ 0X85, "F22" }, 
			{ 0X86, "F23" }, 
			{ 0X87, "F24" }, 
			//88
			//89
			//8A
			//8B
			//8C
			//8D
			//8E
			//8F

			{ 0x90, "NumLk" },
			{ 0x91, "ScrLk" },
			//92
			//93
			//94
			//95
			//96
			{ 0x97, "ScrUp" }, // Hijacked
			{ 0x98, "ScrDn" }, // Hijacked
			{ 0x99, "ScrLeft" }, // Hijacked
			{ 0x9A, "ScrRight" }, // Hijacked
			{ 0x9B, "MouseMovement" }, // Hijacked
			{ 0x9C, "MouseMovOverflow" }, // Hijacked
			{ 0x9D, "DbgHookTimeout" }, // Hijacked
			//9E
			//9F

			{ 0xA0, "LShift" },
			{ 0xA1, "RShift" },
			{ 0xA2, "LCtrl" }, 
			{ 0xA3, "RCtrl" }, 
			{ 0xA4, "LAlt" }, 
			{ 0xA5, "RAlt" }, 
			{ 0xA6, "BrowserBack" }, 
			{ 0xA7, "BrowserFwd" }, 
			{ 0xA8, "BrowserRefresh" }, 
			{ 0xA9, "BrowserStop" }, 
			{ 0xAA, "BrowserSearch" }, 
			{ 0xAB, "BrowserFavorites" }, 
			{ 0xAC, "BrowserHome" }, 
			{ 0xAD, "VolumeMute" }, 
			{ 0xAE, "VolumeDown" }, 
			{ 0xAF, "VolumeUp" },

			{ 0xB0, "MediaNextTrack" },
			{ 0xB1, "MediaPrevTrack" },
			{ 0xB2, "MediaStop" },
			{ 0xB3, "MediaPlayPause" },
			{ 0xB4, "Mail" },
			{ 0xB5, "SelectMedia" },
			{ 0xB6, "LaunchApplication1" },
			{ 0xB7, "LaunchApplication2" },
			//B8
			//B9
			{ 0xBA, ";" },
			{ 0xBB, "=" },
			{ 0xBC, "," },
			{ 0xBD, "-" },
			{ 0xBE, "." },
			{ 0xBF, "/" },

			{ 0xC0, "~" },
			//C1
			//C2
			//C3
			//C4
			//C5
			//C6
			//C7
			//C8
			//C9
			//CA
			//CB
			//CC
			//CD
			//CE
			//CF

			//D0
			//D1
			//D2
			//D3
			//D4
			//D5
			//D6
			//D7
			//D8
			//D9
			//DA
			{ 0xDB, "[" },
			{ 0xDC, "\\" },
			{ 0xDD, "]" },
			{ 0xDE, "'" },
			//DF

			//E0
			//E1
			{ 0xE2, "OEM102" },
			//E3
			//E4
			{ 0xE5, "ProcessKey" },
			//E6
			{ 0xE7, "Packet" },
			//E8
			//E9
			//EA
			//EB
			//EC
			//ED
			//EE
			//EF

			//F0
			//F1
			//F2
			//F3
			//F4
			//F5
			{ 0xF6, "Attn" },
			{ 0xF7, "CrSel" },
			{ 0xF8, "ExSel" },
			{ 0xF9, "EraseEOF" },
			{ 0xFA, "Play" },
			{ 0xFB, "Zoom" },
			//FC
			{ 0xFD, "PA1" },
			{ 0xFE, "Clear" },
			//FF
		};
	}

	// Todo: Custom keynames support
}
