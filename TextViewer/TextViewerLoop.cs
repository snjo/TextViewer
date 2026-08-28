using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace TextViewer
{
    public class TextViewerLoop
    {
        internal readonly FileView fileView;
        internal readonly DirectoryView directoryView;
        internal readonly EventLogView eventLogView;
        readonly string[] arguments;
        internal bool includeSubFolders = false;

        public TextViewerLoop(string[] args)
        {
            arguments = args;
            fileView = new(this);
            directoryView = new(this, "PreviewableFileTypes.txt");
            eventLogView = new(this);
        }

        private readonly ConsoleColor defaultForeColor = Console.ForegroundColor;
        private readonly ConsoleColor defaultBackColor = Console.BackgroundColor;

        internal string? monitorTextFile = null;
        internal string? monitorDirectory = null;
        internal string monitorFilter = "*.*";
        internal bool updateViewRequested = true;
        internal FileSystemWatcher? watcher;

        internal bool alertWhenDirectoryChanged = false;
        internal bool alertWhenFileChanged = false;
        //WatcherChangeTypes changeType = WatcherChangeTypes.All;
        bool alertChangeTypeAll = false;
        bool alertChangeTypeCreated = true;
        bool alertChangeTypeChanged = true;
        bool alertChangeTypeDeleted = true;
        bool alertChangeTypeRenamed = true;
        //internal StringBuilder changeLog = new();
        internal List<WatcherLogEntry> changeLog = new();

        internal enum InterfaceMode
        {
            MainMenu,
            FileView,
            DirectoryView,
            SelectFile,
            SelectDirectory,
            Help,
            EventLog,
            END
        }
        internal InterfaceMode interfaceMode = InterfaceMode.MainMenu;
        internal InterfaceMode previousInterfaceMode = InterfaceMode.MainMenu;

        internal void Start()
        {
            int oldBufferWidth = Console.BufferWidth;
            int oldBufferHeight = Console.BufferHeight;

            ParseArguments(arguments);

            if (interfaceMode == InterfaceMode.FileView)
            {
                if (fileView.LoadFile(monitorTextFile, true) == false)
                {
                    interfaceMode = InterfaceMode.MainMenu;
                }
            }


            bool quit = false;
            DateTime lastViewUpdate = DateTime.Now;
            TimeSpan autoUpdateViewTime = TimeSpan.FromSeconds(300);

            while (quit is false)
            {
                // mouse, only gives screen coordinates, not column and row...
                /*if (Input.IsMouseButtonPressed(Input.MouseButton.LeftMouseButton))
                {
                    Debug.WriteLine($"Mouse pressed");
                    (int mx, int my) = Input.GetMousePosition();
                    Debug.WriteLine($"Mouse x:{mx} y:{my}");
                    Debug.WriteLine(Console.);
                }*/

                if (fileView.fileUpdateCountdown == 0) // the watcher has detected a change, wait for the file to complete, maybe there's two triggers
                {
                    if (interfaceMode == InterfaceMode.FileView) fileView.LoadFile(monitorTextFile, false, false); // Load file, don't set up watcher again, already active
                    fileView.forceFileUpdate = false;
                    fileView.fileUpdateCountdown = -1; // update countdown parked
                    updateViewRequested = true;
                }
                else if (fileView.forceFileUpdate)
                {
                    fileView.LoadFile(monitorTextFile, false);
                    fileView.forceFileUpdate = false;
                }

                if (fileView.fileUpdateCountdown > 0) fileView.fileUpdateCountdown--;

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
                    else if (keyInput.Key == ConsoleKey.H)
                    {
                        interfaceMode = InterfaceMode.Help;
                    }
                    else if (keyInput.Key == ConsoleKey.E)
                    {
                        if (interfaceMode != InterfaceMode.EventLog) previousInterfaceMode = interfaceMode;
                        interfaceMode = InterfaceMode.EventLog;
                    }
                    else if (keyInput.Key == ConsoleKey.S)
                    {
                        includeSubFolders = !includeSubFolders;
                        if (watcher != null)
                        {
                            watcher.IncludeSubdirectories = includeSubFolders;
                        }
                    }
                    //else if (keyInput.Key == ConsoleKey.X)
                    //{
                    //    if (watcher != null)
                    //    {
                    //        //watcher.Dispose();
                    //        watcher.IncludeSubdirectories = false;
                    //        Debug.WriteLine($"Disposing watcher");
                    //    }
                    //    else
                    //    {
                    //        Debug.WriteLine($"Watcher already null");
                    //    }
                    //}
                    else if (interfaceMode == InterfaceMode.FileView)
                    {
                        fileView.HandleFileViewKeys(keyInput);
                    }
                    else if (interfaceMode == InterfaceMode.DirectoryView)
                    {
                        directoryView.HandleDirectoryViewKeys(keyInput);
                    }
                    else if (interfaceMode == InterfaceMode.EventLog)
                    {
                        eventLogView.HandleEventLogViewKeys(keyInput);
                    }
                    else
                    {
                        Debug.WriteLine($"Key press with no action {keyInput.Key.ToString()} [Root loop]");
                    }



                        UpdateView();
                    lastViewUpdate = DateTime.Now;
                    Console.ResetColor();
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

        }

        bool checkAlertType(WatcherChangeTypes alertType)
        {
            if (alertChangeTypeAll && alertType == WatcherChangeTypes.All)
                return true;
            if (alertChangeTypeCreated && alertType == WatcherChangeTypes.Created)
                return true;
            if (alertChangeTypeChanged && alertType == WatcherChangeTypes.Changed)
                return true;
            if (alertChangeTypeDeleted && alertType == WatcherChangeTypes.Deleted)
                return true;
            if (alertChangeTypeRenamed && alertType == WatcherChangeTypes.Renamed)
                return true;
            return false;
        }

        public void ParseArguments(string[] arguments)
        {
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

                if (first == "-?" || first == "/?" || first == "/help" || first == "-help")
                {
                    Cosmetic.ShowHelp();
                    RevertConsoleColors();
                    Environment.Exit(0);
                }

                if (arguments[0].StartsWith('-') == false)
                {
                    string argFile = arguments[0];
                    arguments = ["-f", argFile];
                }
            }

            Dictionary<string, string?> argumentPairs = [];
            //Console.WriteLine($"Filling argument dictionary");
            for (int i = 0; i < arguments.Length; i++)
            {
                

                if (arguments[i].StartsWith('-')) // is an argument
                {
                    arguments[i] = arguments[i].ToLower();
                    // convert short-hand arguments
                    if (arguments[i] == "-f") arguments[i] = "-file";
                    if (arguments[i] == "-d") arguments[i] = "-directory";
                    if (arguments[i] == "-ft") arguments[i] = "-filter";
                    if (arguments[i] == "-s") arguments[i] = "-subdir";
                    if (arguments[i] == "-subdirectory") arguments[i] = "-subdir";

                    if (argumentPairs.ContainsKey(arguments[i]))
                    {
                        Console.WriteLine($"Argument {arguments[i]} already in list, skipping");
                        continue;
                    }
                    //Console.WriteLine($"Found argument {i}: {arguments[i]}");
                    if (arguments.Length > i+1) // has more arguments/values to index
                    {
                        
                        if (arguments[i + 1].StartsWith('-') == false)
                        {
                            //Console.WriteLine($"Found value {i+1}: {arguments[i+1]}");
                            argumentPairs.Add(arguments[i], arguments[i + 1]);
                            i++;
                        }
                        else
                        {
                            argumentPairs.Add(arguments[i], null);
                            //Console.WriteLine($"Next entry start with -, not a value pair");
                        }
                    }
                    else
                    {
                        argumentPairs.Add(arguments[i], null);
                        //Console.WriteLine($"Last entry in list, not a value pair");
                    }
                }
            }
            //Console.WriteLine($"List of arguments ({argumentPairs.Count})");
            foreach (var entry in argumentPairs)
            {
                bool overrideView = false;
                //Console.WriteLine($"   {entry.Key} : {entry.Value}");
                if (entry.Key == "-subdir")
                {
                    includeSubFolders = true;
                    if (watcher != null)
                    {
                        watcher.IncludeSubdirectories = includeSubFolders;
                    }
                }

                if (entry.Key == "-filter")
                {
                    if (entry.Value == null)
                    {
                        Console.WriteLine("Error: -filter used, but missing filter text value");
                        Environment.Exit(1);
                    }
                    monitorFilter = entry.Value;
                    if (watcher != null)
                    {
                        watcher.Filter = monitorFilter;
                    }
                }

                if (entry.Key == "-file")
                {
                    if (entry.Value == null)
                    {
                        Console.WriteLine("Error: -file used, but missing file path value");
                        Environment.Exit(1);
                    }
                    string path = Environment.ExpandEnvironmentVariables(entry.Value);
                    monitorTextFile = Path.GetFullPath(path);

                    if (File.Exists(monitorTextFile) == false)
                    {
                        Console.WriteLine($"File does not exist: {monitorTextFile}");
                        Environment.Exit(1);
                    }

                    monitorDirectory = Path.GetDirectoryName(monitorTextFile);
                    SetupWatcher(monitorDirectory, monitorTextFile, includeSubFolders);
                    if (!overrideView) interfaceMode = InterfaceMode.FileView;
                }
                
                if (entry.Key == "-directory")
                {
                    if (entry.Value == null)
                    {
                        Console.WriteLine("Error: -directory used, but missing directory path value");
                        Environment.Exit(1);
                    }
                    string path = entry.Value;
                    if (path.EndsWith('\\') == false) // ensure directory paths end with backslash
                    {
                        path += '\\';
                    }

                    path = Environment.ExpandEnvironmentVariables(path);

                    if (Directory.Exists(path) == false)
                    {
                        Console.WriteLine($"Directory does not exist: {path}");
                        Environment.Exit(1);
                    }

                    monitorTextFile = "";
                    monitorDirectory = path;
                    SetupWatcher(monitorDirectory, null, includeSubFolders);
                    if (!overrideView) interfaceMode = InterfaceMode.DirectoryView;
                }

                if (entry.Key == "-log")
                {
                    overrideView = true;
                    interfaceMode = InterfaceMode.EventLog;
                }
                
            }
        }





        internal void SetupWatcher(string? directory, string? filePath, bool includeSubDirectories)
        {
            if (filePath != null)
            {
                changeLog.Add(new WatcherLogEntry(WatcherLogEntry.EntryType.WatcherConfig, DateTime.Now, filePath, null, "File"));
            }
            else if (directory != null)
            {
                changeLog.Add(new WatcherLogEntry(WatcherLogEntry.EntryType.WatcherConfig, DateTime.Now, directory, null, "Directory"));
            }

            Debug.WriteLine($"SetupWatcher start, dir:{directory}, filePath:{filePath}");
            string fullpath;
            string folder;
            string filter;

            if (filePath is null || filePath == "")
            {
                if (directory is null)
                {
                    Debug.WriteLine("file is empty or null, and directory is null, return");
                    return;
                }
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
            if (watcher != null) watcher.Dispose();
            watcher = new(folder, filter)
            {
                NotifyFilter = NotifyFilters.Attributes
                          | NotifyFilters.CreationTime
                          | NotifyFilters.DirectoryName
                          | NotifyFilters.FileName
                          | NotifyFilters.LastAccess
                          | NotifyFilters.LastWrite
                          | NotifyFilters.Security
                          | NotifyFilters.Size,
                IncludeSubdirectories = includeSubDirectories
            };

            watcher.Changed += Watcher_OnChanged;
            watcher.Created += Watcher_OnChanged;
            watcher.Deleted += Watcher_OnChanged;
            watcher.Renamed += Watcher_OnChanged;
            watcher.Error += Watcher_OnError;

            //watcher.NotifyFilter = NotifyFilters.LastWrite;

            try
            {
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception opening item:\n{ex.Message}");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Clear();
                Console.WriteLine($"Exception opening item:\n{ex.Message}\n\nPress any key to continue.");
                Console.ResetColor();
                Console.ReadKey();
                watcher.Dispose();
                watcher = null;
                monitorDirectory = null;
                monitorTextFile = null;
            }
            //watcher.Filter = "*.*";
            Debug.WriteLine($"Watcher setup complete: {watcher is not null}, {watcher?.Path} {watcher?.Filter}");
        }

        private void Watcher_OnError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"Watcher error: {e.GetException().Message}");
        }

        private void UpdateView()
        {
            if (interfaceMode == InterfaceMode.FileView)
            {
                fileView.UpdateFileView();
            }
            else if (interfaceMode == InterfaceMode.DirectoryView)
            {
                directoryView.UpdateDirectoryView();
            }
            else if (interfaceMode == InterfaceMode.SelectFile)
            {
                fileView.OpenFileDialog();
            }
            else if (interfaceMode == InterfaceMode.SelectDirectory)
            {
                directoryView.OpenDirectoryDialog();
            }
            else if (interfaceMode == InterfaceMode.EventLog)
            {
                eventLogView.UpdateEvenLogView();
            }
            else if (interfaceMode == InterfaceMode.Help)
            {
                Cosmetic.SetColor(ConsoleColor.White, ConsoleColor.Black);
                Console.Clear();
                Console.WriteLine("");
                Cosmetic.ShowHelp();
                Console.WriteLine();
                Cosmetic.SetColor(ConsoleColor.Cyan);
                Console.WriteLine("Press any key to return to main menu");
                Console.ReadKey();
                interfaceMode = InterfaceMode.MainMenu;
            }
            else
            {
                interfaceMode = InterfaceMode.MainMenu;
                MainMenuView.UpdateMainMenuView(this);
            }
        }

        public static string DateAndTimeString(DateTime date)
        {
            return $"{date.ToShortDateString()} {date.ToShortTimeString()}";
        }

        string lastLogMessage = "";
        DateTime lastDing = DateTime.MinValue;

        private void Watcher_OnChanged(object sender, FileSystemEventArgs e)
        {
            //Debug.WriteLine($"File changed at {DateTime.Now.ToShortTimeString()}");
            //Debug.WriteLine($"   {((FileSystemWatcher)sender).Path} : {e.ChangeType} {e.Name}");

            //bool changeIsDirectory = false;
            WatcherLogEntry.EntryType entryType = WatcherLogEntry.EntryType.FileEvent;

            if (Directory.Exists(e.FullPath))
            {
                //changeIsDirectory = true;
                entryType = WatcherLogEntry.EntryType.DirectoryEvent;
            }

            string logMessage = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}: {e.ChangeType}, {e.Name}";
            if (logMessage != lastLogMessage) // deduplicate identical messages from spammy watcher
            {
                //changeLog.AppendLine(logMessage);
                changeLog.Add(new WatcherLogEntry(entryType, DateTime.Now, e.FullPath, e.ChangeType));
                //Debug.WriteLine($"Change type: {entryType.ToString()} : {e.FullPath}");
            }
            lastLogMessage = logMessage;

            if ((entryType == WatcherLogEntry.EntryType.DirectoryEvent && alertWhenDirectoryChanged) || (entryType == WatcherLogEntry.EntryType.FileEvent && alertWhenFileChanged))
            {
                if (checkAlertType(e.ChangeType) && DateTime.Now - lastDing > TimeSpan.FromSeconds(5)) // check if user wants ding for this type, and prevent dinging too often.
                {
                    Console.Write("\a"); // ding bell
                    lastDing = DateTime.Now;
                }
            }

            fileView.fileUpdateCountdown = 10; // wait a bit before opening the file, it might still be held by the save process. Prevents double triggering of file load if watcher event fires twice
        }

        void RevertConsoleColors()
        {
            Console.ForegroundColor = defaultForeColor;
            Console.BackgroundColor = defaultBackColor;
        }
    }
}
