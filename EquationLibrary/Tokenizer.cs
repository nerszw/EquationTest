using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace EquationLibrary
{
    interface IToken
    {
        /// <summary>
        /// Имя/тип/ключ токена
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Значение
        /// </summary>
        object GetValue { get; }
    }

    class Token<T> : IToken
    {
        public string Key { get; set; }
        public T Value { get; set; }

        public object GetValue
        {
            get { return Value; }
        }

        public override string ToString()
        {
            return Value.ToString() ?? base.ToString();
        }
    }

    class Tokenizer
    {
        class Rule
        {
            public Func<string, Match, IToken> ParseT;
            public string Pattern;
            public string Key;
        }

        private List<Rule> Rules;

        public Tokenizer()
        {
            Rules = new List<Rule>();
        }

        public void AddRule(string key, string pattern, Func<string, Match, IToken> parseT = null)
        {
            var defaultParse = new Func<string, Match, IToken>((k, m) =>
            {
                return new Token<string>()
                {
                    Key = k,
                    Value = m.Value
                };
            });

            Rules.Add(new Rule()
            {
                Key = key,
                Pattern = pattern,
                ParseT = parseT ?? defaultParse
            });
        }

        public IList<IToken> Parse(string input)
        {
            var result = new List<IToken>();  
            int index = 0;

            while (index < input.Length)
            {
                bool isFind = false;
                foreach (var rule in Rules)
                {
                    var x = input.Substring(index);
                    var match = Regex.Match(x, rule.Pattern);

                    if (match.Success && match.Index == 0)
                    {
                        isFind = true;

                        result.Add(rule.ParseT(rule.Key, match));

                        index += match.Length;
                        break;
                    }
                }

                if (!isFind)
                {
                    break;
                }
            }

            if (index < input.Length)
            {
                var message = String.Format("Failed to parse the token <{0}>.", input[index]);
                throw new FormatException(message);
            }

            return result.Cast<IToken>().ToList();
        }
    }
}
