using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationLibrary
{
    class Simpler
    {
        class GVal
        {
            private List<Value> Values;

            public GVal(List<Value> values)
            {
                Values = values.ToList();
            }

            public GVal(Value value)
            {
                Values = new List<Value>();
                Values.Add(value);
            }

            public static GVal operator *(GVal v1, GVal v2)
            {
                var values = new List<Value>();
                foreach (var x1 in v1.Values)
                    foreach (var x2 in v2.Values)
                        values.Add(x1 * x2);

                return new GVal(values);
            }

            public static GVal operator +(GVal v1, GVal v2)
            {
                var values = v1.Values.Union(v2.Values);
                return new GVal(values.ToList());
            }

            public static GVal operator -(GVal v1, GVal v2)
            {
                var vs = v2.Values.Select(x => x * new Value(-1));
                var reverseV2 = new GVal(vs.ToList());

                return v1 + reverseV2;
            }

            private void Simplify()
            {
                var result = new List<Value>();
                var source = new List<Value>(Values);

                while (source.Count > 0)
                {
                    var v = source.First();
                    source.Remove(v);

                    for (int i = source.Count - 1; i > -1; --i)
                    {
                        var vn = source[i];
                        
                        if (Value.IsSameVariables(vn, v))
                        {
                            var num = v.Number + vn.Number;
                            v = new Value(num, v.Variables).Normalize();

                            source.RemoveAt(i);
                        }
                    }

                    result.Add(v);
                }
                
                //Отсечение нулевых значений
                result = result.Where(x => x.Number != 0).ToList();

                if (result.Count == 0)
                    result.Add(Value.Zero);

                Values = result;
            }

            public List<IToken> ToRpn()
            {
                Simplify();
                
                var result = new List<IToken>();

                var sv = Values.First();
                if (sv.Number < 0)
                {
                    var vt = new Token<Value>() { Key = "v", Value = Value.Zero };
                    result.Add(vt);
                }
                
                foreach (var v in Values)
                {
                    if (result.Count > 0)
                    {
                        var xv = new Value(Math.Abs(v.Number), v.Variables);
                        var vt = new Token<Value>() { Key = "v", Value = xv };
                        result.Add(vt);

                        var op = v.Number < 0 ? "-" : "+";
                        var ot = new Token<string>() { Key = op, Value = op };
                        result.Add(ot);
                    }
                    else
                    {
                        var xv = new Value(Math.Abs(v.Number), v.Variables);
                        var vt = new Token<Value>() { Key = "v", Value = xv };
                        result.Add(vt);
                    }
                }

                return result;
            }
        }

        public static List<IToken> Simplify(IList<IToken> tokens)
        {
            var stack = new Stack<GVal>();
            foreach (var token in tokens)
            {
                if (token.Key == "v")
                {
                    var vt = (Token<Value>) token;
                    stack.Push(new GVal(vt.Value));
                }
                else
                {
                    var r = stack.Pop();
                    var l = stack.Pop();

                    if (token.Key == "*")
                    {
                        stack.Push(l * r);
                    }
                    else if (token.Key == "+")
                    {
                        stack.Push(l + r);
                    }
                    else if (token.Key == "-")
                    {
                        stack.Push(l - r);
                    }
                }
            }

            return stack.First().ToRpn();
        }
    }
}
