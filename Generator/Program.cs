using System.Diagnostics;

namespace Generator
{
    class Program
    {
        public static void Main(string[] args) 
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            DataBase.GenerateDataBase(10000).Send();
            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            Console.WriteLine("Время выполнения: {0} секунд, {1} миллисекунд", elapsed.Seconds, elapsed.Milliseconds);
        }
    }
}