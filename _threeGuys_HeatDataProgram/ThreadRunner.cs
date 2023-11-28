using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadRunner
{
    class ThreadRunner
    {
        public static void ThreadStart(Action threadFunction)
        {
            Thread thread = new Thread(() => threadFunction());
            thread.Start();
        }
    }
}

