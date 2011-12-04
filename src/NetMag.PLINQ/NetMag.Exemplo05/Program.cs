using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMag.Exemplo05
{
    class Program
    {
        static void Main(string[] args)
        {

            int[] source = new int[100000];
            Random rand = new Random();

            for (int x = 0; x < source.Length; x++)
            {
                source[x] = rand.Next(10, 20);
            }

            double mean = source.AsParallel().Average();

            double standardDev = source.AsParallel().Aggregate(
                 0.0,
                 (subtotal, item) => subtotal + Math.Pow((item - mean), 2),
                 (total, thisThread) => total + thisThread,
                 (finalSum) => Math.Sqrt((finalSum / (source.Length - 1)))
            );

            Console.WriteLine("Mean value is = {0}", mean);
            Console.WriteLine("Standard deviation is {0}", standardDev);
            Console.ReadLine();

        }
    }
}
