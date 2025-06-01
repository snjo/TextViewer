using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TextViewer.TextViewerLoop;

namespace TextViewer
{
    internal class FileView (TextViewerLoop parent)
    {
        internal string[] lines = [];
        private int scrollLine = 0;
        public bool forceFileUpdate = false;
        public int fileUpdateCountdown = -1;
        internal DateTime lastFileUpdate = DateTime.MinValue;

        public void HandleFileViewKeys(ConsoleKeyInfo keyInput)
        {
            int linemax = Math.Max(lines.Length - 1, 0); // prevent error on 0 size file

            if (keyInput.Key == ConsoleKey.PageDown || (keyInput.Key == ConsoleKey.DownArrow && keyInput.Modifiers == ConsoleModifiers.Shift))
            {
                scrollLine += 10;
                scrollLine = Math.Clamp(scrollLine, 0, linemax);
            }
            else if (keyInput.Key == ConsoleKey.PageUp || (keyInput.Key == ConsoleKey.UpArrow && keyInput.Modifiers == ConsoleModifiers.Shift))
            {
                scrollLine -= 10;
                scrollLine = Math.Clamp(scrollLine, 0, linemax);
            }
            else if (keyInput.Key == ConsoleKey.DownArrow)
            {
                scrollLine++;
                scrollLine = Math.Clamp(scrollLine, 0, linemax);
            }
            else if (keyInput.Key == ConsoleKey.UpArrow)
            {
                scrollLine--;
                scrollLine = Math.Clamp(scrollLine, 0, linemax);
            }
            else if (keyInput.Key == ConsoleKey.F5)
            {
                forceFileUpdate = true;
            }
            else if (keyInput.Key == ConsoleKey.L && parent.interfaceMode == TextViewerLoop.InterfaceMode.FileView)
            {
                Cosmetic.SetColor(ConsoleColor.Yellow);
                Console.SetCursorPosition(10, 5);
                Console.WriteLine("┌─────────────────┐");
                Console.SetCursorPosition(10, 6);
                Console.WriteLine("│ Go go line:     │");
                Console.SetCursorPosition(10, 7);
                Console.WriteLine("└─────────────────┘");
                Console.SetCursorPosition(24, 6);
                string lineSelect = Console.ReadLine() + "";
                if (int.TryParse(lineSelect, out int gotoLine))
                {
                    gotoLine--; // internal line 0, show as line 1
                    int max = Math.Max(0, linemax);
                    scrollLine = Math.Clamp(gotoLine, 0, max);

                    Debug.WriteLine($"line selected: {gotoLine}, scroll set to: {scrollLine}. Lines length: {lines.Length}, max: {max}");
                }
            }
            else if (keyInput.Key == ConsoleKey.Backspace)
            {
                parent.interfaceMode = InterfaceMode.DirectoryView;
                parent.SetupWatcher(parent.monitorDirectory, null);
            }
        }

        internal void UpdateFileView()
        {
            Console.Clear();

            ConsoleColor TitleFG = ConsoleColor.Cyan;
            ConsoleColor TitleBG = ConsoleColor.Black;
            ConsoleColor TextFG = ConsoleColor.White;
            ConsoleColor TextBG = ConsoleColor.Black;
            Console.SetCursorPosition(0, 0);
            Cosmetic.SetColor(TitleFG, TitleBG);
            string displayFileName = parent.monitorTextFile + "";
            if (displayFileName.Length > Console.BufferWidth - 41)
            {
                displayFileName = Path.GetFileName(parent.monitorTextFile) + "";
            }

            string appFileLine = $"┃ Text Viewer ┃ '{displayFileName}' ";
            string dateLine = $" {lastFileUpdate.ToShortDateString()} {lastFileUpdate.ToShortTimeString()} ";

            Console.Write(appFileLine[..Math.Min(appFileLine.Length, Console.BufferWidth - dateLine.Length)].PadRight(Console.BufferWidth - dateLine.Length - 2, ' '));
            Console.Write("┃");
            Cosmetic.SetColorFromAge(lastFileUpdate);

            Console.Write(dateLine);
            Cosmetic.SetColor(TitleFG);
            Console.WriteLine("┃");

            Console.WriteLine($"┣━━━━━━┳━━━━━━┻".PadRight(Console.BufferWidth - 20, '━') + "┻━━━━━━━━━━━━━━━━━━┫");
            //Console.SetCursorPosition(0, 1);
            //SetColor(ConsoleColor.DarkCyan, ConsoleColor.Black);
            //Console.WriteLine($"Time: {DateTime.Now.ToShortTimeString()}");
            int extraLines = 0;
            for (int i = scrollLine; i < lines.Length && i < Console.BufferHeight + scrollLine - 6 - extraLines; i++)
            {
                string line = lines[i];
                line = line.Replace("\t", "   ");

                var lineSplit = SplitByLength(line, Console.BufferWidth - 15).ToList();
                if (lineSplit.Count == 0)
                {
                    lineSplit.Add("");
                }
                //SetColor(ConsoleColor.Magenta, ConsoleColor.DarkGray);

                bool first = true;
                foreach (string l in lineSplit)
                {
                    string lineNumber = (i + 1).ToString(); // change line 0 to line 1
                    if (!first)
                    {
                        lineNumber = "  + ";
                        extraLines++;
                        //Debug.WriteLine($"Extra lines {extraLines} at {i}");
                    }
                    else
                    {
                        //Debug.WriteLine($"Regular at {i}");
                    }
                    PrintTextLine(l, lineNumber, TitleFG, TitleBG, TextFG, TextBG);
                    first = false;
                }
            }
            Console.WriteLine($"┗━━━━━━┻".PadRight(Console.BufferWidth - 1, '━') + "┛");
            Console.WriteLine($" [Esc] Menu  [Backspace] Parent Dir.  [L] Goto Line  [Arrows] scroll  [PgUp/PgDn] scroll 10  [F5] Refresh");
        }

        private static void PrintTextLine(string line, string lineNumber, ConsoleColor TitleFG, ConsoleColor TitleBG, ConsoleColor TextFG, ConsoleColor TextBG)
        {
            Console.Write($"┃ {(lineNumber).ToString().PadLeft(4, '0')} ");
            Console.Write("┇");
            Cosmetic.SetColor(TextFG, TextBG);
            Console.Write(line.PadRight(Console.BufferWidth - 9, ' '));
            Cosmetic.SetColor(TitleFG, TitleBG);
            Console.WriteLine("┃");
        }

        private static IEnumerable<string> SplitByLength(string str, int maxLength)
        {
            for (int index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }

        internal bool LoadFile(string? filePath, bool showError, bool initWatcher = true)
        {
            scrollLine = 0;
            if (filePath is not null)
            {
                //if (File.Exists(filePath))
                //{
                try
                {
                    lines = File.ReadAllLines(filePath);
                    parent.updateViewRequested = true;
                    lastFileUpdate = File.GetLastWriteTime(filePath);

                    Debug.WriteLine($"File loaded {filePath}");

                    if (initWatcher)
                    {
                        parent.watcher?.Dispose();
                        Debug.WriteLine("Asking to update Watcher");
                        string? dir = Path.GetDirectoryName(filePath);
                        if (dir != null)
                        {
                            parent.SetupWatcher(dir, filePath);
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Cosmetic.SetColor(ConsoleColor.Red);
                    Console.WriteLine($" Error opening file!");
                    Console.WriteLine($" {ex.Message}");
                    Console.ReadKey();
                    parent.interfaceMode = InterfaceMode.MainMenu;
                    return false;
                }
                //}
            }
            else if (showError)
            {
                Cosmetic.SetColor(ConsoleColor.Red);
                Console.WriteLine($" File not found:");
                Console.WriteLine(" " + filePath);
                Console.ReadKey();
            }
            return false;
        }

        internal void OpenFileDialog()
        {

            Cosmetic.ShowTitleBar("Open File");

            Cosmetic.SetColor(ConsoleColor.Yellow);
            Console.Write(" File: ");
            string fileSelect = Console.ReadLine() + "";

            Debug.WriteLine($"File selected {fileSelect}");
            fileSelect = Path.GetFullPath(fileSelect);
            Debug.WriteLine($"Full path: {fileSelect}");

            if (fileSelect is not null && File.Exists(fileSelect))
            {
                string? folder = Path.GetDirectoryName(Path.GetFullPath(fileSelect));
                if (folder is not null)
                {
                    Debug.WriteLine($"File: {fileSelect}, Folder for Watcher: {folder}");
                    parent.watcher = new(folder);
                }
                else
                {
                    Debug.WriteLine($"Folder selected {folder} is null");
                }
            }
            else
            {
                Debug.WriteLine($"File selected {fileSelect} is null or doesn't exist");
            }

            if (fileSelect != null && fileSelect.Length > 0 && LoadFile(Path.GetFullPath(fileSelect), true))
            {
                Debug.WriteLine("Load file succeeded");
                parent.monitorTextFile = fileSelect;
                parent.monitorDirectory = Path.GetDirectoryName(fileSelect);
                parent.interfaceMode = InterfaceMode.FileView;
            }
            else
            {
                parent.interfaceMode = InterfaceMode.MainMenu;
            }
        }

    }
}
