using System;

namespace CatalogSynchronizer
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 4)
            {
                throw new ArgumentException("Received invalid data");
            }

            var catalogSynchronizer = new CatalogSynchronizer(args[0], args[1], int.Parse(args[2]), args[3]);

            Console.WriteLine("Start");
            catalogSynchronizer.SynchronizeAsync();

            Console.WriteLine("Enter -exit to close the program");

            string command;

            do
            {
                command = Console.ReadLine();
            } while (command != "-exit");
        }
    }
}
