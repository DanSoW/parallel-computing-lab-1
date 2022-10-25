using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Program
{
    internal class Program
    {
        static int N = 1000;             // Общее количество значений массива
        static int[]? a = null;          // Значения массива
        static int n;                    // Количество запущенных потоков
        static int[]? returns = null;    // Возвращаемые значения

        /// <summary>
        /// Функция выполнения для потока
        /// </summary>
        /// <param name="param">Параметры функции</param>
        /// <returns>Сумма чётных элементов массива</returns>
        static int ThreadFunc(int param)
        {
            int nt, beg, h, end;
            int sum = 0;

            nt = param;
            h = N / n;

            // Вычисление начала и конца обхода массива
            beg = h * nt;
            end = beg + h;

            if (nt == (n - 1))
            {
                end = N;
            }

            Console.WriteLine(
                "Поток {0} начало {1} конец {2}",
                nt, beg, end
            );

            for(int i = beg; i < end; i++)
            {
                if (a[i] % 2 != 0)
                {
                    sum += a[i];
                }
            }

            return sum;
        }

        /// <summary>
        /// Точка входа в приложение
        /// </summary>
        static void Main()
        {
            Console.WriteLine("Введите количество потоков, используемых для вычислений в текущем процессе: ");
            while (true)
            {
                if ((Int32.TryParse(Console.ReadLine(), out n)) && (n > 0))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Ошибка ввода! Необходимо ввести натуральное число отличное от нуля!");
                }
            }

            Random rand = new Random();
            N = rand.Next(100000, 1000000);
            a = new int[N];

            // Объект мониторинга времени выполнения
            Stopwatch sWatch = new Stopwatch();

            // Массив потоков
            Thread[] threads = new Thread[n];

            int rez = 0, s = 0;

            for (int i = 0; i < a.Length; i++)
            {
                a[i] = rand.Next(100, 10000);
            }

            // Начало отсчёта времени выполнения операций
            sWatch.Start();

            returns = new int[n];

            for (int i = 0; i < n; i++)
            {
                // Необходима локальная переменная, чтобы
                // потоки не ссылались на переменную i в цикле
                int tmp = i;
                returns[tmp] = 0;

                threads[i] = new Thread(() =>
                {
                    // Возвращаемое значение для определённого потока
                    returns[tmp] = ThreadFunc(tmp);
                });

                // Запуск потока
                threads[i].Start();
            }

            // Ожидание всех потоков до конца их выполнения
            for (int i = 0; i < n; i++)
            {
                threads[i].Join();
                rez += returns[i];

                Console.WriteLine("Сумма {0}-го потока: {1}", i, returns[i]);
            }

            sWatch.Stop();
            Console.WriteLine("Затраченное время в миллисекундах: " + sWatch.ElapsedMilliseconds.ToString());
            Console.WriteLine("Сумма чётных элементов массива {0} потоками = {1}", n, rez);
            sWatch.Reset();

            sWatch.Start();
            for(int i = 0; i < N; i++)
            {
                if (a[i] % 2 != 0)
                {
                    s += a[i];
                }
            }
            sWatch.Stop();
            Console.WriteLine("Затраченное время в миллисекундах: " + sWatch.ElapsedMilliseconds.ToString());
            Console.WriteLine("Сумма чётных элементов массива без потоков = {0}", s);

            Console.ReadLine();
        }
    }
}