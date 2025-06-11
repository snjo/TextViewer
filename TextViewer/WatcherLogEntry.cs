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
        public WatcherChangeTypes? watcherChangeType;
        public DateTime time;
        public string info = "";
        
        public WatcherLogEntry(EntryType _entryType, DateTime _time, string _path, WatcherChangeTypes? _watcherChangeType, string _info = "")
        {
            entryType = _entryType;
            path = _path;
            watcherChangeType = _watcherChangeType;
            time = _time;
            info = _info;
        }
    }
}