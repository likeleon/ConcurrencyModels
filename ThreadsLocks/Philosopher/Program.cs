using System;
using System.Collections.Generic;
using System.Threading;

namespace Philosopher
{
    class Program
    {
        static void Main(string[] args)
        {
            int numSeats = 5;
            var table = new Table(numSeats, Create<PhilosopherMonitor>);

            var threads = new Thread[table.Philosophers.Length];
            foreach (var p in table.Philosophers)
            {
                threads[p.Index] = new Thread(() => p.Run());
                threads[p.Index].Start();
            }
            foreach (var t in threads)
                t.Join();
        }

        static Philosopher Create<T>(int index, Table table) where T : Philosopher
        {
            var constructor = typeof(T).GetConstructor(new[] { typeof(int), typeof(Table) });
            return constructor.Invoke(new object[] { index, table }) as Philosopher;
        }

        static IEnumerable<T> Generate<T>(int count, Func<int, T> generator)
        {
            for (int i = 0; i < count; ++i)
                yield return generator(i);
        }
    }

    class Table
    {
        public Chopstick[] Chopsticks { get; }
        public Philosopher[] Philosophers { get; }

        public Table(int numSeats, Func<int, Table, Philosopher> philosopherCreator)
        {
            Chopsticks = new Chopstick[numSeats];
            for (int i = 0; i < numSeats; ++i)
                Chopsticks[i] = new Chopstick(i);

            Philosophers = new Philosopher[numSeats];
            for (int i = 0; i < numSeats; ++i)
                Philosophers[i] = philosopherCreator(i, this);
            foreach (var philosopher in Philosophers)
                philosopher.AllPhilosophersBeSeated();
        }
    }

    class Philosopher
    {
        public int Index { get; }
        protected Table Table { get; }
        protected readonly Chopstick _left;
        protected readonly Chopstick _right;
        readonly Random _random;

        public Philosopher(int index, Table table)
        {
            Index = index;
            Table = table;
            _left = table.Chopsticks[index];
            _right = table.Chopsticks[(index + 1) % table.Chopsticks.Length];
            _random = new Random(Index);
        }

        public virtual void AllPhilosophersBeSeated()
        {
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
                Console.WriteLine($"{Index} got {_left}");

                if (tryDeadlock)
                    Think(1000);

                lock (_right)
                {
                    Console.WriteLine($"{Index} got {_right}");
                    Eat();
                }
            }
        }

        protected void Think(int? forceDuration = null)
        {
            var duration = forceDuration ?? _random.Next(1000);
            Console.WriteLine($"{Index} start thinking {duration}ms");
            Thread.Sleep(duration);
            Console.WriteLine($"{Index} ended thinking");
        }

        protected void Eat()
        {
            var duration = _random.Next(1000);
            Console.WriteLine($"{Index} start eating {duration}ms");
            Thread.Sleep(duration);
            Console.WriteLine($"{Index} ended eating");
        }
    }

    class PhilosopherFixed : Philosopher
    {
        readonly Chopstick _first;
        readonly Chopstick _second;

        public PhilosopherFixed(int index, Table table)
           : base(index, table)
        {
            if (_left.Index < _right.Index)
            {
                _first = _left;
                _second = _right;
            }
            else
            {
                _first = _right;
                _second = _left;
            }
        }

        protected override void Loop(bool tryDeadlock)
        {
            Think();
            lock (_first)
            {
                Console.WriteLine($"{Index} got {_first}");

                if (tryDeadlock)
                    Think(1000);

                lock (_second)
                {
                    Console.WriteLine($"{Index} got {_second}");
                    Eat();
                }
            }
        }
    }

    class PhilosopherTimeout : Philosopher
    {
        public PhilosopherTimeout(int index, Table table) 
            : base(index, table)
        {
        }

        protected override void Loop(bool tryDeadlock)
        {
            Think();
            Monitor.Enter(_left);
            try
            {
                Console.WriteLine($"{Index} got {_left}");

                if (tryDeadlock)
                    Think(1000);

                if (Monitor.TryEnter(_right, TimeSpan.FromSeconds(1)))
                {
                    Console.WriteLine($"{Index} got {_right}");
                    try
                    {
                        Eat();
                    }
                    finally
                    {
                        Monitor.Exit(_right);
                        Console.WriteLine($"{Index} released {_right}");
                    }
                }
            }
            finally
            {
                Monitor.Exit(_left);
                Console.WriteLine($"{Index} released {_left}");
            }
        }
    }

    class PhilosopherMonitor : Philosopher
    {
        public bool Eating { get; private set; }

        PhilosopherMonitor _leftPhilosopher;
        PhilosopherMonitor _rightPhilosopher;

        public PhilosopherMonitor(int index, Table table)
            : base(index, table)
        {
        }

        public override void AllPhilosophersBeSeated()
        {
            _leftPhilosopher = Table.Philosophers[(Index + 4) % Table.Philosophers.Length] as PhilosopherMonitor;
            _rightPhilosopher = Table.Philosophers[(Index + 1) % Table.Philosophers.Length] as PhilosopherMonitor;
        }

        protected override void Loop(bool tryDeadlock)
        {
            Monitor.Enter(Table);
            try
            {
                Eating = false;
                Monitor.Pulse(Table);
            }
            finally
            {
                Monitor.Exit(Table);
            }
            Think();

            Monitor.Enter(Table);
            try
            {
                while (_leftPhilosopher.Eating || _rightPhilosopher.Eating)
                    Monitor.Wait(Table);

                Eating = true;
            }
            finally
            {
                Monitor.Exit(Table);
            }
            Eat();
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
