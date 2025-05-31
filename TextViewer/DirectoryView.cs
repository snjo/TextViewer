using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TextViewer.TextViewerLoop;

namespace TextViewer
{
    internal class DirectoryView (TextViewerLoop parent)
    {


        internal void OpenDirectoryDialog()
        {
            Cosmetic.ShowTitleBar($"Open Directory");

            Cosmetic.SetColor(ConsoleColor.Yellow);
            Console.Write(" Directory: ");
            string directorySelect = Console.ReadLine() + "";
            if (directorySelect.Length == 0) directorySelect = ".";
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
                string[] subDirectories = [];
                string[] filesinDirectory = [];
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
                Console.WriteLine("Size        Created           Modified           Age  Name ");

                foreach (string dir in subDirectories)
                {
                    Cosmetic.SetColor(ConsoleColor.Yellow);
                    DirectoryInfo dirInfo = new(dir);
                    Console.Write($"  <DIR>     ");
                    Cosmetic.SetColor(ConsoleColor.Gray);
                    Console.Write($"{TextViewerLoop.DateAndTimeString(dirInfo.CreationTime)}");
                    Cosmetic.SetColorFromAge(dirInfo.LastWriteTime);
                    Console.Write($"  {TextViewerLoop.DateAndTimeString(dirInfo.LastWriteTime)} ");
                    Console.Write($"{(int)(DateTime.Now - dirInfo.LastWriteTime).TotalDays,4}d  "); // pad left 4
                    Cosmetic.SetColor(ConsoleColor.Yellow);
                    Console.WriteLine(Path.GetFileName(dir));
                }

                Cosmetic.SetColor(ConsoleColor.Cyan);

                foreach (string file in filesinDirectory)
                {
                    FileInfo fileInfo = new(file);
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
                    Console.Write($"{sizeDisplay,10}  ");
                    Cosmetic.SetColor(ConsoleColor.Gray);
                    Console.Write($"{DateAndTimeString(fileInfo.CreationTime)}  ");
                    Cosmetic.SetColorFromAge(fileInfo.LastWriteTime);
                    Console.Write($"{DateAndTimeString(fileInfo.LastWriteTime)} ");
                    Console.Write($"{(int)(DateTime.Now - fileInfo.LastWriteTime).TotalDays,4}d  ");
                    Cosmetic.SetColor(ConsoleColor.Cyan);
                    Console.WriteLine(Path.GetFileName(file));
                }
            }
            Console.WriteLine();
            Cosmetic.SetColor(ConsoleColor.Cyan);
            Console.WriteLine($" [Q] Quit  [Esc] Menu  [F] Open File  [D] Open Directory  [F5] Refresh");
        }
    }
}
