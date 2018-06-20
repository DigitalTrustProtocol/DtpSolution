using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest
{
    public class DisposeableScope : IDisposable
    {
        public void Dispose()
        {
            Console.WriteLine("DisposeableScope is being disposed!");
        }
    }
}
