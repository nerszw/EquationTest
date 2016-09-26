using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using EquationLibrary;

namespace EquationConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var equation = "2x ^ 2xy + x (3.5 xy + y) = y^2 - xy + y y";
            //canon: 2 x y + 3.5 x^2 y + 2 x^3 y - 2 y^2 = 0

            Console.WriteLine("in: " + equation);
            Console.WriteLine("out: " + Equation.ToCanonicalForm(equation));

            while (true)
            {
                var eq = Equation.ToCanonicalForm(Console.ReadLine());
                Console.WriteLine(eq);
            }
        }
    }
}
