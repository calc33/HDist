using System;

namespace HCopy
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
                Console.WriteLine(t.ToString());
                Environment.Exit(1);
            }
        }
    }
}
