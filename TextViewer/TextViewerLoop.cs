using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextViewer
{
    public class TextViewerLoop
    {
        readonly FileView fileView;
        readonly DirectoryView directoryView;
        readonly string[] arguments;

        public TextViewerLoop(string[] args)
        {
            arguments = args;
            fileView = new(this);
            directoryView = new(this);
        }

        private readonly ConsoleColor defaultForeColor = Console.ForegroundColor;
        private readonly ConsoleColor defaultBackColor = Console.BackgroundColor;
        
        internal string? monitorTextFile = null;
        internal string? monitorDirectory = null;
        internal string monitorFilter = "*.*";
        internal bool updateViewRequested = true;
        internal FileSystemWatcher? watcher;



        internal enum InterfaceMode
        {
            MainMenu,
            FileView,
            DirectoryView,
            SelectFile,
            SelectDirectory,
            Help,
            END
        }
        internal InterfaceMode interfaceMode = InterfaceMode.MainMenu;

        internal void Update()
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
            TimeSpan autoUpdateViewTime = TimeSpan.FromSeconds(30);

            while (quit is false)
            {
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
                    else if (interfaceMode == InterfaceMode.FileView)
                    {
                        fileView.HandleFileViewKeys(keyInput);
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

                if (first == "-?" || first == "/?")
                {
                    Cosmetic.ShowHelp();
                    RevertConsoleColors();
                    Environment.Exit(0);
                }

                string argFile = arguments[0];
                arguments = ["-f", argFile];
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
                }
                return;
            }
        }

        

        

        internal void SetupWatcher(string? directory, string? filePath)
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
            watcher = new(folder, filter)
            {
                NotifyFilter = NotifyFilters.Attributes
                          | NotifyFilters.CreationTime
                          | NotifyFilters.DirectoryName
                          | NotifyFilters.FileName
                          | NotifyFilters.LastAccess
                          | NotifyFilters.LastWrite
                          | NotifyFilters.Security
                          | NotifyFilters.Size
            };

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
                MainMenuView.UpdateMainMenuView();
            }
        }        

        public static string DateAndTimeString(DateTime date)
        {
            return $"{date.ToShortDateString()} {date.ToShortTimeString()}";
        }

        private void Watcher_OnChanged(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"File changed at {DateTime.Now.ToShortTimeString()}");
            Debug.WriteLine($"   {((FileSystemWatcher)sender).Path} : {e.ChangeType} {e.Name}");

            fileView.fileUpdateCountdown = 10; // wait a bit before opening the file, it might still be held by the save process. Prevents double triggering of file load if watcher event fires twice
        }

        void RevertConsoleColors()
        {
            Console.ForegroundColor = defaultForeColor;
            Console.BackgroundColor = defaultBackColor;
        }

    }
}
