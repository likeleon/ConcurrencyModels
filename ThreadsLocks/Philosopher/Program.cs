using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Philosopher
{
    class Program
    {
        static void Main(string[] args)
        {
            int numPhilosophers = 5;
            var chopsticks = Generate(numPhilosophers, i => new Chopstick(i)).ToArray();
            var philosophers = Generate(numPhilosophers, i => new Philosopher(i, chopsticks[i], chopsticks[(i + 1) % numPhilosophers])).ToArray();
            //var philosophers = Generate(count, i => new PhilosopherFixed(i, chopsticks[i], chopsticks[(i + 1) % count])).ToArray();
            var threads = Generate(numPhilosophers, i => new Thread(() => philosophers[i].Run())).ToList();
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());
        }

        static IEnumerable<T> Generate<T>(int count, Func<int, T> generator)
        {
            for (int i = 0; i < count; ++i)
                yield return generator(i);
        }
    }

    class Philosopher
    {
        protected readonly int _index;
        protected readonly Chopstick _left;
        protected readonly Chopstick _right;
        protected readonly Random _random;

        public Philosopher(int index, Chopstick left, Chopstick right)
        {
            _index = index;
            _left = left;
            _right = right;
            _random = new Random(_index);
        }

        public void Run(bool tryDeadlock = true)
        {
            while (true)
                Loop(tryDeadlock);
        }

        protected virtual void Loop(bool tryDeadlock)
        {
            Think();
            lock (_left)
            {
                Console.WriteLine($"{_index} got {_left}");

                if (tryDeadlock)
                    Think(1000);

                lock (_right)
                {
                    Console.WriteLine($"{_index} got {_right}");
                    Eat();
                }
            }
        }

        protected void Think(int? forceDuration = null)
        {
            var duration = forceDuration ?? _random.Next(1000);
            Console.WriteLine($"{_index} start thinking {duration}ms");
            Thread.Sleep(duration);
            Console.WriteLine($"{_index} ended thinking");
        }

        protected void Eat()
        {
            var duration = _random.Next(1000);
            Console.WriteLine($"{_index} start eating {duration}ms");
            Thread.Sleep(duration);
            Console.WriteLine($"{_index} ended eating {duration}ms");
        }
    }

    class PhilosopherFixed : Philosopher
    {
        readonly Chopstick _first;
        readonly Chopstick _second;

        public PhilosopherFixed(int index, Chopstick left, Chopstick right)
           : base(index, left, right)
        {
            if (left.Index < right.Index)
            {
                _first = left;
                _second = right;
            }
            else
            {
                _first = right;
                _second = left;
            }
        }

        protected override void Loop(bool tryDeadlock)
        {
            Think();
            lock (_first)
            {
                Console.WriteLine($"{_index} got {_first}");

                if (tryDeadlock)
                    Think(1000);

                lock (_second)
                {
                    Console.WriteLine($"{_index} got {_second}");
                    Eat();
                }
            }

        }
    }

    class Chopstick
    {
        public int Index { get; }

        public Chopstick(int index)
        {
            Index = index;
        }

        public override string ToString() => Index.ToString();
    }
}
