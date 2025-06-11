using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextViewer
{
    public class TextScroller
    {
        public int Height = 20;
        public int scrollPosition = 0;
        public int visibleBottomLines = 5;
        int currentLineNumber = 0;

        public int lineCount = 0;

        StringBuilder currentLineBuilder = new();

        public int changeScroll(int change)
        {
            scrollPosition += change;
            if (scrollPosition >= lineCount - visibleBottomLines)
            {
                scrollPosition = lineCount - visibleBottomLines;
            }
            if (scrollPosition < 0)
            {
                scrollPosition = 0;
            }
            return scrollPosition;
        }

        public void Reset()
        {
            currentLineNumber = 0;
            currentLineBuilder.Clear();
        }

        public void Write(string text, bool endLine)
        {
            if (currentLineNumber + scrollPosition < lineCount)
            {
                Console.Write(text);
                if (endLine)
                {
                    Console.WriteLine();
                    currentLineBuilder.Clear();
                    currentLineNumber++;
                }
                Debug.WriteLine($"    Outputting line, currentLineNumber {currentLineNumber}, scroll:{scrollPosition}, lineCount{lineCount}");
            }
            else
            {
                Debug.WriteLine($"Not outputting line, currentLineNumber {currentLineNumber}, scroll:{scrollPosition}, lineCount{lineCount}");
            }

        }

        public void WriteLineFromBuilder()
        {

            currentLineNumber++;
        }

        public void AddWordsToBuilder(string text)
        {
            currentLineBuilder.Append(text);
        }

        public void AddLineToBuilder(string text)
        {
            currentLineBuilder.AppendLine(text);
        }

        public void SetColorInBuilder(ConsoleColor color, bool background = false)
        {
            string colorCode = "";

            

            currentLineBuilder.Append(colorCode);
        }

        public string GetTerminalSequenceForeground(ConsoleColor color)
        {
            // https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
            // ESC [ <n> m	SGR	Set Graphics Rendition	Set the format of the screen and text as specified by <n>
            switch (color)
            {
                case ConsoleColor.Black:
                    return "\x1b[30m";
                case ConsoleColor.Red:
                    return "\x1b[31m";

                case ConsoleColor.Green:
                    return "\x1b[31m";
                case ConsoleColor.Yellow:
                    return "\x1b[31m";
                case ConsoleColor.Blue:
                    return "\x1b[31m";
                case ConsoleColor.Magenta:
                    return "\x1b[31m";
                case ConsoleColor.Cyan:
                    return "\x1b[31m";
                case ConsoleColor.White:
                    return "\x1b[31m";


                default:
                    return "";
            }
            
        }
    }
}
