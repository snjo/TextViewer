using System.Drawing;

namespace TextViewer
{
    internal class Cosmetic
    {
        public static void SetColor(ConsoleColor foreground, ConsoleColor? background = null)
        {
            Console.ForegroundColor = foreground;
            if (background is not null)
                Console.BackgroundColor = (ConsoleColor)background;
        }

        public static Color GetColorFromAge(DateTime time)
        {
            TimeSpan timeSinceFileWrite = DateTime.Now - time;

            TimeSpan JustHappened = TimeSpan.FromMinutes(2);
            TimeSpan MinutesAgo = TimeSpan.FromMinutes(15);
            TimeSpan HourAgo = TimeSpan.FromHours(1);
            TimeSpan DayAgo = TimeSpan.FromDays(1);
            TimeSpan WeekAgo = TimeSpan.FromDays(7);
            TimeSpan MonthAgo = TimeSpan.FromDays(30);

            if (timeSinceFileWrite < JustHappened)
            {
                return Color.Yellow;
            }
            else if (timeSinceFileWrite < MinutesAgo)
            {
                return Color.GreenYellow;
            }
            else if (timeSinceFileWrite < HourAgo)
            {
                return Color.Green;
            }
            else if (timeSinceFileWrite < DayAgo)
            {
                return Color.Blue;
            }
            else if (timeSinceFileWrite < WeekAgo)
            {
                return Color.Magenta;
            }
            else if (timeSinceFileWrite < MonthAgo)
            {
                return Color.Purple;
            }
            else
            {
                return Color.DarkGray;
            }
        }

        public static void SetColorFromAge(DateTime time)
        {
            TimeSpan timeSinceFileWrite = DateTime.Now - time;

            TimeSpan JustHappened = TimeSpan.FromMinutes(2);
            TimeSpan MinutesAgo = TimeSpan.FromMinutes(15);
            TimeSpan HourAgo = TimeSpan.FromHours(1);
            TimeSpan DayAgo = TimeSpan.FromDays(1);
            TimeSpan WeekAgo = TimeSpan.FromDays(7);
            TimeSpan MonthAgo = TimeSpan.FromDays(30);

            if (timeSinceFileWrite < JustHappened)
            {
                SetColor(ConsoleColor.Yellow);
            }
            else if (timeSinceFileWrite < MinutesAgo)
            {
                SetColor(ConsoleColor.DarkYellow);
            }
            else if (timeSinceFileWrite < HourAgo)
            {
                SetColor(ConsoleColor.Blue);
            }
            else if (timeSinceFileWrite < DayAgo)
            {
                SetColor(ConsoleColor.DarkBlue);
            }
            else if (timeSinceFileWrite < WeekAgo)
            {
                SetColor(ConsoleColor.DarkMagenta);
            }
            else if (timeSinceFileWrite < MonthAgo)
            {
                SetColor(ConsoleColor.DarkRed);
            }
            else
            {
                SetColor(ConsoleColor.DarkGray);
            }
        }

        internal static void ShowHelp()
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

        internal static void ShowTitleBar(string title)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            Cosmetic.SetColor(ConsoleColor.Black, ConsoleColor.Cyan);
            Console.WriteLine($" {title}".PadRight(Console.BufferWidth));
            Console.WriteLine();
            Cosmetic.SetColor(ConsoleColor.White, ConsoleColor.Black);
        }

    }
}
