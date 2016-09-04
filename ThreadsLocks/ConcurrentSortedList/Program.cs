using System;
using System.Linq;
using System.Threading;

namespace ConcurrentSortedList
{
    class Program
    {
        static void Main(string[] args)
        {
            var list = new ConcurrentSortedList();
            //var list = new SynchronizedSortedList();
            
            var random = new Random();

            var testThreads = Enumerable.Range(0, 2).Select(_ => new Thread(() =>
            {
                for (int j = 0; j < 10000; ++j)
                    list.Insert(random.Next());
            })).ToList();

            var countingThread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        Thread.Sleep(0);
                        Console.WriteLine($"\r{list.Size()}");
                        Console.Out.FlushAsync();
                    }
                }
                catch (ThreadInterruptedException)
                {
                }
            });

            var start = DateTime.Now;
            testThreads.ForEach(t => t.Start());
            countingThread.Start();

            testThreads.ForEach(t => t.Join());
            countingThread.Interrupt();
            Console.WriteLine($"\rElapsed: {DateTime.Now - start}");

            Console.WriteLine($"\r{list.Size()}");

            if (list.Size() != 20000)
                Console.WriteLine("*** Wrong size!");

            if (!list.IsSorted())
                Console.WriteLine("*** Not sorted!");
        }

        class Node
        {
            public int Value { get; }
            public Node Prev { get; set; }
            public Node Next { get; set; }

            public Node()
            {
            }

            public Node(int value, Node prev, Node next)
            {
                Value = value;
                Prev = prev;
                Next = next;
            }
        }

        class SynchronizedSortedList
        {
            readonly Node _head = new Node();
            readonly Node _tail = new Node();
            readonly object _lock = new object();

            public SynchronizedSortedList()
            {
                _head.Next = _tail;
                _tail.Prev = _head;
            }

            public void Insert(int value)
            {
                lock (_lock)
                {
                    var current = _head;
                    var next = current.Next;
                    while (true)
                    {
                        if (next == _tail || next.Value < value)
                        {
                            var node = new Node(value, current, next);
                            next.Prev = node;
                            current.Next = node;
                            return;
                        }
                        current = next;
                        next = current.Next;
                    }
                }
            }

            public int Size()
            {
                lock (_lock)
                {
                    var current = _tail;
                    int count = 0;

                    while (current.Prev != _head)
                    {
                        ++count;
                        current = current.Prev;
                    }
                    return count;
                }
            }

            public bool IsSorted()
            {
                var current = _head;
                while (current.Next.Next != _tail)
                {
                    current = current.Next;
                    if (current.Value < current.Next.Value)
                        return false;
                }
                return true;
            }
        }

        class ConcurrentSortedList
        {
            readonly Node _head = new Node();
            readonly Node _tail = new Node();

            public ConcurrentSortedList()
            {
                _head.Next = _tail;
                _tail.Prev = _head;
            }

            // Buggy.
            public void Insert(int value)
            {
                var current = _head;
                Monitor.Enter(current);
                var next = current.Next;
                try
                {
                    while (true)
                    {
                        Monitor.Enter(next);
                        try
                        {
                            if (next == _tail || next.Value < value)
                            {
                                var node = new Node(value, current, next);
                                next.Prev = node;
                                current.Next = node;
                                return;
                            }
                        }
                        finally
                        {
                            Monitor.Exit(current);
                        }
                        current = next;
                        next = current.Next;
                    }
                }
                finally
                {
                    if (Monitor.IsEntered(next))
                        Monitor.Exit(next);
                }
            }

            public int Size()
            {
                var current = _tail;
                int count = 0;

                while (current.Prev != _head)
                {
                    var monitorTarget = current;
                    Monitor.Enter(monitorTarget);
                    try
                    {
                        ++count;
                        current = current.Prev;
                    }
                    finally
                    {
                        Monitor.Exit(monitorTarget);
                    }
                }
                return count;
            }

            public bool IsSorted()
            {
                var current = _head;
                while (current.Next.Next != _tail)
                {
                    current = current.Next;
                    if (current.Value < current.Next.Value)
                        return false;
                }
                return true;
            }
        }
    }
}
