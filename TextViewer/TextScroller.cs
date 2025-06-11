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
        private int _height = 20;
        public int Height
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
        }// heighte of the lines + scroll overflow arrows is present
        private int _pageHeight = 18;
        public int PageHeight
        {
            get
            {
                return _pageHeight;
            }
        }
        public int scrollPosition = 0;
        public int visibleBottomLines = 5;
        public bool ShowOverflowArrows = true;
        int currentLineNumber = 0;

        public int lineCount = 0;

        StringBuilder currentLineBuilder = new();
        private List<string> Lines = [];

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

        public void ScrollToBeginning()
        {
            scrollPosition = 0;
        }

        public void ScrollToEnd()
        {
            scrollPosition = lineCount - visibleBottomLines;
        }

        public void Reset()
        {
            currentLineNumber = 0;
            currentLineBuilder.Clear();
        }

        //public void Write(string text, bool endLine)
        //{
        //    if (currentLineNumber + scrollPosition < lineCount)
        //    {
        //        Console.Write(text);
        //        if (endLine)
        //        {
        //            Console.WriteLine();
        //            currentLineBuilder.Clear();
        //            currentLineNumber++;
        //        }
        //        Debug.WriteLine($"    Outputting line, currentLineNumber {currentLineNumber}, scroll:{scrollPosition}, lineCount{lineCount}");
        //    }
        //    else
        //    {
        //        Debug.WriteLine($"Not outputting line, currentLineNumber {currentLineNumber}, scroll:{scrollPosition}, lineCount{lineCount}");
        //    }

        //}

        public void WriteLineFromBuilder()
        {
            currentLineNumber++;
        }

        public void ResetLines()
        {
            Lines.Clear();
            lineCount = 0;
        }

        public void AddTextToLine(string text)
        {
            currentLineBuilder.Append(text);
        }

        public void FinishLine(string text, bool addToBeginning = false)
        {
            currentLineBuilder.Append(text);
            if (addToBeginning)
            {
                Lines.Insert(0,currentLineBuilder.ToString());
            }
            else
            {
                Lines.Add(currentLineBuilder.ToString());
            }
            currentLineBuilder.Clear();
            lineCount = Lines.Count;
        }

        public void SetColorInBuilder(ConsoleColor color, bool background = false)
        {
            string colorCode = "";

            

            currentLineBuilder.Append(colorCode);
        }

        public List<string> GetLines()
        {
            return Lines;
        }

        public void OutputLines(List<string>? lines)
        {
            if (lines == null)
            {
                lines = Lines;
            }
            lineCount = lines.Count;

            int usableHeight = Height;
            if (ShowOverflowArrows) usableHeight -= 2;

            if (ShowOverflowArrows && scrollPosition > 0)
            {
                Console.WriteLine(" ⮝ ⮝ ⮝ ⮝ ⮝ ⮝ ");
            }
            else
            {
                Console.WriteLine();
            }

            for (int i = scrollPosition; i < lines.Count && i < scrollPosition + usableHeight; i++)
            {
               Console.WriteLine(Lines[i]);
            }

            if (ShowOverflowArrows && scrollPosition + usableHeight < lines.Count)
            {
                Console.WriteLine(" ⮟ ⮟ ⮟ ⮟ ⮟ ⮟ ");
            }
            else
            {
                Console.WriteLine();
            }
            
        }
    }
}
