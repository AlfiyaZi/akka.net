﻿//-----------------------------------------------------------------------
// <copyright file="ByteStringSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------
using System.Linq;
using System.Text;
using Akka.IO;
using FsCheck;
using Xunit;

namespace Akka.Tests.Util
{

    /// <summary>
    /// TODO: Should we use the FsCheck.XUnit integration when they upgrade to xUnit 2
    /// </summary>
    public class ByteStringSpec
    {
        class Generators
        {

            // TODO: Align with JVM Akka Generator
            public static Arbitrary<ByteString> ByteStrings()
            {
                return Arb.From(Arb.Generate<byte[]>().Select(ByteString.Create));
            }
        }

        public ByteStringSpec()
        {
            Arb.Register<Generators>();
        }

        [Fact]
        public void A_ByteString_must_have_correct_size_when_concatenating()
        {
            Prop.ForAll((ByteString a, ByteString b) => (a + b).Count == a.Count + b.Count)
                .QuickCheckThrowOnFailure();
        }

        [Fact]
        public void A_ByteString_must_have_correct_size_when_dropping()
        {
            Prop.ForAll((ByteString a, ByteString b) => (a + b).Drop(b.Count).Count == a.Count)
                .QuickCheckThrowOnFailure();
        }

        [Fact]
        public void A_ByteString_must_be_sequential_when_taking()
        {
            Prop.ForAll((ByteString a, ByteString b) => (a + b).Take(a.Count).SequenceEqual(a))
                .QuickCheckThrowOnFailure();
        }
        [Fact]
        public void A_ByteString_must_be_sequential_when_dropping()
        {
            Prop.ForAll((ByteString a, ByteString b) => (a + b).Drop(a.Count).SequenceEqual(b))
                .QuickCheckThrowOnFailure();
        }

        [Fact]
        public void A_ByteString_must_be_equal_to_the_original_when_compacting()
        {
            Prop.ForAll((ByteString xs) =>
            {
                var ys = xs.Compact();
                return xs.SequenceEqual(ys) && ys.IsCompact();
            }).QuickCheckThrowOnFailure();
        }
        [Fact]
        public void A_ByteString_must_be_equal_to_the_original_when_recombining()
        {
            Prop.ForAll((ByteString xs, int from, int until) =>
            {
                var tmp1 = xs.SplitAt(until);
                var tmp2 = tmp1.Item1.SplitAt(until);
                return (tmp2.Item1 + tmp2.Item2 + tmp1.Item2).SequenceEqual(xs);
            }).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void A_ByteString_must_behave_as_expected_when_created_from_and_decoding_to_String()
        {
            Prop.ForAll((string s) => ByteString.FromString(s, Encoding.UTF8).DecodeString(Encoding.UTF8) == (s ?? "")) // TODO: What should we do with null string?
                .QuickCheckThrowOnFailure();
        }

        [Fact]
        public void A_ByteString_must_behave_as_expected_when_compacting()
        {
            Prop.ForAll((ByteString a) =>
            {
                var wasCompact = a.IsCompact();
                var b = a.Compact();
                return ((!wasCompact) || (b == a)) &&
                       b.SequenceEqual(a) &&
                       b.IsCompact() &&
                       b.Compact() == b;
            }).QuickCheckThrowOnFailure();
        }

        [Fact(DisplayName = @"A concatenated byte string should return the index of a byte in one the two byte strings.")]
        public void A_concatenated_bytestring_must_return_correct_index_of_elements_in_string()
        {
            var b = ByteString.Create(new byte[] { 1 }) + ByteString.Create(new byte[] { 2 });
            int offset = b.IndexOf(2);

            Assert.Equal(1, offset);
        }

        [Fact(DisplayName = @"A concatenated byte string should return -1 when it was not found in the concatenated byte strings")]
        public void A_concatenated_bytestring_must_return_negative_one_when_an_element_was_not_found()
        {
            var b = ByteString.Create(new byte[] { 1 }) + ByteString.Create(new byte[] { 2 });
            int offset = b.IndexOf(3);

            Assert.Equal(-1, offset);
        }
    }
}
