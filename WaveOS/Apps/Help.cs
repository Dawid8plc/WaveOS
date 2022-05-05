using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveOS.Apps
{
    public class Help : WaveApp
    {
        public override void Initialize()
        {
            Name = "WaveOS Help";
        }

        public override int Run()
        {
            //Header
            Console.ForegroundColor = ConsoleColor.White;
            WaveShell.Write("-- Wave");
            Console.ForegroundColor = ConsoleColor.Cyan;
            WaveShell.Write("OS");
            Console.ForegroundColor = ConsoleColor.White;
            WaveShell.WriteLine(" Help --");

            WaveShell.WriteLine(@"Apps:
Help - Displays this message
HelloWorld [input] - Prints Hello World! And echoes input
Canvas - Test GUI Environment

Commands:
neofetch - Shows OS info
clear - Clears console
pause - Pauses execution and waits for user input
cd <dir> - Enters the directory
echo <input> - Echoes input
cauto - Creates autoexec.ws file
rm <file> - Removes file
cat <file> - Reads file
reboot - Reboots OS
shutdown - Shutdowns PC");

            return 0;
        }
    }
}
