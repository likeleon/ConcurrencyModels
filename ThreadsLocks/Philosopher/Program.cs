using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Philosopher
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    class Philosopher
    {
        readonly Chopstick _left;
        readonly Chopstick _right;
        readonly Random _random = new Random();

        public Philosopher(Chopstick left, Chopstick right)
        {
            _left = left;
            _right = right;
        }

        public void Run()
        {
        }
    }

    class Chopstick
    {
    }
}
