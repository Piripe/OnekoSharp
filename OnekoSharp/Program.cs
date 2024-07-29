using System;
using System.Windows.Forms;

namespace OnekoSharp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Oneko());
        }
    }
}
