using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class Program
{
    private static Populacao populacao;

    private static IEnumerable<Cidadao> seqQuery;
    private static ParallelQuery<Cidadao> parQuery;

    const int anoInicial = 1960;
    const int anoFinal = 2000;

    static void Main()
    {
        Console.Write("Criando a população... ");
        CriaPopulacao(1000000);
        Console.WriteLine("População criada.");
        Console.WriteLine();

        ComparaLINQcomPLINQ();
        
        CausarUmaException();
    }

    static void CriaPopulacao(int total)
    {
        var ano = 1900;
        var random = new Random();
        var cidadaos = new List<Cidadao>();

        for (int c = 0; c < total; c++)
        {
            ano = random.Next(1900, DateTime.Now.Year);
            cidadaos.Add(new Cidadao("Cidadão " + ano.ToString(), ano));
        }

        populacao = new Populacao(cidadaos);
    }

    static void ComparaLINQcomPLINQ()
    {
        for (; ; )
        {
            Stopwatch stopWatch;

            // Contabilizando o tempo com LINQ
            stopWatch = Stopwatch.StartNew();
            seqQuery = from n in populacao.Cidadaos
                       where n.AnoNascimento >= anoInicial
                            && n.AnoNascimento <= anoFinal
                       orderby n.AnoNascimento ascending
                       select n;
            Console.WriteLine("Encontrados {0} registros em {1} ms com LINQ",
                seqQuery.Count(), stopWatch.ElapsedMilliseconds);

            // Contabilizando o tempo com PLINQ
            stopWatch = Stopwatch.StartNew();
            parQuery = from n in populacao.Cidadaos
                           .AsParallel()
                           .WithMergeOptions(ParallelMergeOptions.FullyBuffered)
                           .WithDegreeOfParallelism(2)
                           .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                       where n.AnoNascimento >= anoInicial
                            && n.AnoNascimento <= anoFinal
                       orderby n.AnoNascimento ascending
                       select n;
            Console.WriteLine("Encontrados {0} registros em {1} ms com PLINQ",
                parQuery.Count(), stopWatch.ElapsedMilliseconds);

            Console.ReadLine();
        }
    }

    static void CausarUmaException()
    {
        List<Cidadao> cidadaos = populacao.Cidadaos;

        // Alterando um nome para causar a exception
        cidadaos[54].Nome = "12345678";

        var parlQuery = from n in cidadaos.AsParallel()
                        let campos = n.Nome.Split(' ')
                        where
                            /* Essa condição do Where
                             * vai criar uma exception do
                             * tipo "Index out of range"
                             */
                            campos[1].StartsWith("C")
                        select new
                        {
                            PrimeiroNome = campos[0],
                            thread = Thread.CurrentThread.ManagedThreadId
                        };

        try
        {
            parlQuery.ForAll(e =>
                Console.WriteLine("Primeiro nome: {0}, Thread:{1}",
                e.PrimeiroNome, e.thread));
        }
        catch (AggregateException e)
        {
            foreach (var ex in e.InnerExceptions)
            {
                Console.WriteLine(ex.Message);
                if (ex is IndexOutOfRangeException)
                    Console.WriteLine("Um erro ocorreu, a query foi finalizada!");
            }
        }

        Console.ReadLine();
    }

    static void CancelarUmaConsulta()
    {
        CancellationTokenSource cTokenSource = new CancellationTokenSource();

        parQuery = from n in populacao.Cidadaos
                           .AsParallel()
                           .WithCancellation(cTokenSource.Token)
                   where n.AnoNascimento >= anoInicial
                        && n.AnoNascimento <= anoFinal
                   orderby n.AnoNascimento ascending
                   select n;

        /* O código abaixo deve ser usado
         * quando se deseja cancelar a execução
         * de uma query PLINQ
         */
        cTokenSource.Cancel();
    }
}

class Populacao
{
    public List<Cidadao> Cidadaos { get; private set; }

    public Populacao(List<Cidadao> cidadaos)
    {
        Cidadaos = cidadaos;
    }
}

class Cidadao
{
    public string Nome { get; set; }
    public int AnoNascimento { get; private set; }

    public Cidadao(string nome, int anoNascimento)
    {
        Nome = nome;
        AnoNascimento = anoNascimento;
    }
}