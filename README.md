Monitors a file or directory for changes and highlights any recently edited files. Updates file contents when modified.

## Command line arguments

TEXTVIEWER [file]        (Auto-converts to -file PATH)
TEXTVIEWER -file PATH [-log]
TEXTVIEWER -directory PATH [-filter FILTER] [-subdir] [-log]

Parameters:
-file -f        Select file to open
-directory -d   Select directory to open
-filter         Use file filter (default is *.*)
-subdir         Watch subfolders in event log
-log            Open the event log view

Example:
textviewer -directory c:\temp -filter *.txt    Don't use backslash at the end of a directory argument
textviewer -directory "c:\Program Files"    Don't use backslash at the end of a directory argument
textviewer "c:\tmp\file.txt"
textviewer -file "c:\tmp\file.txt"

## Navigation

| Key         | Function                                |
|-------------|-----------------------------------------|
| Esc         | Main menu                               |
| F           | Open File                               |
| D           | Open Directory                          |
| S           | Toggle Subfolder events in Event Log    |
| Q / Ctrl+C  | Quit                                    |
| Enter       | Open File / Directory                   |
| Backspace   | Go to previous view or parent directory |
| P           | Toggle text/image preview in directory  |
| F5          | Refresh                                 |
| E           | Open Event log                          |
| L           | Goto line (in Text Viewer)              |
| Up/Down     | Scroll                                  |
| PgUp/PgDn   | Scroll 10                               |
| Shift+Up/Dn | Scroll 10                               |
| B           | Cycle bell ding audio in Event log      |
