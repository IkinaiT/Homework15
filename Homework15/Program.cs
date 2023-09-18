using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Homework15
{
    internal class Program
    {
        private static int _completedActions = 0;
        private static object _objBuildLocker = new object();
        private static bool _aborter = true;

        static void Main(string[] args)
        {
            #region Log thread

            Thread logThread = new Thread(() =>
            {
                Thread.CurrentThread.Name = "log";

                while (true)
                {
                    Thread.Sleep(1000);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " " + Thread.CurrentThread.Name + "  " + "Completed actions: " + _completedActions);
                }
            });
            logThread.IsBackground = true;
            logThread.Start();

            #endregion

            #region Lock threads

            Thread building1 = new Thread(BuildMethod);
            Thread building2 = new Thread(BuildMethod);

            building1.Name = "Building 1";
            building2.Name = "Building 2";

            building1.Start(4);
            building2.Start();

            #endregion

            #region Long action

            Task longActionThread = Task.Run(() =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Long action started (10 sec)");
                Thread.Sleep(10000);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Long action ended");
            });


            #endregion

            #region Main thread

            Thread.CurrentThread.Name = "main";

            for(int i = 0; i < 30;  i++)
            {
                Console.ForegroundColor = ConsoleColor.Gray;

                _completedActions++;
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " " + Thread.CurrentThread.Name + "  " + ": Main action " + (i + 1));
                Thread.Sleep(250);
            }

            #endregion

            longActionThread.Wait();

            #region Synchronyzed threads

            Thread st1 = new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < 20; i++)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " " + Thread.CurrentThread.Name + "  " + ": Action" + (i + 1));
                        Thread.Sleep(200);
                    }
                }
                catch (ThreadAbortException ex)
                {
                    if(!_aborter)
                        Thread.ResetAbort();
                }
                
            })
            { Name = "Synchronyzed thread 1" };

            Thread st2 = new Thread(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " " + Thread.CurrentThread.Name + "  " + ": Action" + (i + 1));
                    Thread.Sleep(200);
                    if (i == 5)
                    {
                        st1.Abort();
                    }
                }
                _aborter = false;
            })
            { Name = "Synchronyzed thread 2" };

            st1.Start();
            st2.Start();

            #endregion

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Main end");
        }

        /// <summary>
        /// Method for lock threads
        /// </summary>
        private static void BuildMethod(object speed)
        {
            lock (_objBuildLocker)
            {
                int i = 0;
                while(true) 
                {
                    Thread.Sleep(100);

                    if (speed is int _speed)
                        i += _speed;
                    else
                        i += 10;
                    
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " " + Thread.CurrentThread.Name + "  " + $"Build progress: {i}%");

                    if(i >= 100)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine("Build ended");
                        break;
                    }

                    _completedActions++;
                }
            }
        }
    }
}
