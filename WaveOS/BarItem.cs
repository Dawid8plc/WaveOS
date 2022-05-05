using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveOS
{
    public class BarItem
    {
        public string Title;
    }

    public class BarButton : BarItem
    {
        public Action Action;
    }
}
