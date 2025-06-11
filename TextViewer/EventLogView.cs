using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TextViewer
{
    class EventLogView (TextViewerLoop parent)
    {
        int scroll = 0;

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
            Cosmetic.ShowTitleBar($"Event Log : {directory}".PadRight(Console.BufferWidth-25) + $"{dingDir}  {dingFile}");
            //Console.WriteLine(parent.changeLog.ToString());
            if (scroll > 0)
            {
                Console.WriteLine(" ⮝ ⮝ ⮝ ⮝ ⮝ ⮝ ");
            }
            int count = 0;
            for (int i = scroll; i < parent.changeLog.Count && i < Console.BufferHeight + scroll - 6; i++)
            {
                WatcherLogEntry entry = parent.changeLog[i];
                Console.WriteLine($"{entry.time.ToShortDateString()} {entry.time.ToLongTimeString()}, {entry.entryType,-10}: {entry.info} {entry.watcherChangeType} {entry.path}");
                count = i;
            }
            if (count < parent.changeLog.Count-1)
            {
                Console.WriteLine(" ⮟ ⮟ ⮟ ⮟ ⮟ ⮟ ");
            }
            Console.WriteLine("");
            Console.WriteLine(" [Esc] Menu  [Backspace] Return to previous screen  [B] Toggle Bell ding  [↓↑] scroll [PgUp PgDn Shift+↓↑] scroll 10");
        }

        internal void HandleEventLogViewKeys(ConsoleKeyInfo keyInput)
        {
            int pageScroll = Math.Max(10, Console.BufferHeight - 6);

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
                parent.changeLog.Add(new WatcherLogEntry(WatcherLogEntry.EntryType.Information, DateTime.Now, "", null, "test enry" + DateTime.Now.Microsecond));
            }
            else if (keyInput.Key == ConsoleKey.PageDown || (keyInput.Key == ConsoleKey.DownArrow && keyInput.Modifiers == ConsoleModifiers.Shift))
            {
                AddToScroll(pageScroll);
                Debug.WriteLine($"page scroll: {pageScroll}");
            }
            else if (keyInput.Key == ConsoleKey.PageUp || (keyInput.Key == ConsoleKey.UpArrow && keyInput.Modifiers == ConsoleModifiers.Shift))
            {
                AddToScroll(-pageScroll);
                Debug.WriteLine($"page scroll: {pageScroll}");
            }
            else if (keyInput.Key == ConsoleKey.DownArrow)
            {
                AddToScroll(1);
            }
            else if (keyInput.Key == ConsoleKey.UpArrow)
            {
                AddToScroll(-1);
            }
            else if (keyInput.Key == ConsoleKey.End)
            {
                scroll = parent.changeLog.Count - 1;
            }
            else if (keyInput.Key == ConsoleKey.Home)
            {
                scroll = 0;
            }

        }

        void AddToScroll(int change)
        {
            scroll += change;
            if (scroll >= parent.changeLog.Count) scroll = parent.changeLog.Count-1;
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
