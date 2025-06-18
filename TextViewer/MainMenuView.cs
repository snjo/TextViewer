namespace TextViewer
{
    internal class MainMenuView
    {
        internal static void UpdateMainMenuView()
        {
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


            //Cosmetic.SetColor(ConsoleColor.Yellow);
            //Console.WriteLine("Fresh");
            //Cosmetic.SetColor(ConsoleColor.DarkYellow);
            //Console.WriteLine("Newish");
            //Cosmetic.SetColor(ConsoleColor.Blue);
            //Console.WriteLine("Kinda");
            //Cosmetic.SetColor(ConsoleColor.DarkBlue);
            //Console.WriteLine("Old");
            //Cosmetic.SetColor(ConsoleColor.DarkGray);
            //Console.WriteLine("Oldest");
        }
    }
}
