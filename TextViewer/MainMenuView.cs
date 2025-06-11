using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TextViewer
{
    internal class MainMenuView
    {
        internal static void UpdateMainMenuView()
        {
            Console.ResetColor();
            Cosmetic.ShowTitleBar("TextViewer");
            Console.Write(" F   ");
            Cosmetic.SetColor(ConsoleColor.Cyan);
            Console.WriteLine("Open File");

            Cosmetic.SetColor(ConsoleColor.White);
            Console.Write(" D   ");
            Cosmetic.SetColor(ConsoleColor.Cyan);
            Console.WriteLine("Open Directory");

            Cosmetic.SetColor(ConsoleColor.White);
            Console.Write(" Q   ");
            Cosmetic.SetColor(ConsoleColor.Cyan);
            Console.WriteLine("Quit");
        }
    }
}
