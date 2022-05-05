using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveOS.Apps
{
    public class HelloWorld : WaveApp
    {
        public override void Initialize()
        {
            Name = "Hello World!";

            barItems.Add("Test");
        }

        public override int Run()
        {
            WaveShell.WriteLine("Hello World!");
            if(args.Length > 0)
                WaveShell.WriteLine("Echoing " + args[0]);

            WaveShell.WriteLine("Press anything to exit...");
            Console.ReadKey();
            return 0;
        }
    }
}
