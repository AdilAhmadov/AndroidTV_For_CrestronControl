using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualConsoleApp;

namespace AndroidTvControl.Hellpers
{
    public class Debug
    {
        public static void PrintLine(string message)
        {
            if (InitialParametersClass.ControllerPromptName == "VC-4")
            {
                VirtualConsole.Send("VC: " + message);
            }
            else
            {
                CrestronConsole.PrintLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " || " + message);
            }
        }
        public static void PrintLine(string message, bool isDebugEnabled)
        {
            if (isDebugEnabled)
            {
                CrestronConsole.PrintLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " || " + message);
                ErrorLog.Warn(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " || " + message);
                VirtualConsole.Send("Virtual Console " + "|| " + message);
            }
            else
            {
                CrestronConsole.PrintLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " || " + message);
                VirtualConsole.Send("Virtual Console " + "|| " + message);
            }
        }
    }
}
