using System.Drawing;
using System.Text;

namespace TextViewer
{
    public class TextScroller
    {
        private int _height = 20;
        public int Height // height of the lines + scroll overflow arrows is present
        {
            get
            {
                return _height;
            }
            set
            {
                if (ShowOverflowArrows)
                {
                    _pageHeight = value - 2;
                }
                else
                {
                    _pageHeight = value;
                }
                _height = value;
            }
        }
        private int _pageHeight = 18;
        public int PageHeight // height of the lines inside the scroll area (height - overflow scroll arrows)
        {
            get
            {
                return _pageHeight;
            }
        }
        public int scrollPosition = 0;
        public int visibleBottomLines = 5;
        public bool ShowOverflowArrows = true;
        public int HiglightLine = -1;
        public string HighlightSymbol = "🢂";
        public Color? HiglightColor = null;
        //int currentLineNumber = 0;

        public int LineCount
        {
            get
            {
                return Lines.Count;
            }
        }

        private readonly StringBuilder currentLineBuilder = new();

        public List<string> Lines = [];

        public int ChangeScroll(int change)
        {
            scrollPosition += change;
            if (scrollPosition >= LineCount - visibleBottomLines)
            {
                scrollPosition = LineCount - visibleBottomLines;
            }
            if (scrollPosition < 0)
            {
                scrollPosition = 0;
            }
            return scrollPosition;
        }

        public void ScrollToBeginning()
        {
            scrollPosition = 0;
        }

        public void ScrollToEnd()
        {
            scrollPosition = LineCount - visibleBottomLines;
        }

        public void ResetLineBuilder()
        {
            currentLineBuilder.Clear();
        }

        public void ResetLines(bool resetScroll = false)
        {
            Lines.Clear();
            if (resetScroll)
            {
                scrollPosition = 0;
            }
        }

        public void AddTextToLine(string text)
        {
            currentLineBuilder.Append(text);
        }

        public void FinishLine(string text, bool addToStartOfLines = false)
        {
            currentLineBuilder.Append(text);
            if (addToStartOfLines)
            {
                Lines.Insert(0, currentLineBuilder.ToString());
            }
            else
            {
                Lines.Add(currentLineBuilder.ToString());
            }
            currentLineBuilder.Clear();
        }

        public string GetLineInProgress()
        {
            return currentLineBuilder.ToString();
        }

        public void SetColor(Color color, bool background = false)
        {
            string colorCode;
            if (background)
            {
                colorCode = TerminalCodes.RGBtoBackground(color);
            }
            else
            {
                colorCode = TerminalCodes.RGBtoForeground(color);
            }
            currentLineBuilder.Append(colorCode);
        }

        public List<string> GetLines()
        {
            return Lines;
        }

        public void OutputLines(List<string>? lines)
        {
            lines ??= Lines; // if lines is null, use the already created list of lines

            if (ShowOverflowArrows && scrollPosition > 0)
            {
                Console.WriteLine(" ⮝ ⮝ ⮝ ⮝ ⮝ ⮝ ");
            }
            else
            {
                Console.WriteLine();
            }

            for (int i = scrollPosition; i < lines.Count && i < scrollPosition + PageHeight; i++)
            {
                string line = lines[i];

                if (HiglightLine == i)
                {
                    ConsoleColor foreground = Console.ForegroundColor;
                    if (HiglightColor != null)
                    {
                        Console.Write(TerminalCodes.RGBtoForeground((Color)HiglightColor));
                    }
                    Console.Write(line); // cut off first chars for highlight symbol
                    if (HiglightColor != null)
                    {
                        Console.ForegroundColor = foreground;
                    }
                    if (HighlightSymbol != "")
                    {
                        Console.SetCursorPosition(0, Console.GetCursorPosition().Top);
                        Console.Write(HighlightSymbol);
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine(line);
                }
            }

            if (ShowOverflowArrows && scrollPosition + PageHeight < lines.Count)
            {
                Console.WriteLine(" ⮟ ⮟ ⮟ ⮟ ⮟ ⮟ ");
            }
            else
            {
                Console.WriteLine();
            }

        }

        internal bool HighlightIsAtEndOfPage()
        {

            bool atEnd = HiglightLine >= scrollPosition + PageHeight - 1;
            return atEnd;
        }

        internal int HighlightBeyondPageCount()
        {
            return (scrollPosition + PageHeight - 2) - HiglightLine;
        }

        internal bool HighlightIsAtStartOfPage()
        {

            bool atStart = HiglightLine <= scrollPosition;
            return atStart;
        }
    }
}
