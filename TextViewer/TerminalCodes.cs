using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace TextViewer
{
    public static class TerminalCodes
    {
        // all terminal codes start with the escape character 0x1B : \x1b[30m
        public static readonly string Default =              "\x1B[0m";

        public static readonly string Bold =                 "\x1B[1m";
        public static readonly string NoBold =               "\x1B[22m";
        public static readonly string Regular =              "\x1B[22m"; // same as NoBold, for convenience

        public static readonly string Underline =            "\x1B[4m";
        public static readonly string NoUnderline =          "\x1B[24m";

        public static readonly string Negative =             "\x1B[7m";
        public static readonly string Positive =             "\x1B[27m";

        public static readonly string ForegroundBlack =      "\x1B[30m";
        public static readonly string ForegroundRed =        "\x1B[31m";
        public static readonly string ForegroundGreen =      "\x1B[32m";
        public static readonly string ForegroundYellow =     "\x1B[33m";
        public static readonly string ForegroundBlue =       "\x1B[34m";
        public static readonly string ForegroundMagenta =    "\x1B[35m";
        public static readonly string ForegroundCyan =       "\x1B[36m";
        public static readonly string ForegroundWhite =      "\x1B[37m";
        //public static string ForegroundExtended = "\x1B[38m";
        public static readonly string ForegroundDefault =    "\x1B[39m";
        public static readonly string BackgroundBlack =      "\x1B[40m";
        public static readonly string BackgroundRed =        "\x1B[41m";
        public static readonly string BackgroundGreen =      "\x1B[42m";
        public static readonly string BackgroundYellow =     "\x1B[43m";
        public static readonly string BackgroundBlue =       "\x1B[44m";
        public static readonly string BackgroundMagenta =    "\x1B[45m";
        public static readonly string BackgroundCyan =       "\x1B[46m";
        public static readonly string BackgroundWhite =      "\x1B[47m";
        //public static string BackgroundExtended = "\x1B[48m";
        public static readonly string BackgroundDefault =    "\x1B[49m";

        public static string RGBtoForeground(int r, int g, int b)
        {
            return $"\x1B[38;2;{r};{g};{b}m";
        }

        public static string RGBtoForeground(Color color)
        {
            return RGBtoForeground(color.R, color.G, color.B);
        }

        public static string RGBtoBackground(int r, int g, int b)
        {
            return $"\x1B[48;2;{r};{g};{b}m";
        }

        public static string RGBtoBackground(Color color)
        {
            return RGBtoBackground(color.R, color.G, color.B);
        }


        /*
        0	Default Returns all attributes to the default state prior to modification
        1	Bold/Bright Applies brightness/intensity flag to foreground color
        22	No bold/bright Removes brightness/intensity flag from foreground color
        4	Underline Adds underline
        24	No underline    Removes underline
        7	Negative Swaps foreground and background colors
        27	Positive(No negative)  Returns foreground/background to normal
        30	Foreground Black    Applies non-bold/bright black to foreground
        31	Foreground Red      Applies non-bold/bright red to foreground
        32	Foreground Green    Applies non-bold/bright green to foreground
        33	Foreground Yellow   Applies non-bold/bright yellow to foreground
        34	Foreground Blue     Applies non-bold/bright blue to foreground
        35	Foreground Magenta  Applies non-bold/bright magenta to foreground
        36	Foreground Cyan     Applies non-bold/bright cyan to foreground
        37	Foreground White    Applies non-bold/bright white to foreground
        38	Foreground Extended Applies extended color value to the foreground(see details below)
        39	Foreground Default  Applies only the foreground portion of the defaults(see 0)
        40	Background Black    Applies non-bold/bright black to background
        41	Background Red      Applies non-bold/bright red to background
        42	Background Green    Applies non-bold/bright green to background
        43	Background Yellow   Applies non-bold/bright yellow to background
        44	Background Blue     Applies non-bold/bright blue to background
        45	Background Magenta  Applies non-bold/bright magenta to background
        46	Background Cyan     Applies non-bold/bright cyan to background
        47	Background White    Applies non-bold/bright white to background
        48	Background Extended Applies extended color value to the background(see details below)
        49	Background Default  Applies only the background portion of the defaults(see 0)
        90	Bright Foreground Black Applies bold/bright black to foreground
        91	Bright Foreground Red Applies bold/bright red to foreground
        92	Bright Foreground Green Applies bold/bright green to foreground
        93	Bright Foreground Yellow Applies bold/bright yellow to foreground
        94	Bright Foreground Blue Applies bold/bright blue to foreground
        95	Bright Foreground Magenta Applies bold/bright magenta to foreground
        96	Bright Foreground Cyan Applies bold/bright cyan to foreground
        97	Bright Foreground White Applies bold/bright white to foreground
        100	Bright Background Black Applies bold/bright black to background
        101	Bright Background Red Applies bold/bright red to background
        102	Bright Background Green Applies bold/bright green to background
        103	Bright Background Yellow Applies bold/bright yellow to background
        104	Bright Background Blue Applies bold/bright blue to background
        105	Bright Background Magenta Applies bold/bright magenta to background
        106	Bright Background Cyan Applies bold/bright cyan to background
        107	Bright Background White Applies bold/bright white to background
        */
    }




    /* CODES
    From: https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
    Check full text there for explanations
     
    Simple Cursor Positioning
        Sequence	Shorthand	Behavior
        ESC M	RI	Reverse Index – Performs the reverse operation of \n, moves cursor up one line, maintains horizontal position, scrolls buffer if necessary*
        ESC 7	DECSC	Save Cursor Position in Memory**
        ESC 8	DECSR	Restore Cursor Position from Memory** 
    
    Cursor Positioning
        Sequence	Code	Description	Behavior
        ESC [ <n> A	CUU	Cursor Up	Cursor up by <n>
        ESC [ <n> B	CUD	Cursor Down	Cursor down by <n>
        ESC [ <n> C	CUF	Cursor Forward	Cursor forward (Right) by <n>
        ESC [ <n> D	CUB	Cursor Backward	Cursor backward (Left) by <n>
        ESC [ <n> E	CNL	Cursor Next Line	Cursor down <n> lines from current position
        ESC [ <n> F	CPL	Cursor Previous Line	Cursor up <n> lines from current position
        ESC [ <n> G	CHA	Cursor Horizontal Absolute	Cursor moves to <n>th position horizontally in the current line
        ESC [ <n> d	VPA	Vertical Line Position Absolute	Cursor moves to the <n>th position vertically in the current column
        ESC [ <y> ; <x> H	CUP	Cursor Position	*Cursor moves to <x>; <y> coordinate within the viewport, where <x> is the column of the <y> line
        ESC [ <y> ; <x> f	HVP	Horizontal Vertical Position	*Cursor moves to <x>; <y> coordinate within the viewport, where <x> is the column of the <y> line
        ESC [ s	ANSISYSSC	Save Cursor – Ansi.sys emulation	**With no parameters, performs a save cursor operation like DECSC
        ESC [ u	ANSISYSRC	Restore Cursor – Ansi.sys emulation	**With no parameters, performs a restore cursor operation like DECRC

    Cursor Visibility
        Sequence	Code	Description	Behavior
        ESC [ ? 12 h	ATT160	Text Cursor Enable Blinking	Start the cursor blinking
        ESC [ ? 12 l	ATT160	Text Cursor Disable Blinking	Stop blinking the cursor
        ESC [ ? 25 h	DECTCEM	Text Cursor Enable Mode Show	Show the cursor
        ESC [ ? 25 l	DECTCEM	Text Cursor Enable Mode Hide	Hide the cursor

    Cursor Shape
        Sequence	Code	Description	Behavior
        ESC [ 0 SP q	DECSCUSR	User Shape	Default cursor shape configured by the user
        ESC [ 1 SP q	DECSCUSR	Blinking Block	Blinking block cursor shape
        ESC [ 2 SP q	DECSCUSR	Steady Block	Steady block cursor shape
        ESC [ 3 SP q	DECSCUSR	Blinking Underline	Blinking underline cursor shape
        ESC [ 4 SP q	DECSCUSR	Steady Underline	Steady underline cursor shape
        ESC [ 5 SP q	DECSCUSR	Blinking Bar	Blinking bar cursor shape
        ESC [ 6 SP q	DECSCUSR	Steady Bar	Steady bar cursor shape

    Viewport Positioning
        Sequence	Code	Description	Behavior
        ESC [ <n> S	SU	Scroll Up	Scroll text up by <n>. Also known as pan down, new lines fill in from the bottom of the screen
        ESC [ <n> T	SD	Scroll Down	Scroll down by <n>. Also known as pan up, new lines fill in from the top of the screen

    Text Modification
        Sequence	Code	Description	Behavior
        ESC [ <n> @	ICH	Insert Character	Insert <n> spaces at the current cursor position, shifting all existing text to the right. Text exiting the screen to the right is removed.
        ESC [ <n> P	DCH	Delete Character	Delete <n> characters at the current cursor position, shifting in space characters from the right edge of the screen.
        ESC [ <n> X	ECH	Erase Character	Erase <n> characters from the current cursor position by overwriting them with a space character.
        ESC [ <n> L	IL	Insert Line	Inserts <n> lines into the buffer at the cursor position. The line the cursor is on, and lines below it, will be shifted downwards.
        ESC [ <n> M	DL	Delete Line	Deletes <n> lines from the buffer, starting with the row the cursor is on.

        Sequence	Code	Description	Behavior
        ESC [ <n> J	ED	Erase in Display	Replace all text in the current viewport/screen specified by <n> with space characters
        ESC [ <n> K	EL	Erase in Line	Replace all text on the line with the cursor specified by <n> with space characters

    Text Formatting
        Value	Description	Behavior
        0	Default	Returns all attributes to the default state prior to modification
        1	Bold/Bright	Applies brightness/intensity flag to foreground color
        22	No bold/bright	Removes brightness/intensity flag from foreground color
        4	Underline	Adds underline
        24	No underline	Removes underline
        7	Negative	Swaps foreground and background colors
        27	Positive (No negative)	Returns foreground/background to normal
        30	Foreground Black	Applies non-bold/bright black to foreground
        31	Foreground Red	Applies non-bold/bright red to foreground
        32	Foreground Green	Applies non-bold/bright green to foreground
        33	Foreground Yellow	Applies non-bold/bright yellow to foreground
        34	Foreground Blue	Applies non-bold/bright blue to foreground
        35	Foreground Magenta	Applies non-bold/bright magenta to foreground
        36	Foreground Cyan	Applies non-bold/bright cyan to foreground
        37	Foreground White	Applies non-bold/bright white to foreground
        38	Foreground Extended	Applies extended color value to the foreground (see details below)
        39	Foreground Default	Applies only the foreground portion of the defaults (see 0)
        40	Background Black	Applies non-bold/bright black to background
        41	Background Red	Applies non-bold/bright red to background
        42	Background Green	Applies non-bold/bright green to background
        43	Background Yellow	Applies non-bold/bright yellow to background
        44	Background Blue	Applies non-bold/bright blue to background
        45	Background Magenta	Applies non-bold/bright magenta to background
        46	Background Cyan	Applies non-bold/bright cyan to background
        47	Background White	Applies non-bold/bright white to background
        48	Background Extended	Applies extended color value to the background (see details below)
        49	Background Default	Applies only the background portion of the defaults (see 0)
        90	Bright Foreground Black	Applies bold/bright black to foreground
        91	Bright Foreground Red	Applies bold/bright red to foreground
        92	Bright Foreground Green	Applies bold/bright green to foreground
        93	Bright Foreground Yellow	Applies bold/bright yellow to foreground
        94	Bright Foreground Blue	Applies bold/bright blue to foreground
        95	Bright Foreground Magenta	Applies bold/bright magenta to foreground
        96	Bright Foreground Cyan	Applies bold/bright cyan to foreground
        97	Bright Foreground White	Applies bold/bright white to foreground
        100	Bright Background Black	Applies bold/bright black to background
        101	Bright Background Red	Applies bold/bright red to background
        102	Bright Background Green	Applies bold/bright green to background
        103	Bright Background Yellow	Applies bold/bright yellow to background
        104	Bright Background Blue	Applies bold/bright blue to background
        105	Bright Background Magenta	Applies bold/bright magenta to background
        106	Bright Background Cyan	Applies bold/bright cyan to background
        107	Bright Background White	Applies bold/bright white to background

    Extended Colors
        Some virtual terminal emulators support a palette of colors greater than the 16 colors provided by the Windows Console.
        For these extended colors, the Windows Console will choose the nearest appropriate color from the existing 16 color table for display.
        Unlike typical SGR values above, the extended values will consume additional parameters after the initial indicator according to the table below.

        SGR Subsequence	Description
        38 ; 2 ; <r> ; <g> ; <b>	Set foreground color to RGB value specified in <r>, <g>, <b> parameters*
        48 ; 2 ; <r> ; <g> ; <b>	Set background color to RGB value specified in <r>, <g>, <b> parameters*
        38 ; 5 ; <s>	Set foreground color to <s> index in 88 or 256 color table*
        48 ; 5 ; <s>	Set background color to <s> index in 88 or 256 color table*

        *The 88 and 256 color palettes maintained internally for comparison are based from the xterm terminal emulator. The comparison/rounding tables cannot be modified at this time.

    Screen Colors
        Sequence	Description	Behavior
        ESC ] 4 ; <i> ; rgb : <r> / <g> / <b> <ST>	Modify Screen Colors	Sets the screen color palette index <i> to the RGB values specified in <r>, <g>, <b>

    Mode Changes
        Sequence	Code	Description	Behavior
        ESC =	DECKPAM	Enable Keypad Application Mode	Keypad keys will emit their Application Mode sequences.
        ESC >	DECKPNM	Enable Keypad Numeric Mode	Keypad keys will emit their Numeric Mode sequences.
        ESC [ ? 1 h	DECCKM	Enable Cursor Keys Application Mode	Keypad keys will emit their Application Mode sequences.
        ESC [ ? 1 l	DECCKM	Disable Cursor Keys Application Mode (use Normal Mode)	Keypad keys will emit their Numeric Mode sequences.

    Query State
        Sequence	Code	Description	Behavior
        ESC [ 6 n	DECXCPR	Report Cursor Position	Emit the cursor position as: ESC [ <r> ; <c> R Where <r> = cursor row and <c> = cursor column
        ESC [ 0 c	DA	Device Attributes	Report the terminal identity. Will emit “\x1b[?1;0c”, indicating "VT101 with No Options".

    Tabs
        Sequence	Code	Description	Behavior
        ESC H	HTS	Horizontal Tab Set	Sets a tab stop in the current column the cursor is in.
        ESC [ <n> I	CHT	Cursor Horizontal (Forward) Tab	Advance the cursor to the next column (in the same row) with a tab stop. If there are no more tab stops, move to the last column in the row. If the cursor is in the last column, move to the first column of the next row.
        ESC [ <n> Z	CBT	Cursor Backwards Tab	Move the cursor to the previous column (in the same row) with a tab stop. If there are no more tab stops, moves the cursor to the first column. If the cursor is in the first column, doesn’t move the cursor.
        ESC [ 0 g	TBC	Tab Clear (current column)	Clears the tab stop in the current column, if there is one. Otherwise does nothing.
        ESC [ 3 g	TBC	Tab Clear (all columns)	Clears all currently set

    Designate Character Set
        Sequence	Description	Behavior
        ESC ( 0	Designate Character Set – DEC Line Drawing	Enables DEC Line Drawing Mode
        ESC ( B	Designate Character Set – US ASCII	Enables ASCII Mode (Default)

        Hex	ASCII	DEC Line Drawing
        0x6a	j	┘
        0x6b	k	┐
        0x6c	l	┌
        0x6d	m	└
        0x6e	n	┼
        0x71	q	─
        0x74	t	├
        0x75	u	┤
        0x76	v	┴
        0x77	w	┬
        0x78	x	│

    Scrolling Margins
        Sequence	Code	Description	Behavior
        ESC [ <t> ; <b> r	DECSTBM	Set Scrolling Region	Sets the VT scrolling margins of the viewport.

    Window Title
        Sequence	Description	Behavior
        ESC ] 0 ; <string> <ST>	Set Window Title	Sets the console window’s title to <string>.
        ESC ] 2 ; <string> <ST>	Set Window Title	Sets the console window’s title to <string>.

    Alternate Screen Buffer
        Sequence	Description	Behavior
        ESC [ ? 1 0 4 9 h	Use Alternate Screen Buffer	Switches to a new alternate screen buffer.
        ESC [ ? 1 0 4 9 l	Use Main Screen Buffer	Switches to the main buffer.

    Window Width
        Sequence	Code	Description	Behavior
        ESC [ ? 3 h	DECCOLM	Set Number of Columns to 132	Sets the console width to 132 columns wide.
        ESC [ ? 3 l	DECCOLM	Set Number of Columns to 80	Sets the console width to 80 columns wide.

    Soft Reset
        Sequence	Code	Description	Behavior
        ESC [ ! p	DECSTR	Soft Reset	Reset certain terminal settings to their defaults.

    Cursor Keys
        Key	        Normal Mode	    Application Mode
        Up Arrow	ESC [ A	        ESC O A
        Down Arrow	ESC [ B	        ESC O B
        Right Arrow	ESC [ C	        ESC O C
        Left Arrow	ESC [ D	        ESC O D
        Home	    ESC [ H	        ESC O H
        End	        ESC [ F	        ESC O F

        Key	                Any Mode
        Ctrl + Up Arrow	    ESC [ 1 ; 5 A
        Ctrl + Down Arrow	ESC [ 1 ; 5 B
        Ctrl + Right Arrow	ESC [ 1 ; 5 C
        Ctrl + Left Arrow	ESC [ 1 ; 5 D

    
    Numpad & Function Keys
        Key	Sequence
        Backspace	0x7f (DEL)
        Pause	0x1a (SUB)
        Escape	0x1b (ESC)
        Insert	ESC [ 2 ~
        Delete	ESC [ 3 ~
        Page Up	ESC [ 5 ~
        Page Down	ESC [ 6 ~
        F1	ESC O P
        F2	ESC O Q
        F3	ESC O R
        F4	ESC O S
        F5	ESC [ 1 5 ~
        F6	ESC [ 1 7 ~
        F7	ESC [ 1 8 ~
        F8	ESC [ 1 9 ~
        F9	ESC [ 2 0 ~
        F10	ESC [ 2 1 ~
        F11	ESC [ 2 3 ~
        F12	ESC [ 2 4 ~

    Modifiers
        Key	Sequence
        Ctrl + Space	0x00 (NUL)
        Ctrl + Up Arrow	ESC [ 1 ; 5 A
        Ctrl + Down Arrow	ESC [ 1 ; 5 B
        Ctrl + Right Arrow	ESC [ 1 ; 5 C
        Ctrl + Left Arrow	ESC [ 1 ; 5 D
    */
}
