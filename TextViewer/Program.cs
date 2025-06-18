using System.Text;

namespace TextViewer;

class Program
{
    static void Main(string[] args)
    {
        //string argfull = Environment.CommandLine;
        //Console.WriteLine(argfull);
        //Console.ReadLine();
        Console.OutputEncoding = Encoding.Unicode;
        TextViewerLoop loop = new(args);
        loop.Start();
    }
}
