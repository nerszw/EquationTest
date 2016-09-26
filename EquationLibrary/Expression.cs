using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationLibrary
{
    class Expression
    {
        private List<IToken> RpnTokens;
        
        public Expression Simplify()
        {
            return new Expression()
            {
                RpnTokens = Simpler.Simplify(RpnTokens)
            };
        }

        public override string ToString()
        {
            var stack = new Stack<Tuple<string, int>>();
            foreach (var token in RpnTokens)
            {
                if (token.Key == "v")
                {
                    var value = (Token<Value>) token;
                    stack.Push(new Tuple<string, int>(value.ToString(), int.MaxValue));
                }
                else
                {
                    var right = stack.Pop();
                    var left = stack.Pop();

                    var operation = token.Key;
                    var priority = GetPriority(token.Key);
                    
                    var expRight = right.Item1;
                    if (right.Item2 < priority || (right.Item2 == priority && operation == "-"))
                        expRight = "(" + expRight + ")";

                    var expLeft = left.Item1;
                    if (left.Item2 < priority)
                        expLeft = "(" + expLeft + ")";
                    
                    var exp = token.Key == "*" ? expLeft + " " + expRight :
                            expLeft + " " + operation + " " + expRight;
                    
                    stack.Push(new Tuple<string, int>(exp, priority));
                }
            }

            return stack.First().Item1;
        }

        public static Expression FromString(string input)
        {
            var tokenizer = new Tokenizer();
            tokenizer.AddRule("(", @"^\s*\(\s*");
            tokenizer.AddRule(")", @"^\s*\)\s*");
            tokenizer.AddRule("+", @"^\s*\+\s*");
            tokenizer.AddRule("-", @"^\s*\-\s*");
            tokenizer.AddRule("v", Value.Reg, (k, m) =>
            {
                return new Token<Value>()
                {
                    Key = k,
                    Value = Value.Parse(m)
                };
            });

            //Разбивка на токены
            var tokens = tokenizer.Parse(input); 

            //Проверка выражения на валидность
            if (!IsExpValid(tokens.Select(x => x.Key).ToList()))
                throw new FormatException("Expression is incorrect");

            //Добавление 0-v и операции умножить
            var fullForm = ToCorrectInfix(tokens);

            return new Expression()
            {
                //Преобразовываем в ОПН
                RpnTokens = ToRpn(fullForm)
            };
        }

        private static List<IToken> ToCorrectInfix(IList<IToken> tokens)
        {
            var result = new List<IToken>();

            var p = "(";
            for (int i = 0; i < tokens.Count; ++i)
            {
                var k = tokens[i].Key;

                //за скобкой идёт знак то добавляем 0
                if (p == "(" && (k == "+" || k == "-"))
                {
                    result.Add(new Token<Value>()
                    {
                        Key = "v",
                        Value = Value.Zero
                    });
                }

                //за зак. скобкой идёт только операция
                if ((p == "v" && k == "(") || (p == ")" && k == "v"))
                {
                    result.Add(new Token<string>()
                    {
                        Key = "*",
                        Value = "*"
                    });
                }

                result.Add(tokens[i]);
                p = k;
            }

            return result;
        }
        
        /// <summary>
        /// Преобразование из инфиксной нотации
        /// (алгоритм Дейкстра).
        /// </summary>
        private static List<IToken> ToRpn(IList<IToken> tokens)
        {
            var res = new List<IToken>();
            var stack = new Stack<IToken>();

            foreach (var token in tokens)
            {
                var k = token.Key;

                if (k == "v")
                {
                    res.Add(token);
                }
                else if (k == "(")
                {
                    stack.Push(token);
                }
                else if (k == ")")
                {
                    //Выталкиваем термы в результат
                    while (stack.Peek().Key != "(")
                        res.Add(stack.Pop());

                    //Отбрасываем отк. скобку
                    stack.Pop();
                }
                else if (k == "+" || k == "-" || k == "*")
                {
                    var priority = GetPriority(k);
                    
                    //Выталкиваем термы в результат
                    while (stack.Count != 0 &&
                        GetPriority(stack.Peek().Key) >= priority)
                    {
                        res.Add(stack.Pop());
                    }
                
                    stack.Push(token);
                }
                else
                {
                    throw new FormatException("Token not found");
                }
            }

            while (stack.Count != 0)
                res.Add(stack.Pop());

            return res;
        }

        private static int GetPriority(string key)
        {
            if (key == "-" || key == "+")
                return 1;
            else if (key == "*")
                return 2;
            else
                return 0;
        }

        /// <summary>
        /// Валидация первоначального выражения
        /// </summary>
        private static bool IsExpValid(IList<string> keys)
        {
            if (!IsBracketsValid(keys))
                return false;

            var p = "(";     
            foreach (var k in keys)
            {
                //за отк. скобкой идёт только значение или операция или отк. скобка
                var m1 = (p == "(") && (k == "v" || k == "+" || k == "-" || k == "(");
                
                //за зак. скобкой идёт только операция или закр. скобка
                var m2 = (p == ")") && (k == "+" || k == "-" || k == ")");

                //за значением только скобки или операция
                var m3 = (p == "v") && (k == "(" || k == ")" || k == "+" || k == "-");

                //за операцией только открывающаяся скобка или значение
                var m4 = (p == "+" || p == "-") && (k == "v" || k == "(");

                var valid = m1 || m2 || m3 || m4;

                if (!valid)
                    return false;

                p = k;
            }

            return true;
        }
        
        private static bool IsBracketsValid(IList<string> keys)
        {
            int count = 0;
            foreach(var key in keys)
            {
                if (key == "(")
                {
                    count++;
                }
                else if (key == ")")
                {
                    count--;
                }

                if (count < 0)
                {
                    break;
                }
            }
            return count == 0;
        }
    }
}
