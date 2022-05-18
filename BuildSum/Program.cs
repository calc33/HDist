using System;

namespace BuildSum
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MainModule module = new MainModule(args);
                module.Run();
            }
            catch (Exception t)
            {
                Console.Error.WriteLine(t.ToString());
            }
        }
    }
}
