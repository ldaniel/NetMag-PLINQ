using System;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        Stopwatch stopWatch;

        for (; ; )
        {
            // Contabilizando o tempo do FOR
            stopWatch = Stopwatch.StartNew();
            for (int c = 0; c < 1000000; c++)
            {
                VerificaNumeroPrimo(c);
            }
            System.Console.WriteLine("Tempo com FOR.........: "
                + stopWatch.ElapsedMilliseconds + " ms");
            
            // Contabilizando o tempo do PARALLEL FOR
            stopWatch = Stopwatch.StartNew();
            Parallel.For(0, 1000000, numero =>
            {
                VerificaNumeroPrimo(numero);
            });
            System.Console.WriteLine("Tempo com PARALLEL FOR: "
                + stopWatch.ElapsedMilliseconds + " ms");
            
            System.Console.ReadLine();
        }
    }

    static bool VerificaNumeroPrimo(int numero)
    {
        if (numero == 2) return true;

        if (numero < 2 || (numero & 1) == 0) return false;

        int upperBound = (int)Math.Sqrt(numero);

        for (int i = 3; i < upperBound; i += 2)
        {
            if ((numero % i) == 0) return false;
        }

        return true;
    }
}