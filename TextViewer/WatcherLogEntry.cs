namespace TextViewer
{
    public class WatcherLogEntry
    {
        public enum EntryType
        {
            FileEvent,
            DirectoryEvent,
            WatcherConfig,
            Information
        }

        public EntryType entryType;
        public string path;
        public string? oldPath;
        public string? name;
        public string? oldName;
        public WatcherChangeTypes? watcherChangeType;
        public DateTime time;
        public string info = "";

        public WatcherLogEntry(EntryType _entryType, DateTime _time, string _path, string? _name, WatcherChangeTypes? _watcherChangeType, string _info = "", string? _oldPath = null, string? _oldName = null)
        {
            entryType = _entryType;
            path = _path;
            watcherChangeType = _watcherChangeType;
            time = _time;
            info = _info;
            oldPath = _oldPath;
            name = _name;
            oldName = _oldName;
        }
    }
}