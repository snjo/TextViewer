
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Quic;
using System.Text;

namespace TextViewer;

class Program
{
    private static ConsoleColor defaultForeColor = Console.ForegroundColor;
    private static ConsoleColor defaultBackColor = Console.BackgroundColor;
    private static string[] lines = { };
    private static string? textfile = null;
    private static bool updateViewRequested = true;
    private static int scrollLine = 0;
    private static DateTime lastFileUpdate = DateTime.MinValue;
    private static FileSystemWatcher? watcher;
    private static bool forceFileUpdate = false;
    private static int fileUpdateCountdown = -1;

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.Unicode;
        int oldBufferWidth = Console.BufferWidth;
        int oldBufferHeight = Console.BufferHeight;

        textfile = "";

        string? directory = "";
        if (textfile != "")
        {
            directory = Path.GetDirectoryName(Path.GetFullPath(textfile));
        }
        Debug.WriteLine($"Loading file {textfile}, Direcory: {directory}");

        if (directory is not null)
        {
            SetupWatcher(textfile);
        }

        if (args.Length > 0)
        {
            if (File.Exists(args[0]))
            {
                textfile = Path.GetFullPath(args[0]);
                Console.WriteLine($"File in argument : {textfile}");
            }
            else
            {
                Console.WriteLine($"File in argument does not exist: '{args[0]}'");
                Console.WriteLine("Exiting.");
                Console.ReadKey();
                Environment.Exit(0);
            }

        }

        if (textfile == "")
        {
            OpenFileDialog(watcher);
        }
        else
        {
            LoadFile(textfile, true);
        }


        bool quit = false;
        DateTime lastViewUpdate = DateTime.Now;
        TimeSpan autoUpdateViewTime = TimeSpan.FromSeconds(30);

        while (quit is false)
        {
            if (fileUpdateCountdown == 0) // the watcher has detected a change, wait for the file to complete, maybe there's two triggers
            {
                LoadFile(textfile, false, false); // Load file, don't set up watcher again, already active
                forceFileUpdate = false;
                fileUpdateCountdown = -1; // update countdown parked
            }
            else if (forceFileUpdate)
            {
                LoadFile(textfile, false);
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
                ConsoleKeyInfo keyInput = Console.ReadKey();
                
                int linemax = Math.Max(lines.Length - 1, 0); // prevent error on 0 size file

                if (keyInput.Key == ConsoleKey.Q)
                {
                    RevertConsoleColors();
                    quit = true;
                }
                else if (keyInput.Key == ConsoleKey.PageDown || (keyInput.Key == ConsoleKey.DownArrow && keyInput.Modifiers == ConsoleModifiers.Shift))
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
                else if (keyInput.Key == ConsoleKey.L)
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
                else if (keyInput.Key == ConsoleKey.O)
                {
                    OpenFileDialog(watcher);
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

    private static void OpenFileDialog(FileSystemWatcher? watcher)
    {
        SetColor(ConsoleColor.Yellow);
        Console.SetCursorPosition(10, 5);
        Console.WriteLine("┌────────────────────────────────────────────────────┐");
        Console.SetCursorPosition(10, 6);
        Console.WriteLine("│ Open File:                                         │");
        Console.SetCursorPosition(10, 7);
        Console.WriteLine("└────────────────────────────────────────────────────┘");
        Console.SetCursorPosition(24, 6);
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

        if (LoadFile(fileSelect, true))
        {
            Debug.WriteLine("Load file succeeded");
            textfile = fileSelect;
        }
        else
        {
            textfile = "NO FILE";
            lines = ["", " Press O to open a file", ""];
            Debug.WriteLine("Load file failed");
        }
	}

	private static void SetupWatcher(string? filePath)
    {
        if (filePath is null || filePath == "") return; //file = "";
        string fullpath = Path.GetFullPath(filePath);
        string folder = Path.GetDirectoryName(fullpath) + "";
        string fileName = Path.GetFileName(filePath);
        Debug.WriteLine($"Setting up watcher, file: {fileName}, dir: {folder} --- path: {fullpath}");
        watcher = new(folder, fileName);
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += Watcher_OnChanged;
        watcher.EnableRaisingEvents = true;
        //watcher.Filter = "*.*";
        Debug.WriteLine($"Watcher setup complete: {watcher is not null}, {watcher?.Path} {watcher?.Filter}");
    }

    private static void UpdateView()
    {
        //Debug.WriteLine($"Updating View @{DateTime.Now.ToShortTimeString()}");
        Console.Clear();
        ConsoleColor TitleFG = ConsoleColor.Cyan;
        ConsoleColor TitleBG = ConsoleColor.Black;
        ConsoleColor TextFG = ConsoleColor.White;
        ConsoleColor TextBG = ConsoleColor.Black;
        Console.SetCursorPosition(0, 0);
        SetColor(TitleFG, TitleBG);
        string displayFileName = textfile + "";
        if (displayFileName.Length > Console.BufferWidth - 41)
        {
            displayFileName = Path.GetFileName(textfile) + "";
        }

        string appFileLine = $"┃ Text Viewer ┃ '{displayFileName}' ";
        string dateLine = $" {lastFileUpdate.ToShortDateString()} {lastFileUpdate.ToShortTimeString()} ";

        Console.Write(appFileLine.Substring(0, Math.Min(appFileLine.Length, Console.BufferWidth - dateLine.Length)).PadRight(Console.BufferWidth - dateLine.Length - 2, ' '));
        Console.Write("┃");
        TimeSpan timeSinceFileWrite = DateTime.Now - lastFileUpdate;
        //if (timeSinceFileWrite < TimeSpan.FromMinutes(1))
        //{
        TimeSpan Shortest = TimeSpan.FromMinutes(5);
        TimeSpan Medium = TimeSpan.FromMinutes(60);
        TimeSpan Old = TimeSpan.FromMinutes(600);

        if (timeSinceFileWrite < Shortest)
        {
            SetColor(ConsoleColor.Yellow);
            //Debug.WriteLine($"File is new {timeSinceFileWrite.TotalMinutes}");
        }
        else if (timeSinceFileWrite < Medium)
        {
            SetColor(ConsoleColor.Blue);
            //Debug.WriteLine($"File is medium {timeSinceFileWrite.TotalMinutes}");
        }
        else if (timeSinceFileWrite < Old)
        {
            SetColor(ConsoleColor.DarkBlue);
            //Debug.WriteLine($"File is medium {timeSinceFileWrite.TotalMinutes}");
        }
        else
        {
            SetColor(ConsoleColor.DarkGray);
            //Debug.WriteLine($"File is old {timeSinceFileWrite.TotalMinutes}");
        }

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
        Console.WriteLine($" [Q] Quit   [L] Select Line   [O] Open File   [Arrows] scroll   [PgUp/PgDn or Shift+Arrow] scroll 10   [F5] Refresh");
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

    private static bool LoadFile(string? file, bool showError, bool initWatcher = true)
    {
        if (file is not null)
        {
            if (File.Exists(file))
            {
                try
                {
                    lines = File.ReadAllLines(file);
                    updateViewRequested = true;
                    lastFileUpdate = File.GetLastWriteTime(file);

                    Debug.WriteLine($"File loaded {file}");

                    if (initWatcher)
                    {
                        watcher?.Dispose();
                        Debug.WriteLine("Asking to update Watcher");
                        SetupWatcher(file);
                    }

                    return true;
                }
                catch
                {
                    Debug.WriteLine("Can't read file");
                }
            }
        }

        if (showError)
        {
            SetColor(ConsoleColor.Red);
            Console.SetCursorPosition(10, 5);
            Console.WriteLine($"┌─────────────────────────────────────────────────────┐");
            Console.SetCursorPosition(10, 6);
            Console.WriteLine($"│                Error opening file!                  │");
            Console.SetCursorPosition(10, 7);
            Console.WriteLine($"│ {file} ".PadRight(54, ' ') + "│");
            Console.SetCursorPosition(10, 8);
            Console.WriteLine($"└──────────────────────────────────────────────[OK]───┘");
            Console.SetCursorPosition(35, 6);
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
