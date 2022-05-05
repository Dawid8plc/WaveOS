using Cosmos.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveOS.Apps
{
    public class RAMTest : WaveApp
    {
        public override void Initialize()
        {
            Name = "RAM Test";
        }

        public override int Run()
        {
            WaveShell.WriteLine(
                "\nMemory Used: " + Cosmos.Core.GCImplementation.GetUsedRAM() +
                "\nMemory Free: " + Cosmos.Core.GCImplementation.GetAvailableRAM() +
                "\nTotal Memory: " + (Cosmos.Core.GCImplementation.GetAvailableRAM() + Cosmos.Core.GCImplementation.GetUsedRAM()));

            return 0;
        }
    }
}
