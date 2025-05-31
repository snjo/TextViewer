
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Quic;
using System.Runtime;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TextViewer;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.Unicode;
        TextViewerLoop loop = new (args);
        loop.Update();
    }
}
