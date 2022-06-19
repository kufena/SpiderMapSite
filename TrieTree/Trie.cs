using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrieTree
{
    public class Trie<T>
    {
        private Dictionary<char, Trie<T>> members;
        private bool endOf = false;
        private T? item;

        public Trie()
        {
            members = new Dictionary<char, Trie<T>>();
        }

        public void Add(string s, int index, T it)
        {
            if (index > s.Length)
            {
                throw new ArgumentException($"{index} is beyond range of {s}");
            }

            if (members.ContainsKey(s[index]))
            {
                var t = members[s[index]];
                if (index + 1 == s.Length) // we're at the end.
                {
                    t.endOf = true;
                    t.item = it;
                }
                else
                {
                    t.Add(s, index + 1, it);
                }
            }
            else
            {
                var t = new Trie<T>();
                members[s[index]] = t;
                if (index + 1 == s.Length)
                {
                    t.endOf = true;
                    t.item = it;
                }
                else
                {
                    t.Add(s, index + 1, it);
                }
            }
        }

        public static int CountFrom(Trie<T> t)
        {
            int total = 0;
            if (t.endOf)
                total += 1;
            foreach (var (c, tr) in t.members)
            {
                total += Trie<T>.CountFrom(tr);
            }

            return total;
        }
        public static int ElaborateFrom(Trie<T> t, List<T> result)
        {
            int total = 0;
            if (t.endOf && (t.item is not null))
            {
                result.Add(t.item);
                total += 1;
            }

            foreach (var (c, tr) in t.members)
            {
                total += Trie<T>.ElaborateFrom(tr, result);
            }

            return total;
        }

        public int CountAfterPrefix(string prefix, int index)
        {
            if (index >= prefix.Length)
            {
                return Trie<T>.CountFrom(this);
            }
            else
            {
                if (members.ContainsKey(prefix[index]))
                {
                    return members[prefix[index]].CountAfterPrefix(prefix, index + 1);
                }
                else
                {
                    return 0;
                }
            }
        }

        public List<T> ElaborateAfterPrefix(string prefix, int index)
        {
            if (index >= prefix.Length)
            {
                List<T> result = new List<T>();
                Trie<T>.ElaborateFrom(this, result);
                return result;
            }
            else
            {
                if (members.ContainsKey(prefix[index]))
                {
                    return members[prefix[index]].ElaborateAfterPrefix(prefix, index + 1);
                }
                else
                {
                    return new List<T>();
                }
            }
        }
    }
}
