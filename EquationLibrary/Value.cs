using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EquationLibrary
{
    class Variable
    {
        public int Degree;
        public char Symbol;

        public override string ToString()
        {
            return (Degree == 1) ? Symbol.ToString() :
                String.Format("{0}^{1}", Symbol.ToString(), Degree);
        }
    }

    class Value
    {
        public static string Reg
        {
            get
            {
                // regex: number variable[] {symbol ^ degree} 
                var numReg = @"(\s*(?<num>\d+(\.\d+)?)\s*)";
                var symReg = @"(\s*(?<ivs>[a-z,A-Z])\s*)";
                var intReg = @"(\s*(?<ivs>(\-)?\d+)\s*)";
                var varReg = String.Format(@"({0}(?:\^{1})?)", symReg, intReg);
                return String.Format(@"(?n)(?<value>({0}{1}*)|({1}+))", numReg, varReg);
            }
        }

        public double Number { get; private set; }
        public List<Variable> Variables { get; private set; }

        public Value(double number = 0, List<Variable> variables = null)
        {
            Number = number;
            Variables = variables ?? new List<Variable>();
        }

        public static Value Zero
        {
            get { return new Value(); }
        }

        public override string ToString()
        {
            var build = new StringBuilder();
            if (Number != 1 || Variables.Count == 0)
            {
                build.Append(Number.ToString(CultureInfo.InvariantCulture));
            }
            Variables.ForEach(x => build.Append(x));
            return build.ToString();
        }

        public static Value Parse(string input)
        {
            return Parse(Regex.Match(input, Reg));
        }

        public static Value Parse(Match match)
        {
            if (!match.Groups["value"].Success)
                throw new FormatException();

            var result = new Value();

            var gnum = match.Groups["num"];
            result.Number = gnum.Length <= 0 ? 
                1 : double.Parse(gnum.Value, CultureInfo.InvariantCulture);

            var ivs = match.Groups["ivs"].Captures.Cast<Capture>().ToList();
            foreach (var iv in ivs.Select(x => x.Value))
            {
                int degree;
                if (Int32.TryParse(iv, out degree))
                {
                    result.Variables.Last().Degree = degree;
                }
                else
                {
                    result.Variables.Add(new Variable()
                    {
                        Degree = 1,
                        Symbol = iv[0]
                    });
                }
            }

            return result;
        }

        public static Value operator *(Value v1, Value v2)
        {
            var result = new Value()
            {
                Number = v1.Number * v2.Number,
                Variables = v1.Variables.Union(v2.Variables).ToList()
            };

            return result.Normalize();
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj == null && obj is Value)
        //        return false;

        //    var value = (Value)obj;
        //    return value.Number == Number && IsSameVariables(this, value);
        //}

        public static bool IsSameVariables(Value v1, Value v2)
        {
            var vars1 = NormalizeVaritables(v1.Variables);
            var vars2 = NormalizeVaritables(v2.Variables);
            
            if (vars1.Count == vars2.Count)
            {
                if (vars1.Count > 0)
                {
                    return vars1.All(x => vars2.Any(y => 
                        x.Degree == y.Degree && x.Symbol == y.Symbol));
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public Value Normalize()
        {
            var variables = (Number == 0) ? new List<Variable>() 
                : NormalizeVaritables(Variables);

            return new Value()
            {
                Number = Number,
                Variables = variables
            };
        }

        private static List<Variable> NormalizeVaritables(List<Variable> vs)
        {
            //Убираем повторы переменных, складывая их степени
            var unique = vs.GroupBy(x => x.Symbol).Select(x => new Variable()
            {
                Symbol = x.Key,
                Degree = x.Sum(y => y.Degree)
            });

            //Убираем переменные с нулевой степенью
            return unique.Where(x => x.Degree != 0).ToList();
        }
    }
}
