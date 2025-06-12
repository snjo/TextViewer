using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using static TextViewer.TextViewerLoop;

namespace TextViewer
{
    internal class DirectoryView (TextViewerLoop parent)
    {
        int selectedLine = 0;
        string[] subDirectories = [];
        string[] filesinDirectory = [];
        TextScroller textScroller = new();

        internal void OpenDirectoryDialog()
        {
            selectedLine = 0;
            Cosmetic.ShowTitleBar($"Open Directory");

            Cosmetic.SetColor(ConsoleColor.Yellow);
            Console.Write(" Directory: ");
            string directorySelect = Console.ReadLine() + "";
            if (directorySelect.Length == 0) directorySelect = ".";
            if (directorySelect.Substring(directorySelect.Length-1,1) != "\\")
            {
                // this prevents "c:" from being interpreted as the program's folder by making it "c:\"
                directorySelect += "\\";
                Debug.WriteLine($"Adding \\ to end of directory: {directorySelect}");
            }
            Console.Write(" Filter: ");
            Cosmetic.SetColor(ConsoleColor.Gray);
            Console.Write("*");
            Cosmetic.SetColor(ConsoleColor.White);
            Console.SetCursorPosition(9, 3);
            parent.monitorFilter = Console.ReadLine() + "";
            if (parent.monitorFilter == null || parent.monitorFilter == "")
            {
                parent.monitorFilter = "*";
            }


            if (directorySelect is null)
            {
                parent.interfaceMode = InterfaceMode.MainMenu;
                return;
            }

            if (Directory.Exists(directorySelect))
            {
                parent.monitorDirectory = directorySelect;
                parent.interfaceMode = InterfaceMode.DirectoryView;
                parent.SetupWatcher(directorySelect, null);
            }
            else
            {
                Cosmetic.SetColor(ConsoleColor.Red);
                Console.WriteLine("Directory does not exist.");
                Debug.WriteLine("Directory does not exist.");
                Console.ReadKey();
                Debug.WriteLine("Exiting Directory select");
                parent.interfaceMode = InterfaceMode.MainMenu;
                parent.updateViewRequested = true;
            }
        }

        internal void UpdateDirectoryView()
        {
            textScroller.visibleBottomLines = textScroller.PageHeight;
            textScroller.ResetLines(false);
            textScroller.ResetLineBuilder();
            textScroller.HiglightLine = selectedLine;

            if (Directory.Exists(parent.monitorDirectory) == false)
            {
                Console.WriteLine($"Directory does not exist: {parent.monitorDirectory}");
                Console.WriteLine("Check that command line arguments don't end in a backslash.");
                parent.interfaceMode = InterfaceMode.MainMenu;
                return;
            }

            Cosmetic.ShowTitleBar($"DirectoryView : {Path.GetFullPath(parent.monitorDirectory)}");

            if (parent.monitorDirectory is null)
            {
                Console.WriteLine("Error, directory is null");
            }
            else
            {
                try
                {
                    subDirectories = Directory.GetDirectories(parent.monitorDirectory, parent.monitorFilter);
                    filesinDirectory = Directory.GetFiles(parent.monitorDirectory, parent.monitorFilter);
                }
                catch
                {
                    Console.WriteLine($"Error parsing path {parent.monitorDirectory}, could not get list of files and subfolders.");
                }
                Cosmetic.SetColor(ConsoleColor.White);
                Console.WriteLine("  Size       Created           Modified           Age  Name ");

                int itemCount = 0;
                //int topMargin = 3;

                if (subDirectories.Length == 0 && filesinDirectory.Length == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine(" Directory is empty.");
                }

                foreach (string dir in subDirectories)
                {
                    //PlaceCursor(itemCount);
                    textScroller.SetColor(Color.Yellow);
                    DirectoryInfo dirInfo = new(dir);
                    textScroller.AddTextToLine($"  <DIR>     ");
                    textScroller.SetColor(Color.Gray);
                    textScroller.AddTextToLine($"{TextViewerLoop.DateAndTimeString(dirInfo.CreationTime)}  ");
                    textScroller.SetColor(Cosmetic.GetColorFromAge(dirInfo.LastWriteTime));
                    textScroller.AddTextToLine($"{TextViewerLoop.DateAndTimeString(dirInfo.LastWriteTime)}  ");
                    string timeSiceText = TimeSinceToString(dirInfo.LastWriteTime);
                    textScroller.AddTextToLine($"{timeSiceText,4}  "); // pad left 4
                    textScroller.SetColor(Color.Yellow);
                    textScroller.FinishLine(Path.GetFileName(dir));
                    itemCount++;
                }

                Cosmetic.SetColor(ConsoleColor.Cyan);

                foreach (string file in filesinDirectory)
                {
                    //PlaceCursor(itemCount);
                    FileInfo fileInfo = new(file);
                    string sizeDisplay = FileSizeToText(fileInfo);
                    textScroller.AddTextToLine($"{sizeDisplay,10}  ");
                    textScroller.SetColor(Color.Gray);
                    textScroller.AddTextToLine($"{DateAndTimeString(fileInfo.CreationTime)}  ");
                    textScroller.SetColor(Cosmetic.GetColorFromAge(fileInfo.LastWriteTime));
                    textScroller.AddTextToLine($"{DateAndTimeString(fileInfo.LastWriteTime)}  ");
                    string timeSiceText = TimeSinceToString(fileInfo.LastWriteTime);
                    textScroller.AddTextToLine($"{timeSiceText,4}  "); // pad left 4
                    textScroller.SetColor(Color.Cyan);
                    textScroller.FinishLine(Path.GetFileName(file));
                    itemCount++;
                }

                Console.SetCursorPosition(0, 3);
                textScroller.OutputLines(null);


                if (selectedLine > itemCount - 1)
                {
                    selectedLine = itemCount - 1;
                    parent.updateViewRequested = true;
                }
            }
            Console.WriteLine();
            Cosmetic.SetColor(ConsoleColor.Cyan);
            int cursorTop = Console.GetCursorPosition().Top;
            Console.WriteLine($" [Esc] Menu  [Enter] Open  [Backspace] Parent Dir.  [F] File  [D] Directory  [F5] Refresh  [E] Log");
            //Console.SetCursorPosition(0, 4);
            //Console.Write("🢂");
            //Console.SetCursorPosition(0, cursorTop);
        }

        void PlaceCursor(int lineNumber)
        {
            if (lineNumber == selectedLine)
            {
                Console.Write("🢂");
            }
            else
            {
                Console.Write(" ");
            }
        }

        private static string FileSizeToText(FileInfo fileInfo)
        {
            long size = fileInfo.Length;
            int sizeFraction = 1;
            if (size > 1024) sizeFraction = 1024;
            if (size >= 1024 * 1024) sizeFraction = 1024 * 1024;
            if (size >= 1024 * 1024 * 1024) sizeFraction = 1024 * 1024 * 1024;
            string sizeDisplay = (size / sizeFraction).ToString();
            if (sizeFraction == 1) sizeDisplay += " B ";
            if (sizeFraction == 1024) sizeDisplay += " KB";
            if (sizeFraction == 1024 * 1024) sizeDisplay += " MB";
            if (sizeFraction == 1024 * 1024 * 1024) sizeDisplay += " GB";
            return sizeDisplay;
        }

        private static string TimeSinceToString(DateTime time)
        {
            string timeSiceText;
            TimeSpan timeSince = DateTime.Now - time;
            if (timeSince.TotalDays >= 365)
            {
                timeSiceText = $"{(int)(timeSince.TotalDays/365)}y";
            }
            else if (timeSince.TotalDays >= 1)
            {
                timeSiceText = $"{(int)(timeSince.TotalDays)}d";
            }
            else if (timeSince.TotalHours >= 1)
            {
                timeSiceText = $"{(int)(timeSince.TotalHours)}h";
            }
            else
            {
                timeSiceText = $"{(int)(timeSince.TotalMinutes)}m";
            }

            return timeSiceText;
        }

        public void HandleDirectoryViewKeys(ConsoleKeyInfo keyInput)
        {
            if ((keyInput.Key == ConsoleKey.DownArrow && keyInput.Modifiers == ConsoleModifiers.Shift) || keyInput.Key == ConsoleKey.PageDown)
            {
                textScroller.changeScroll(10);
                selectedLine += 10;
            }
            else if ((keyInput.Key == ConsoleKey.UpArrow && keyInput.Modifiers == ConsoleModifiers.Shift) || keyInput.Key == ConsoleKey.PageUp)
            {
                textScroller.changeScroll(-10);
                selectedLine -= 10;
                if (selectedLine < 0)
                {
                    selectedLine = 0;
                }
            }

            else if (keyInput.Key == ConsoleKey.DownArrow)
            {
                selectedLine++;
                if (textScroller.HighlightIsAtEndOfPage())
                {
                    textScroller.changeScroll(1);
                }
            }
            else if (keyInput.Key >= ConsoleKey.UpArrow)
            {
                selectedLine--;
                if (selectedLine < 0)
                {
                    selectedLine = 0;
                }
                if (textScroller.HighlightIsAtStartOfPage())
                {
                    textScroller.changeScroll(-1);
                }
                //textScroller.changeScroll(-1);
            }
            else if (keyInput.Key == ConsoleKey.Enter)
            {
                int selectedItem = textScroller.HiglightLine;
                Debug.WriteLine($"Open selected item {selectedItem}");
                if (selectedItem < subDirectories.Length)
                {
                    parent.monitorDirectory = subDirectories[selectedItem];
                    parent.interfaceMode = InterfaceMode.DirectoryView;
                    parent.SetupWatcher(parent.monitorDirectory, null);
                    textScroller.scrollPosition = 0;
                    //selectedItem = 0;
                }
                else
                {

                    int index = selectedItem - subDirectories.Length;
                    parent.monitorTextFile = filesinDirectory[index];
                    parent.monitorDirectory = Path.GetDirectoryName(parent.monitorTextFile);
                    parent.fileView.LoadFile(parent.monitorTextFile, true, true);
                    parent.interfaceMode = InterfaceMode.FileView;
                    parent.fileView.scrollLine = 0;
                    //selectedLine = 0;
                }
            }
            else if (keyInput.Key == ConsoleKey.Backspace)
            {
                Debug.WriteLine($"Changing to parent directory from {parent.monitorDirectory}");
                parent.monitorDirectory = Path.GetDirectoryName(parent.monitorDirectory); // parent directory
                Debug.WriteLine($"                               to {parent.monitorDirectory}");
                parent.interfaceMode = InterfaceMode.DirectoryView;
                parent.SetupWatcher(parent.monitorDirectory, null);
                textScroller.scrollPosition = 0;
                //selectedLine = 0;
            }
        }
    }
}
