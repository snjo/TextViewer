using System.Diagnostics;
using System.Drawing;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Text;

namespace TextViewer
{
    class EventLogView(TextViewerLoop parent)
    {
        int scroll = 0;
        private readonly TextScroller scroller = new();

        internal void UpdateEvenLogView()
        {
            Console.Clear();
            string directory = "";
            if (parent.monitorDirectory != null)
            {
                directory = Path.GetFullPath(parent.monitorDirectory);
            }
            string dingDir = parent.alertWhenDirectoryChanged ? "🔔 Dir" : "🔕 Dir";
            string dingFile = parent.alertWhenFileChanged ? "🔔 File" : "🔕 File";
            string wathcSubDir = parent.includeSubFolders ? "📂+" : "📁.";
            //string wathcSubDir = parent.watcher.IncludeSubdirectories ? "📂+" : "📁."; // test
            Cosmetic.ShowTitleBar($"Event Log : {directory}".PadRight(Console.BufferWidth - 22) + $"{wathcSubDir} " + $"{dingDir}  {dingFile}");
            //Console.WriteLine(parent.changeLog.ToString());
            Console.WriteLine("Date".PadRight(12) + "Time".PadRight(10) + "Entry type".PadRight(16) + "Information".PadRight(16) + "Change type".PadRight(16) + "Path");

            scroller.Height = Console.BufferHeight - 6;
            AssembleScrollableText();
            scroller.OutputLines(null);

            Console.WriteLine("");
            Console.WriteLine(" [Esc] Menu  [Backspace] Return to previous screen  [B] Toggle Bell ding  [↓↑] scroll [PgUp PgDn Shift+↓↑] scroll 10");
        }

        private void AssembleScrollableText()
        {
            scroller.ResetLines();
            try
            {
                foreach (WatcherLogEntry entry in parent.changeLog)
                {
                    scroller.SetColor(Color.Blue);
                    scroller.AddTextToLine($"{entry.time.ToShortDateString(),-12}{entry.time.ToLongTimeString(),-10}");
                    scroller.SetColor(Color.Cyan);
                    scroller.AddTextToLine($"{entry.entryType,-16}{entry.info,-16}");
                    scroller.SetColor(Color.LightGreen);
                    string additional = "";
                    if (entry.watcherChangeType == WatcherChangeTypes.Renamed)
                    {
                        additional = $" (was {entry.oldName})";
                    }
                    string line = $"{entry.watcherChangeType,-16}{TerminalCodes.ForegroundWhite}{entry.path}{additional}";
                    int maxLength = Console.BufferWidth - scroller.CurrentLineLength;
                    //Debug.WriteLine($"Buffer {Console.BufferWidth} - scrollerL {scroller.CurrentLineLength} = {maxLength}");
                    if (line.Length > maxLength)
                    {
                        line = line.Substring(0, maxLength +4) + "…"; // +4 to compensate for a hidden color tag in the line
                    }
                    scroller.FinishLine(line, true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception during AssembleScrollable text:\n{ex.Message}");
            }
        }

        internal void HandleEventLogViewKeys(ConsoleKeyInfo keyInput)
        {
            int pageScroll = scroller.PageHeight;//Math.Max(10, Console.BufferHeight - 6);

            if (keyInput.Key == ConsoleKey.Backspace)
            {
                parent.interfaceMode = parent.previousInterfaceMode;
            }
            else if (keyInput.Key == ConsoleKey.B)
            {
                ToggleAlertModes();
                parent.updateViewRequested = true;
            }
            else if (keyInput.Key == ConsoleKey.J)
            {
                // test log entries
                parent.changeLog.Add(new WatcherLogEntry(WatcherLogEntry.EntryType.Information, DateTime.Now, "No file", null, WatcherChangeTypes.All, "test enry" + DateTime.Now.Microsecond));
            }
            else if (keyInput.Key == ConsoleKey.PageDown || (keyInput.Key == ConsoleKey.DownArrow && keyInput.Modifiers == ConsoleModifiers.Shift))
            {
                scroller.ChangeScroll(pageScroll);
            }
            else if (keyInput.Key == ConsoleKey.PageUp || (keyInput.Key == ConsoleKey.UpArrow && keyInput.Modifiers == ConsoleModifiers.Shift))
            {
                scroller.ChangeScroll(-pageScroll);
            }
            else if (keyInput.Key == ConsoleKey.DownArrow)
            {
                scroller.ChangeScroll(1);
            }
            else if (keyInput.Key == ConsoleKey.UpArrow)
            {
                scroller.ChangeScroll(-1);
            }
            else if (keyInput.Key == ConsoleKey.End)
            {
                scroller.ScrollToEnd();
            }
            else if (keyInput.Key == ConsoleKey.Home)
            {
                scroller.ScrollToBeginning();
            }
            else if (keyInput.Key == ConsoleKey.X)
            {
                Debug.WriteLine($"Export log");
                ExportEventLogToFile();
            }

        }

        private void ExportEventLogToFile()
        {
            StringBuilder logLines = new();
            foreach (var entry in parent.changeLog)
            {
                logLines.Append($"{entry.time.ToShortDateString(),-12}{entry.time.ToLongTimeString(),-10}");

                logLines.Append($"{entry.entryType,-16}{entry.info,-16}");
                
                string additional = "";
                if (entry.watcherChangeType == WatcherChangeTypes.Renamed)
                {
                    additional = $" (was {entry.oldName})";
                }
                logLines.Append($"{entry.watcherChangeType,-16}{entry.path}{additional}");
                logLines.AppendLine();
            }
            File.WriteAllText($"EventLog {DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}.txt", logLines.ToString());
        }

        void AddToScroll(int change)
        {
            scroll += change;
            if (scroll >= parent.changeLog.Count) scroll = parent.changeLog.Count - 1;
            if (scroll < 0) scroll = 0;
            Debug.WriteLine($"scroll: {scroll}");
        }

        private void ToggleAlertModes()
        {
            if (parent.alertWhenDirectoryChanged && parent.alertWhenFileChanged)
            {
                parent.alertWhenDirectoryChanged = false;
                parent.alertWhenFileChanged = false;
            }
            else if (!parent.alertWhenDirectoryChanged && !parent.alertWhenFileChanged)
            {
                parent.alertWhenDirectoryChanged = true;
                parent.alertWhenFileChanged = false;
            }
            else if (parent.alertWhenDirectoryChanged && !parent.alertWhenFileChanged)
            {
                parent.alertWhenDirectoryChanged = false;
                parent.alertWhenFileChanged = true;
            }
            else if (!parent.alertWhenDirectoryChanged && parent.alertWhenFileChanged)
            {
                parent.alertWhenDirectoryChanged = true;
                parent.alertWhenFileChanged = true;
            }
        }
    }
}
