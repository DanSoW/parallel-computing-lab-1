using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Pipes;

namespace Program
{
    internal class Program
    {
        static Mutex mut = new Mutex();
        static public Stream outStream;
        static public Stream inStream;
        public static AnonymousPipeServerStream pipeServer;
        public static AnonymousPipeClientStream pipeClient;
        static public string pipeHandle;
        static int mode = 0;
        static public int i = 0;
        public static float sredn = 0, dlina = 0;
        static bool stop = false;

        public static void Second()
        {
            pipeClient = new AnonymousPipeClientStream(PipeDirection.In, pipeHandle);

            string? buffer = "";
            StreamReader binReader = new StreamReader(pipeClient);
            while (!stop)
            {
                buffer = binReader.ReadLine();
                mut.WaitOne();
                int tmp = mode;
                Console.WriteLine("Message read ({0}): {1}",DateTime.Now, buffer);
                if (tmp == 1)
                {
                    Console.WriteLine("Обработка строки");
                    if (buffer != null)
                    {
                        string[] msgRcv = buffer.Split(';');
                        int index = msgRcv[1].IndexOf(msgRcv[0]);

                        if(index >= 0)
                        {
                            string result = msgRcv[1].Substring(0, (index + 1))
                                + msgRcv[2]
                                + msgRcv[1].Substring((index + 1));
                            Console.WriteLine("Результат: " + result);
                        }else
                        {
                            Console.WriteLine("Данный символ отсутствует в целевой строке!");
                        }
                    }
                }
                else if (tmp == 2)
                {
                    Console.WriteLine("Обработка чисел ... ");
                    int counter = 0;

                    if (buffer != null)
                    {
                        string[] msgRcv = buffer.Split(' ');
                        int[] values = new int[msgRcv.Length];

                        for (int j = 0; j < values.Length; j++)
                        {
                            if (int.TryParse(msgRcv[j], out _))
                            {
                                values[j] = Convert.ToInt32(msgRcv[j]);
                                if ((values[j] > 0) && (j % 2 != 0))
                                {
                                    counter++;
                                }
                            }
                        }
                    }

                    Console.WriteLine("Количество положительных чисел в массиве находящиеся на чётных индексах равно {0}", counter);
                }
                else
                {
                    Console.WriteLine("Shutting down!");
                }
                mut.ReleaseMutex();
            }
            binReader.Close();
        }

        public static void Main()
        {
            Thread th2 = new Thread(() => Second()); // Создание нового потока
            pipeServer = new AnonymousPipeServerStream(PipeDirection.Out);
            outStream = pipeServer;
            pipeHandle = pipeServer.GetClientHandleAsString();
            th2.Start();
            StreamWriter writer = new StreamWriter(outStream);
            writer.AutoFlush = true;

            while (!stop)
            {
                Console.WriteLine("Введите цифру 1 для того что бы выполнить задание с буквенной строкой");
                Console.WriteLine("Введите цифру 2 для того что бы выполнить задание с массивом");
                Console.WriteLine("Введите цифру 3 для того что бы завершить выполнение программы");

                int com;
                while (true)
                {
                    if ((Int32.TryParse(Console.ReadLine(), out com)) && ((com == 1) || (com == 2) || (com == 3)))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Ошибка ввода! Необходимо ввести натуральное число в диапазоне [1; 3]");
                    }
                }

                mut.WaitOne();
                mode = com;
                switch (com)
                {
                    case 1:
                        Console.WriteLine("Введите буквенную строку 1");
                        string? stroka1 = Console.ReadLine();

                        Console.WriteLine("Введите буквенную строку 2");
                        string? stroka2 = Console.ReadLine();

                        Console.WriteLine("Введите символ");
                        char? symbol1 = Console.ReadLine()[0];

                        string tmpStr = symbol1 + ";" + stroka1 + ";" + stroka2;
                        writer.WriteLine(tmpStr);
                        break;
                    case 2:
                        Console.WriteLine("Введите элементы массива через пробел");
                        string? stroka3 = Console.ReadLine();
                        writer.WriteLine(stroka3);
                        break;
                    case 3:
                        stop = true;
                        break;
                }
                mut.ReleaseMutex();
            }
            writer.Close();

            th2.Join();
        }
    }
}