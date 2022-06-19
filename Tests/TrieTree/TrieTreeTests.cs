using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TrieTree;

namespace Tests.TrieTree
{
    public class TrieTreeTests
    {
        public class UnitTest1
        {
            [Fact]
            public void EmptyTrie()
            {
                var tt = new Trie<string>();
                int n = Trie<string>.CountFrom(tt);
                Assert.Equal(0, n);
            }

            [Fact]
            public void OneEntryTrie()
            {
                var tt = new Trie<string>();
                tt.Add("a",0, "a");
                int n = Trie<string>.CountFrom(tt);
                Assert.Equal(1,n);
            }

            [Fact]
            public void TwoEntries()
            {
                var tt = new Trie<string>();
                tt.Add("one",0, "one");
                tt.Add("two",0, "two");
                int n = Trie<string>.CountFrom(tt);
                Assert.Equal(2,n);
            }

            [Fact]
            public void CountOverlapping()
            {
                var tt = new Trie<string>();
                tt.Add("Won",0, "Won");
                tt.Add("Wonder",0, "Wonder");
                int n = Trie<string>.CountFrom(tt);
                Assert.Equal(2, n);
            }

            [Fact]
            public void ManyEntries()
            {
                var tt = new Trie<string>();
                AddEightThings(tt);
                int n = Trie<string>.CountFrom(tt);
                Assert.Equal(8,n);
            }

            [Fact]
            public void FindFromPrefix()
            {
                var tt = new Trie<string>();
                AddEightThings(tt);
                int n = tt.CountAfterPrefix("wo", 0);
                Assert.Equal(2, n);
            }

            [Fact]
            public void FindFromPrefixWithRealWord()
            {
                var tt = new Trie<string>();
                AddEightThings(tt);
                int n = tt.CountAfterPrefix("won", 0);
                Assert.Equal(2, n);
            }

            [Fact]
            public void FindZeroFromPrefix()
            {
                var tt = new Trie<string>();
                AddEightThings(tt);
                int n = tt.CountAfterPrefix("tart", 0);
                Assert.Equal(0, n);
            }

            [Fact]
            public void FindZeroFromPrefixSomeMatch()
            {
                var tt = new Trie<string>();
                AddEightThings(tt);
                int n = tt.CountAfterPrefix("wot", 0);
                Assert.Equal(0, n);
            }

            [Fact]
            public void ElaborateNone()
            {
                var tt = new Trie<string>();
                AddEightThings(tt);
                var list = tt.ElaborateAfterPrefix("zzz", 0);
                Assert.Empty(list);
            }

            [Fact]
            public void ElaborateThings()
            {
                var tt = new Trie<string>();
                AddEightThings(tt);
                var list = tt.ElaborateAfterPrefix("won", 0);
                Assert.Equal(2, list.Count);
                Assert.Contains(list, x => x.Equals("won"));
                Assert.Contains(list, x => x.Equals("wonder"));
            }

            private static void AddEightThings(Trie<string> tt)
            {
                tt.Add("jimbob", 0, "jimbob");
                tt.Add("won", 0, "won");
                tt.Add("wonder", 0, "wonder");
                tt.Add("walter", 0, "walter");
                tt.Add("window", 0, "window");
                tt.Add("exeter", 0, "exeter");
                tt.Add("exercise", 0, "exercise");
                tt.Add("flobalob", 0, "flobalob");
            }
        }
    
}
}
