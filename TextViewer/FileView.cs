using System.Diagnostics;
using static TextViewer.TextViewerLoop;

namespace TextViewer
{
    internal class FileView(TextViewerLoop parent)
    {
        internal string[] lines = [];
        internal int scrollLine = 0;
        public bool forceFileUpdate = false;
        public int fileUpdateCountdown = -1;
        internal DateTime lastFileUpdate = DateTime.MinValue;
        private FileTypes.ContentTypes contentType = FileTypes.ContentTypes.Unknown;
        private ImageParser? imageParser = null;

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
                parent.SetupWatcher(parent.monitorDirectory, null, parent.includeSubFolders);
            }
        }

        private void UpdateTextView()
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
            Console.WriteLine($" [Esc] Menu  [Backspace] Parent Dir.  [L] Goto Line  [↓↑] scroll  [PgUp PgDn Shift+↓↑] scroll 10  [F5] Refresh  [E] Log");
        }

        private void UpdateImageView()
        {
            Console.Clear();
            if (imageParser != null)
            {
                imageParser.WriteImageToConsole(0, 0, Console.BufferWidth, Console.BufferHeight, true);
            }
        }

        internal void UpdateFileView()
        {
            if (contentType == FileTypes.ContentTypes.Text)
            {
                UpdateTextView();
            }
            else if (contentType == FileTypes.ContentTypes.Image)
            {
                UpdateImageView();
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Only text and image files can be viewed.");
                Console.WriteLine("Press any key to return.");
                Console.ReadKey();
                parent.interfaceMode = InterfaceMode.DirectoryView;
                parent.SetupWatcher(parent.monitorDirectory, null, parent.includeSubFolders);
            }
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
            Debug.WriteLine($"Loading file: {filePath} showError: {showError} init: {initWatcher}");
            scrollLine = 0;

            string extension = Path.GetExtension(filePath) + "";
            contentType = FileTypes.GetContentType(extension);

            lines = [];

            if (filePath is not null)
            {
                try
                {
                    if (contentType == FileTypes.ContentTypes.Text)
                    {
                        lines = File.ReadAllLines(filePath);
                    }
                    else if (contentType == FileTypes.ContentTypes.Image)
                    {
                        if (imageParser != null)
                        {
                            imageParser.Dispose();
                        }
                        imageParser = new(filePath);
                    }
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
                            parent.SetupWatcher(dir, filePath, parent.includeSubFolders);
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($" Exception when opening file: {filePath}");
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
                Debug.WriteLine($" File not found: {filePath}");
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
            if (fileSelect == "")
            {
                return;
            }

            Debug.WriteLine($"File selected {fileSelect}");
            fileSelect = Environment.ExpandEnvironmentVariables(fileSelect);
            fileSelect = Path.GetFullPath(fileSelect);
            Debug.WriteLine($"Full path: {fileSelect}");

            if (fileSelect is not null && File.Exists(fileSelect))
            {
                string? folder = Path.GetDirectoryName(Path.GetFullPath(fileSelect));
                if (folder is not null)
                {
                    Debug.WriteLine($"File: {fileSelect}, Folder for Watcher: {folder}");
                    parent.SetupWatcher(null, folder, parent.includeSubFolders);
                    //parent.watcher = new(folder) { IncludeSubdirectories = parent.includeSubFolders };
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
