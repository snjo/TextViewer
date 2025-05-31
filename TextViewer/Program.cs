
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Quic;
using System.Runtime;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TextViewer;

class Program
{
    private static ConsoleColor defaultForeColor = Console.ForegroundColor;
    private static ConsoleColor defaultBackColor = Console.BackgroundColor;
    private static string[] lines = { };
    private static string? monitorTextFile = null;
    private static string? monitorDirectory = null;
    private static string monitorFilter = "*.*";
    private static bool updateViewRequested = true;
    private static int scrollLine = 0;
    private static DateTime lastFileUpdate = DateTime.MinValue;
    private static FileSystemWatcher? watcher;
    private static bool forceFileUpdate = false;
    private static int fileUpdateCountdown = -1;

    private enum InterfaceMode
    {
        MainMenu,
        FileView,
        DirectoryView,
        SelectFile,
        SelectDirectory,
        END
    }
    private static InterfaceMode interfaceMode = InterfaceMode.MainMenu;

    private static void parseArguments(string[] arguments)
    {
        //test
        //foreach (string arg in arguments)
        //{ 
        //    Console.WriteLine("Arg: >" + arg + "<");
            
        //}
        //Console.ReadKey();


        if (arguments.Length == 0)
            return;

        if (arguments.Length == 1)
        {
            string first = arguments[0];
            if (first == "-" && first.Length < 3)
            {
                Console.WriteLine($"Error in argument: {first}");
                Console.WriteLine("Use -? to show Help");
            }

            if (first == "-?" || first == "/?")
            {
                ShowHelp();
                RevertConsoleColors();
                Environment.Exit(0);
            }

            string argFile = arguments[0];
            arguments = [ "-f", argFile ];
        }

        if (arguments.Length >= 2)
        {
            if (arguments[0] == "-f")
            {
                interfaceMode = InterfaceMode.FileView;
                monitorTextFile = Path.GetFullPath(arguments[1]);
                monitorDirectory = Path.GetDirectoryName(monitorTextFile);
                SetupWatcher(monitorDirectory, monitorTextFile);
            }
            else if (arguments[0] == "-d")
            {
                monitorTextFile = "";
                monitorDirectory = arguments[1];
                SetupWatcher(monitorDirectory, "*.*");
                interfaceMode = InterfaceMode.DirectoryView;
                if (arguments.Length >= 3)
                {
                    monitorFilter = arguments[2];
                }
                //Console.WriteLine($"Directory mode: dir:{monitorDirectory} file:{monitorTextFile} filter:{monitorFilter}");
                //Console.ReadKey();
            }
            return;
        }
        //interfaceMode = InterfaceMode.MainMenu;
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.Unicode;
        int oldBufferWidth = Console.BufferWidth;
        int oldBufferHeight = Console.BufferHeight;
        
        parseArguments(args);

        if (interfaceMode == InterfaceMode.FileView)
        {
            if (LoadFile(monitorTextFile, true) == false)
            {
                interfaceMode = InterfaceMode.MainMenu;
            }
        }


        bool quit = false;
        DateTime lastViewUpdate = DateTime.Now;
        TimeSpan autoUpdateViewTime = TimeSpan.FromSeconds(30);

        while (quit is false)
        {
            if (fileUpdateCountdown == 0) // the watcher has detected a change, wait for the file to complete, maybe there's two triggers
            {
                if (interfaceMode == InterfaceMode.FileView) LoadFile(monitorTextFile, false, false); // Load file, don't set up watcher again, already active
                forceFileUpdate = false;
                fileUpdateCountdown = -1; // update countdown parked
                updateViewRequested = true;
            }
            else if (forceFileUpdate)
            {
                LoadFile(monitorTextFile, false);
                forceFileUpdate = false;
            }

            if (fileUpdateCountdown > 0) fileUpdateCountdown--;

            if (DateTime.Now - lastViewUpdate > autoUpdateViewTime)
            {
                updateViewRequested = true;
            }

            if (oldBufferWidth != Console.BufferWidth || oldBufferHeight != Console.BufferHeight)
            {
                updateViewRequested = true;
            }
            oldBufferWidth = Console.BufferWidth;
            oldBufferHeight = Console.BufferHeight;

            if (updateViewRequested)
            {
                UpdateView();
                updateViewRequested = false;
                lastViewUpdate = DateTime.Now;
            }

            if (Console.KeyAvailable)
            {
                updateViewRequested = true;
                ConsoleKeyInfo keyInput = Console.ReadKey();

                if (keyInput.Key == ConsoleKey.Q)
                {
                    RevertConsoleColors();
                    quit = true;
                }
                else if (keyInput.Key == ConsoleKey.Escape)
                {
                    if (interfaceMode != InterfaceMode.MainMenu)
                    {
                        interfaceMode = InterfaceMode.MainMenu;
                    }
                    else
                    {
                        RevertConsoleColors();
                        quit = true;
                    }
                }
                else if (keyInput.Key == ConsoleKey.M || keyInput.Key == ConsoleKey.Escape)
                {
                    interfaceMode = InterfaceMode.MainMenu;
                }
                else if (keyInput.Key == ConsoleKey.F)
                {
                    interfaceMode = InterfaceMode.SelectFile;
                }
                else if (keyInput.Key == ConsoleKey.D)
                {
                    interfaceMode = InterfaceMode.SelectDirectory;
                }
                else if (interfaceMode == InterfaceMode.FileView)
                {
                    HandleFileViewKeys(keyInput);
                }

                UpdateView();
                lastViewUpdate = DateTime.Now;
            }
            else
            {
                Thread.Sleep(100);
            }
        }
    }

    private static void HandleFileViewKeys(ConsoleKeyInfo keyInput)
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
            int max = Math.Max(lines.Length - 1, 0); // prevent error on 0 size file
            scrollLine = Math.Clamp(scrollLine, 0, linemax);
        }
        else if (keyInput.Key == ConsoleKey.F5)
        {
            forceFileUpdate = true;
        }
        else if (keyInput.Key == ConsoleKey.L && interfaceMode == InterfaceMode.FileView)
        {
            SetColor(ConsoleColor.Yellow);
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
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Monitors a file or directory for changes and highlights any recently edited files. Updates file contents when modified.");
        Console.WriteLine();
        Console.WriteLine("TEXTVIEWER [file]");
        Console.WriteLine("TEXTVIEWER -f file");
        Console.WriteLine("TEXTVIEWER -d directory [filter]");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("textviewer -d c:\\temp *.txt    Don't use backslash at the end of a directory argument");
        Console.WriteLine("textviewer -d \"c:\\Program Files\" *.*    Don't use backslash at the end of a directory argument");

    }

    private static void OpenDirectoryDialog()
    {
        Console.Clear();
        SetColor(ConsoleColor.Black, ConsoleColor.Cyan);
        Console.WriteLine($" Open Directory".PadRight(Console.BufferWidth));
        SetColor(ConsoleColor.White, ConsoleColor.Black);
        Console.WriteLine();

        SetColor(ConsoleColor.Yellow);
        Console.Write(" Directory: ");
        string directorySelect = Console.ReadLine() + "";
        if (directorySelect.Length == 0) directorySelect = ".";
        Console.Write(" Filter: ");
        SetColor(ConsoleColor.Gray);
        Console.Write("*");
        SetColor(ConsoleColor.White);
        Console.SetCursorPosition(9, 3);
        monitorFilter = Console.ReadLine() + "";
        if (monitorFilter == null || monitorFilter == "")
        {
            monitorFilter = "*";
        }


        if (directorySelect is null)
        {
            interfaceMode = InterfaceMode.MainMenu;
            return;
        }

        if (Directory.Exists(directorySelect))
        {
            monitorDirectory = directorySelect;
            interfaceMode = InterfaceMode.DirectoryView;
            SetupWatcher(directorySelect, null);
        }
        else
        {
            SetColor(ConsoleColor.Red);
            Console.WriteLine("Directory does not exist.");
            Debug.WriteLine("Directory does not exist.");
            Console.ReadKey();
            Debug.WriteLine("Exiting Directory select");
            interfaceMode = InterfaceMode.MainMenu;
            updateViewRequested = true;
        }
    }

    private static void OpenFileDialog()
    {
        Console.Clear();
        SetColor(ConsoleColor.Black, ConsoleColor.Cyan);
        Console.WriteLine($" Open File".PadRight(Console.BufferWidth));
        SetColor(ConsoleColor.White, ConsoleColor.Black);
        Console.WriteLine();

        SetColor(ConsoleColor.Yellow);
        Console.Write(" File: ");
        string fileSelect = Console.ReadLine() + "";

        Debug.WriteLine($"File selected {fileSelect}");

        if (fileSelect is not null && File.Exists(fileSelect))
        {
            string? folder = Path.GetDirectoryName(Path.GetFullPath(fileSelect));
            if (folder is not null)
            {
                Debug.WriteLine($"File: {fileSelect}, Folder for Watcher: {folder}");
                watcher = new(folder);
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
            monitorTextFile = fileSelect;
            interfaceMode = InterfaceMode.FileView;
        }
        else
        {
            interfaceMode = InterfaceMode.MainMenu;
        }
	}

	private static void SetupWatcher(string? directory, string? filePath)
    {
        string fullpath;
        string folder;
        string filter;

        if (filePath is null || filePath == "")
        {
            if (directory is null) return;
            if (directory == "") directory = ".";
            fullpath = Path.GetFullPath(directory);
            folder = fullpath;
            filter = "";
        }
        else
        {
            fullpath = Path.GetFullPath(filePath);
            folder = Path.GetDirectoryName(fullpath) + "";
            filter = Path.GetFileName(filePath);
        }

        
        
        Debug.WriteLine($"Setting up watcher, filter: {filter}, dir: {folder} --- path: {fullpath}");
        watcher = new(folder, filter);

        watcher.NotifyFilter = NotifyFilters.Attributes
                      | NotifyFilters.CreationTime
                      | NotifyFilters.DirectoryName
                      | NotifyFilters.FileName
                      | NotifyFilters.LastAccess
                      | NotifyFilters.LastWrite
                      | NotifyFilters.Security
                      | NotifyFilters.Size;

        watcher.Changed += Watcher_OnChanged;
        watcher.Created += Watcher_OnChanged;
        watcher.Deleted += Watcher_OnChanged;
        watcher.Renamed += Watcher_OnChanged;
        watcher.Error += Watcher_OnError;



        //watcher.NotifyFilter = NotifyFilters.LastWrite;

        watcher.EnableRaisingEvents = true;
        //watcher.Filter = "*.*";
        Debug.WriteLine($"Watcher setup complete: {watcher is not null}, {watcher?.Path} {watcher?.Filter}");
    }

    private static void Watcher_OnError(object sender, ErrorEventArgs e)
    {
        Debug.WriteLine($"Watcher error: {e.GetException().Message}");
    }

    private static void UpdateView()
    {
        if (interfaceMode == InterfaceMode.MainMenu)
        {
            UpdateMainMenuView();
        }
        else if (interfaceMode == InterfaceMode.FileView)
        {
            UpdateFileView();
        }
        else if (interfaceMode == InterfaceMode.DirectoryView)
        {
            UpdateDirectoryView();
        }
        else if (interfaceMode == InterfaceMode.SelectFile)
        {
            OpenFileDialog();
        }
        else if (interfaceMode == InterfaceMode.SelectDirectory)
        {
            OpenDirectoryDialog();
        }
        else
        {
            Console.Clear();
            Console.WriteLine("Huh, what happened?");
        }
    }

    private static void UpdateFileView()
    {
        //Debug.WriteLine($"Updating View @{DateTime.Now.ToShortTimeString()}");
        Console.Clear();
        ConsoleColor TitleFG = ConsoleColor.Cyan;
        ConsoleColor TitleBG = ConsoleColor.Black;
        ConsoleColor TextFG = ConsoleColor.White;
        ConsoleColor TextBG = ConsoleColor.Black;
        Console.SetCursorPosition(0, 0);
        SetColor(TitleFG, TitleBG);
        string displayFileName = monitorTextFile + "";
        if (displayFileName.Length > Console.BufferWidth - 41)
        {
            displayFileName = Path.GetFileName(monitorTextFile) + "";
        }

        string appFileLine = $"┃ Text Viewer ┃ '{displayFileName}' ";
        string dateLine = $" {lastFileUpdate.ToShortDateString()} {lastFileUpdate.ToShortTimeString()} ";

        Console.Write(appFileLine.Substring(0, Math.Min(appFileLine.Length, Console.BufferWidth - dateLine.Length)).PadRight(Console.BufferWidth - dateLine.Length - 2, ' '));
        Console.Write("┃");
        SetColorFromAge(lastFileUpdate);

        Console.Write(dateLine);
        SetColor(TitleFG);
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
        Console.WriteLine($" [Q] Quit  [Esc] Menu  [L] Select Line  [Arrows] scroll  [PgUp/PgDn or Shift+Arrow] scroll 10   [F5] Refresh");
    }

    private static void SetColorFromAge(DateTime time)
    {
        TimeSpan timeSinceFileWrite = DateTime.Now - time;
        //if (timeSinceFileWrite < TimeSpan.FromMinutes(1))
        //{
        TimeSpan Shortest = TimeSpan.FromMinutes(5);
        TimeSpan Medium = TimeSpan.FromMinutes(60);
        TimeSpan Old = TimeSpan.FromMinutes(600);

        if (timeSinceFileWrite < Shortest)
        {
            SetColor(ConsoleColor.Yellow);
        }
        else if (timeSinceFileWrite < Medium)
        {
            SetColor(ConsoleColor.Blue);
        }
        else if (timeSinceFileWrite < Old)
        {
            SetColor(ConsoleColor.DarkBlue);
        }
        else
        {
            SetColor(ConsoleColor.DarkGray);
        }
    }

    private static void UpdateDirectoryView()
    {
        
        Console.Clear();
        if (Directory.Exists(monitorDirectory) == false)
        {
            Console.WriteLine($"Directory does not exist: {monitorDirectory}");
            Console.WriteLine("Check that command line arguments don't end in a backslash.");
            interfaceMode = InterfaceMode.MainMenu;
            return;
        }

        SetColor(ConsoleColor.Black, ConsoleColor.Cyan);
        if (monitorDirectory == null) monitorDirectory = ".";
        Console.WriteLine($" DirectoryView : {Path.GetFullPath(monitorDirectory)}".PadRight(Console.BufferWidth));
        SetColor(ConsoleColor.White, ConsoleColor.Black);
        Console.WriteLine();

        if (monitorDirectory is null)
        {
            Console.WriteLine("Error, directory is null");
        }
        else
        {
            string[] subDirectories = [];
            string[] filesinDirectory = [];
            try
            {
                subDirectories = Directory.GetDirectories(monitorDirectory, monitorFilter);
                filesinDirectory = Directory.GetFiles(monitorDirectory, monitorFilter);
            }
            catch
            {
                Console.WriteLine($"Error parsing path {monitorDirectory}, could not get list of files and subfolders.");
            }
            SetColor(ConsoleColor.White);
            Console.WriteLine("Size        Created           Modified           Age  Name ");

            foreach (string dir in subDirectories)
            {
                SetColor(ConsoleColor.Yellow);
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                Console.Write($"  <DIR>     ");
                SetColor(ConsoleColor.Gray);
                Console.Write($"{DateAndTimeString(dirInfo.CreationTime)}");
                SetColorFromAge(dirInfo.LastWriteTime);
                Console.Write($"  {DateAndTimeString(dirInfo.LastWriteTime)} ");
                Console.Write($"{((int)(DateTime.Now - dirInfo.LastWriteTime).TotalDays).ToString().PadLeft(4,' ')}d  ");
                SetColor(ConsoleColor.Yellow);
                Console.WriteLine(Path.GetFileName(dir));
            }
            
            SetColor(ConsoleColor.Cyan);

            foreach (string file in filesinDirectory)
            {
                FileAttributes attributes = File.GetAttributes(file);
                FileInfo fileInfo = new FileInfo(file);
                long size = fileInfo.Length;
                int sizeFraction = 1;
                if (size > 1024) sizeFraction = 1024;
                if (size >= 1024 * 1024) sizeFraction = 1024*1024;
                if (size >= 1024 * 1024 * 1024) sizeFraction = 1024 * 1024 * 1024;
                string sizeDisplay = (size / sizeFraction).ToString();
                if (sizeFraction == 1) sizeDisplay += " B ";
                if (sizeFraction == 1024) sizeDisplay += " KB";
                if (sizeFraction == 1024*1024) sizeDisplay += " MB";
                if (sizeFraction == 1024 * 1024 * 1024) sizeDisplay += " GB";
                Console.Write($"{sizeDisplay.PadLeft(10)}  ");
                SetColor(ConsoleColor.Gray);
                Console.Write($"{DateAndTimeString(fileInfo.CreationTime)}  ");
                SetColorFromAge(fileInfo.LastWriteTime);
                Console.Write($"{DateAndTimeString(fileInfo.LastWriteTime)} ");
                Console.Write($"{((int)(DateTime.Now - fileInfo.LastWriteTime).TotalDays).ToString().PadLeft(4, ' ')}d  ");
                SetColor(ConsoleColor.Cyan);
                Console.WriteLine(Path.GetFileName(file));
            }
        }
        Console.WriteLine();
        SetColor(ConsoleColor.Cyan);
        Console.WriteLine($" [Q] Quit  [Esc] Menu  [F] Open File  [D] Open Directory  [F5] Refresh");
    }

    private static string DateAndTimeString( DateTime date )
    {
        return $"{date.ToShortDateString()} {date.ToShortTimeString()}";
    }

    private static void UpdateMainMenuView()
    {
        SetColor(ConsoleColor.White, ConsoleColor.Black);
        Console.Clear();
        SetColor(ConsoleColor.Black, ConsoleColor.Cyan);
        Console.WriteLine(" Main Menu".PadRight(Console.BufferWidth));
        Console.WriteLine();
        
        SetColor(ConsoleColor.White, ConsoleColor.Black);
        Console.Write(" F   ");
        SetColor(ConsoleColor.Cyan);
        Console.WriteLine("Open File");

        SetColor(ConsoleColor.White);
        Console.Write(" D   ");
        SetColor(ConsoleColor.Cyan);
        Console.WriteLine("Open Directory");

        SetColor(ConsoleColor.White);
        Console.Write(" Q   ");
        SetColor(ConsoleColor.Cyan);
        Console.WriteLine("Quit");
    }

    public static IEnumerable<string> SplitByLength(string str, int maxLength) {
        for (int index = 0; index < str.Length; index += maxLength) {
            yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
        }
    }

    private static void PrintTextLine(string line, string lineNumber, ConsoleColor TitleFG, ConsoleColor TitleBG, ConsoleColor TextFG, ConsoleColor TextBG)
    {
        Console.Write($"┃ {(lineNumber).ToString().PadLeft(4, '0')} ");
        Console.Write("┇");
        SetColor(TextFG, TextBG);
        Console.Write(line.PadRight(Console.BufferWidth - 9, ' '));
        SetColor(TitleFG, TitleBG);
        Console.WriteLine("┃");
    }

	private static void Watcher_OnChanged(object sender, FileSystemEventArgs e)
	{
		Debug.WriteLine($"File changed at {DateTime.Now.ToShortTimeString()}");
        Debug.WriteLine($"   {((FileSystemWatcher)sender).Path} : {e.ChangeType} {e.Name}");
        
        fileUpdateCountdown = 10; // wait a bit before opening the file, it might still be held by the save process. Prevents double triggering of file load if watcher event fires twice
	}

    private static bool LoadFile(string? filePath, bool showError, bool initWatcher = true)
    {
        if (filePath is not null)
        {
            //if (File.Exists(filePath))
            //{
                try
                {
                    lines = File.ReadAllLines(filePath);
                    updateViewRequested = true;
                    lastFileUpdate = File.GetLastWriteTime(filePath);

                    Debug.WriteLine($"File loaded {filePath}");

                    if (initWatcher)
                    {
                        watcher?.Dispose();
                        Debug.WriteLine("Asking to update Watcher");
                        string? dir = Path.GetDirectoryName(filePath);
                        if (dir != null)
                        { 
                            SetupWatcher(dir, filePath); 
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    SetColor(ConsoleColor.Red);
                    Console.WriteLine($" Error opening file!");
                    Console.WriteLine($" {ex.Message}");
                    Console.ReadKey();
                    interfaceMode = InterfaceMode.MainMenu;
                    return false;
                }
            //}
        }
        else if (showError)
        {
            SetColor(ConsoleColor.Red);
            Console.WriteLine($" File not found:");
            Console.WriteLine(" " + filePath);
            Console.ReadKey();
        }
        return false;
	}

	static void SetColor(ConsoleColor foreground, ConsoleColor? background = null)
    {
        Console.ForegroundColor = foreground;
        if (background is not null)
            Console.BackgroundColor = (ConsoleColor)background;
    }
    
    static void RevertConsoleColors()
    {
        Console.BackgroundColor = Program.defaultBackColor;
        Console.BackgroundColor = Program.defaultBackColor;
    }
}
