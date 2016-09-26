using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using EquationLibrary;


namespace EquationConsole
{
    class Program
    {
        static string GetEquation(string eq)
        {
            try
            {
                return Equation.ToCanonicalForm(eq);
            }
            catch (Exception exc)
            {
                return exc.Message;
            }
        }

        static void Main(string[] args)
        {
            bool withFile = args.Length > 0 && File.Exists(args[0]);
            
            try
            {
                if (withFile)
                {
                    Console.WriteLine("File mode => " + args[0]);

                    var lines = File.ReadAllLines(args[0]);
                    var canons = lines.Select(x => GetEquation(x));
                    File.WriteAllLines(args[0] + ".out", canons);
                }
                else
                {
                    Console.WriteLine("Console mode ");

                    while (true)
                    {
                        Console.WriteLine("Write equation: ");
                        var equation = Console.ReadLine();

                        Console.WriteLine("Canonical form: ");
                        Console.WriteLine(GetEquation(equation));
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                Console.ReadKey();
            }
        }
    }
}
