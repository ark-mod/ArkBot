using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArkBot.Helpers;
using System.Globalization;

namespace ArkBot.Tests
{
    [TestClass]
    public class CommandHelper_ParseArgs
    {
        [TestMethod]
        public void ParseArgs_Multiple1()
        {
            var args = new[] { "tribe", "Tribe", "Name", "player", "PlayerName" };

            var parsed = CommandHelper.ParseArgs(args, null, new { tribe = "", player = "", skip = 0 }, x =>
                x.For(y => y.tribe, untilNextToken: true)
                .For(y => y.player)
                .For(y => y.skip, defaultValue: 10));

            Assert.AreEqual("Tribe Name", parsed.tribe);
            Assert.AreEqual("PlayerName", parsed.player);
            Assert.AreEqual(10, parsed.skip);
        }

        [TestMethod]
        public void ParseArgs_Multiple2()
        {
            var args = new[] { "tribe", "TribeName", "skip", "20", "player", "Player", "Name" };

            var parsed = CommandHelper.ParseArgs(args, null, new { tribe = "", player = "", skip = 0 }, x =>
                x.For(y => y.tribe)
                .For(y => y.player, untilNextToken: true)
                .For(y => y.skip, defaultValue: 0));

            Assert.AreEqual("TribeName", parsed.tribe);
            Assert.AreEqual("Player Name", parsed.player);
            Assert.AreEqual(20, parsed.skip);
        }

        [TestMethod]
        public void ParseArgs_NoPrefixUntilNextToken()
        {
            var args = new[] { "Search", "Query", "player", "Player", "Name", "skip", "30" };

            var parsed = CommandHelper.ParseArgs(args, null, new { query = "", tribe = "", player = "", skip = 0 }, x =>
                x.For(y => y.query, noPrefix: true, untilNextToken: true)
                .For(y => y.player, untilNextToken: true)
                .For(y => y.skip, defaultValue: 0));

            Assert.AreEqual("Search Query", parsed.query);
            Assert.IsNull(parsed.tribe);
            Assert.AreEqual("Player Name", parsed.player);
            Assert.AreEqual(30, parsed.skip);
        }

        [TestMethod]
        public void ParseArgs_NoPrefixTwiceAndCapitalized()
        {
            var args = new[] { "Search Query", "70", "player", "Player", "Name", "skip", "30" };

            var parsed = CommandHelper.ParseArgs(args, null, new { Query = "", Age = 0, Tribe = "", Player = "", Skip = 0 }, x =>
                x.For(y => y.Query, noPrefix: true)
                .For(y => y.Age, noPrefix: true)
                .For(y => y.Player, untilNextToken: true)
                .For(y => y.Skip, defaultValue: 0));

            Assert.AreEqual("Search Query", parsed.Query);
            Assert.AreEqual(70, parsed.Age);
            Assert.IsNull(parsed.Tribe);
            Assert.AreEqual("Player Name", parsed.Player);
            Assert.AreEqual(30, parsed.Skip);
        }

        [TestMethod]
        public void ParseArgs_Named()
        {
            var args = new[] { "Search Query", "player", "Player", "Name", "skip", "30" };
            var getNamedArg = new Func<string, string>(x => x == "query" ? args[0] : null);

            var parsed = CommandHelper.ParseArgs(args, getNamedArg, new { query = "", player = "", skip = 0 }, x =>
                x.For(y => y.query, named: "query")
                .For(y => y.player, untilNextToken: true)
                .For(y => y.skip, defaultValue: 0));

            Assert.AreEqual("Search Query", parsed.query);
            Assert.AreEqual("Player Name", parsed.player);
            Assert.AreEqual(30, parsed.skip);
        }

        [TestMethod]
        public void ParseArgs_IsRequired()
        {
            var args = new[] {"player", "Player", "Name" };

            var parsed = CommandHelper.ParseArgs(args, null, new { player = "", skip = 0 }, x =>
                x.For(y => y.player, untilNextToken: true)
                .For(y => y.skip, isRequired: true));

            Assert.IsNull(parsed);
        }

        [TestMethod]
        public void ParseArgs_Flags()
        {
            var args = new[] { "Search Query", "flag1", "player", "Player", "Name", "flag2", "skip", "30" };
            var getNamedArg = new Func<string, string>(x => x == "query" ? args[0] : null);

            var parsed = CommandHelper.ParseArgs(args, getNamedArg, new { query = "", flag1 = false, player = "", flag2 = false, flag3 = false, skip = 0 }, x =>
                x.For(y => y.query, named: "query")
                .For(y => y.player, untilNextToken: true)
                .For(y => y.flag1, flag: true)
                .For(y => y.flag2, flag: true)
                .For(y => y.flag3, flag: true)
                .For(y => y.skip, defaultValue: 0));

            Assert.AreEqual("Search Query", parsed.query);
            Assert.AreEqual("Player Name", parsed.player);
            Assert.AreEqual("Player Name", parsed.player);
            Assert.IsTrue(parsed.flag1);
            Assert.IsTrue(parsed.flag2);
            Assert.IsFalse(parsed.flag3);
            Assert.AreEqual(30, parsed.skip);
        }

        [TestMethod]
        public void ParseArgs_NoDoubleParsingRequired()
        {
            var args = new[] { "owner", "john" };

            var parsed = CommandHelper.ParseArgs(args, null, new { Query = "", Owner = "" }, x =>
                x.For(y => y.Query, noPrefix: true, untilNextToken: true, isRequired: true)
                .For(y => y.Owner, untilNextToken: true));

            Assert.IsNull(parsed);
        }

        [TestMethod]
        public void ParseArgs_NoDoubleParsing()
        {
            var args = new[] { "owner", "john" };

            var parsed = CommandHelper.ParseArgs(args, null, new { query = "", owner = "" }, x =>
                x.For(y => y.query, noPrefix: true, untilNextToken: true)
                .For(y => y.owner, untilNextToken: true));

            Assert.IsNull(parsed.query);
            Assert.AreEqual("john", parsed.owner);
        }

        [TestMethod]
        public void ParseArgs_AvoidIndexOutOfRange()
        {
            var args = new[] { "compareto" };

            var parsed = CommandHelper.ParseArgs(args, null, new { CompareTo = DateTime.MinValue }, x =>
                x.For(y => y.CompareTo, formatProvider: CultureInfo.CurrentCulture));

            Assert.AreEqual(DateTime.MinValue, parsed.CompareTo);
        }

        [TestMethod]
        public void ParseArgs_StartWithFlags()
        {
            var args = new[] { "flag1" };

            var parsed = CommandHelper.ParseArgs(args, null, new { Flag1 = false }, x =>
                x.For(y => y.Flag1, flag: true));

            Assert.AreEqual(true, parsed.Flag1);
        }

        [TestMethod]
        public void ParseArgs_NoPrefixAfterConstantLengthTokens()
        {
            var args = new[] { "flag1", "some", "text" };

            var parsed = CommandHelper.ParseArgs(args, null, new { Flag1 = false, Text = "" }, x =>
                x.For(y => y.Flag1, flag: true)
                .For(y => y.Text, noPrefix: true, untilNextToken: true));

            Assert.AreEqual(true, parsed.Flag1);
            Assert.AreEqual("some text", parsed.Text);
        }

        [TestMethod]
        public void ParseArgs_NoPrefixButPreferPrefix()
        {
            var args = new[] { "flag1", "prefix", "some", "text" };

            var parsed = CommandHelper.ParseArgs(args, null, new { Flag1 = false, Text = "", Prefix = "" }, x =>
                x.For(y => y.Flag1, flag: true)
                .For(y => y.Text, noPrefix: true, untilNextToken: true)
                .For(y => y.Prefix, untilNextToken: true));

            Assert.AreEqual(true, parsed.Flag1);
            Assert.AreEqual("some text", parsed.Prefix);
            Assert.IsNull(parsed.Text);
        }
    }
}
