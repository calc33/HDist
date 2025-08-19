using System;
using System.Threading.Tasks;

namespace HCopy
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				MainModule module = new(args);
				Task task = module.RunAsync();
				task.Wait();
				if (task.IsFaulted)
				{
					foreach (Exception ex in task.Exception.InnerExceptions)
					{
						if (ex is ApplicationException)
						{
							Console.WriteLine(ex.Message);
						}
						else
						{
							Console.WriteLine(ex.ToString());
						}
					}
					Environment.Exit(1);
				}
			}
			catch (ApplicationException t)
			{
				Console.WriteLine(t.Message);
				Environment.Exit(1);
			}
			catch (Exception t)
			{
				Console.WriteLine(t.ToString());
				Environment.Exit(1);
			}
		}
	}
}
