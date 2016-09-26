using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationLibrary
{
    public static class Equation
    {
        public static string ToCanonicalForm(string equation)
        {
            var exps = equation.Split('=');

            if (exps.Length != 2)
                throw new FormatException("Number of expressions is not equal to 2");
            
            var exp = string.Format("({0}) - ({1})", exps[0], exps[1]);
            return string.Format("{0} = 0", Expression.FromString(exp).Simplify());
        }
    }
}
