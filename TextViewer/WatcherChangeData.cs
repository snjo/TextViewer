using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextViewer
{
    internal class WatcherChangeData
    {
        string NewPath;
        string? OldPath;
        WatcherChangeTypes ChangeType;
        string? NewName;
        string? OldName;

        internal WatcherChangeData(FileSystemEventArgs e)
        {
            NewPath = e.FullPath;
            OldPath = null;
            NewName = e.Name;
            OldName = null;
            ChangeType = e.ChangeType;
        }

        internal WatcherChangeData(RenamedEventArgs e)
        {
            NewPath = e.FullPath;
            OldPath = e.OldFullPath;
            NewName = e.Name;
            OldName = e.OldName;
            ChangeType = e.ChangeType;
        }
    }
}
