using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Ellanet
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.DoEvents();
            Application.Run(new SystemTrayIcon());
            //todo Create bullet point documentation and have link on app
            //todo Simple and advanced mode
        }
    }
}
