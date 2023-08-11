using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.FSharp.Collections;
using SharpX;
using SharpX.Extensions;
using Xunit;

namespace Outcomes;

public class EnumerableExtensionsSpecs
{
    #region TryHead
    [Property(Arbitrary = new[] { typeof(ArbitraryListOfIntegers) })]
    public void Trying_to_get_the_head_element_of_a_sequence_should_return_Just(
        FSharpList<int> values)
    {
        var outcome = values.TryHead();

        outcome.Should().Match<Maybe<int>>(x =>
            x.Tag == MaybeType.Just && x._value == values.ElementAt(0));
    }

    [Fact]
    public void Trying_to_get_the_head_element_of_an_empty_sequence_should_return_Nothing()
    {
        var outcome = Enumerable.Empty<int>().TryHead();

        outcome.Tag.Should().Be(MaybeType.Nothing);
    }
    #endregion

    #region TryLast
    [Property(Arbitrary = new[] { typeof(ArbitraryListOfIntegers) })]
    public void Trying_to_get_the_last_element_of_a_sequence_should_return_Just(
        FSharpList<int> values)
    {
        var outcome = values.TryLast();

        outcome.Should().Match<Maybe<int>>(x =>
            x.Tag == MaybeType.Just && x._value == values.Last());
    }

    [Fact]
    public void Trying_to_get_the_last_element_of_an_empty_sequence_should_return_Nothing()
    {
        var outcome = Enumerable.Empty<int>().TryLast();

        outcome.Tag.Should().Be(MaybeType.Nothing);
    }
    #endregion

    #region ToMaybe
    [Fact]
    public void An_empty_sequence_should_be_converted_to_Nothing()
    {
        var outcome = Enumerable.Empty<int>().ToMaybe();

        outcome.Should().Match<Maybe<IEnumerable<int>>>(x => x.Tag == MaybeType.Nothing);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryListOfIntegers) })]
    public void An_not_empty_sequence_should_be_converted_to_Just(FSharpList<int> values)
    {
        var outcome = values.ToMaybe();

        outcome.Tag.Should().Be(MaybeType.Just);
    }
    #endregion

    #region Choose
    [Theory]
    [InlineData(
        new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
        new int[] {0, 2, 4, 6, 8})]
    public void Should_choose_elements_to_create_a_new_sequence(
        IEnumerable<int> values, IEnumerable<int> expected)
    {
        var outcome = values.Choose(x => x % 2 == 0
                                        ? Maybe.Just(x)
                                        : Maybe.Nothing<int>());
        
        outcome.Should().BeEquivalentTo(expected);
    }
    #endregion

    #region Intersperse
    [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
    public void Should_intersperse_a_value_in_a_sequence(int value)
    {
        var sequence = new int[] {0, 1, 2, 3, 4};

        var outcome = sequence.Intersperse(value);

        outcome.Should().NotBeNullOrEmpty()
            .And.HaveCount(sequence.Count() * 2 - 1)
            .And.SatisfyRespectively(
                item => item.Should().Be(0),
                item => item.Should().Be(value),
                item => item.Should().Be(1),
                item => item.Should().Be(value),
                item => item.Should().Be(2),
                item => item.Should().Be(value),
                item => item.Should().Be(3),
                item => item.Should().Be(value),
                item => item.Should().Be(4)
            );
    }
    #endregion

    #region FlattenOnce
    [Fact]
    public void Should_flatten_a_sequence_by_one_level()
    {
        var sequence = new List<IEnumerable<int>>()
            {
                new int[] {0, 1, 2},
                new int[] {3, 4, 5},
                new int[] {6, 7, 8}
            };

        var outcome = sequence.FlattenOnce();
        
        outcome.Should().BeEquivalentTo(new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8});
    }
    #endregion

    #region Tail
    [Property(Arbitrary = new[] { typeof(ArbitraryListOfIntegers) })]
    public void Should_return_the_tail_of_a_sequence(FSharpList<int> values)
    {
        var outcome = values.Tail();

        outcome.Should().HaveCount(values.Count() - 1)
            .And.BeEquivalentTo(values.Skip(1));
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryListOfIntegers) })]
    public void Should_return_the_tail_of_a_sequence_using_TailOrEmpty(FSharpList<int> values)
    {
        var outcome = values.TailOrEmpty();

        outcome.Should().HaveCount(values.Count() - 1)
            .And.BeEquivalentTo(values.Skip(1));
    }

    [Fact]
    public void Trying_to_get_the_tail_of_an_empty_sequence_throws_ArgumentException()
    {
        Action action = () => { foreach (var _ in Enumerable.Empty<int>().Tail()) { } };

        action.Should().ThrowExactly<ArgumentException>()
            .WithMessage("The input sequence has an insufficient number of elements.");
    }

    [Fact]
    public void Trying_to_get_the_tail_of_an_empty_sequence_returns_an_empty_sequence_using_TailOrEmpty()
    {
        var outcome = Enumerable.Empty<int>().TailOrEmpty();

        outcome.Should().HaveCount(0)
            .And.BeEquivalentTo(Enumerable.Empty<int>());
    }
    #endregion

    [Fact]
    public void Should_materialize_a_sequence()
    {
        Action action = () => NullYielder(true).Materialize();

        action.Should().Throw<Exception>();

        IEnumerable<object> NullYielder(bool raise)
        {
            if (raise) throw new Exception();

            yield return null;
        }
    }

    #region ChunkBySize
    [Fact]
    public void Should_partition_a_sequence_by_chunk_size_into_arrays_without_remainder()
    {
        var values = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

        var outcome = values.ChunkBySize(3);

        outcome.Should().NotBeNullOrEmpty()
            .And.HaveCount(3)
            .And.SatisfyRespectively(
                item => item.Should().BeEquivalentTo(new int[] { 0, 1, 2 }),
                item => item.Should().BeEquivalentTo(new int[] { 3, 4, 5 }),
                item => item.Should().BeEquivalentTo(new int[] { 6, 7, 8 }));
    }

    [Fact]
    public void Should_partition_a_sequence_by_chunk_size_into_arrays_with_remainder()
    {
        var values = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        var outcome = values.ChunkBySize(3);

        outcome.Should().NotBeNullOrEmpty()
            .And.HaveCount(4)
            .And.SatisfyRespectively(
                item => item.Should().BeEquivalentTo(new int[] { 0, 1, 2 }),
                item => item.Should().BeEquivalentTo(new int[] { 3, 4, 5 }),
                item => item.Should().BeEquivalentTo(new int[] { 6, 7, 8 }),
                item => item.Should().BeEquivalentTo(new int[] { 9, 10 }));
    }

    [Fact]
    public void Should_partition_a_sequence_in_a_single_chunk_if_chunk_size_is_equal_than_elements_count()
    {
        var values = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        var outcome = values.ChunkBySize(10);

        outcome.Should().NotBeNullOrEmpty()
            .And.HaveCount(1)
            .And.SatisfyRespectively(
                item => item.Should().BeEquivalentTo(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
    }

    [Fact]
    public void Should_partition_a_sequence_in_a_single_chunk_if_chunk_size_is_greater_than_elements_count()
    {
        var values = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        var outcome = values.ChunkBySize(11);

        outcome.Should().NotBeNullOrEmpty()
            .And.HaveCount(1)
            .And.SatisfyRespectively(
                item => item.Should().BeEquivalentTo(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Trying_to_partition_a_sequence_by_zero_or_less_chunks_throws_ArgumentException(
        int value)
    {
        Action action = () => { foreach (var _ in new[] { 0, 1, 2  }.ChunkBySize(value)) {}; };

        action.Should().ThrowExactly<ArgumentException>()
            .WithMessage("The input must be positive.");
    }
    
    [Property(Arbitrary = new[] { typeof(ArbitraryListOfIntegers) })]
    public void Trying_to_partition_a_sequence_by_a_negative_number_of_chunks_throws_ArgumentException(
        FSharpList<int> values)
    {
        Action action = () => { foreach (var _ in values.ChunkBySize(-1)) {}; };

        action.Should().ThrowExactly<ArgumentException>()
            .WithMessage("The input must be positive.");
    }
    #endregion

    #region SplitAt
    [Fact]
    public void Should_split_a_sequence_before_index()
    {
        var value = new int[] { 0, 1, 3, 4, 5 };
        var outcome = value.SplitAt(3);

        outcome.Item1.Should().NotBeNullOrEmpty().And.HaveCount(3).And.ContainInOrder(0, 1, 3);
        outcome.Item2.Should().NotBeNullOrEmpty().And.HaveCount(2).And.ContainInOrder(4, 5);
    }

    [Fact]
    public void Left_array_is_empty_when_splitting_at_index_zero()
    {
        var value = new int[] { 0, 1, 3, 4, 5 };
        var outcome = value.SplitAt(0);

        outcome.Item1.Should().BeEmpty();
        outcome.Item2.Should().NotBeNullOrEmpty().And.BeEquivalentTo(value);
    }

    [Fact]
    public void Right_array_is_empty_when_splitting_with_index_equal_to_sequence_elements_count()
    {
        var value = new int[] { 0, 1, 3, 4, 5 };
        var outcome = value.SplitAt(value.Count());

        outcome.Item1.Should().NotBeNullOrEmpty().And.BeEquivalentTo(value);
        outcome.Item2.Should().BeEmpty();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryListOfIntegers) })]
    public void Trying_to_split_a_sequence_with_a_negative_index_throws_ArgumentException(
        FSharpList<int> values)
    {
        Action action = () => values.SplitAt(-1);

        action.Should().ThrowExactly<ArgumentException>()
            .WithMessage("The input must be non-negative.");
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryListOfIntegers) })]
    public void Trying_to_split_a_sequence_with_an_index_greater_than_elements_count_throws_ArgumentException(
        FSharpList<int> values)
    {
        Action action = () => values.SplitAt(values.Count() + 1);

        action.Should().ThrowExactly<ArgumentException>()
            .WithMessage("The input sequence has an insufficient number of elements.");
    }
    #endregion

    // TODO: add a Guid arbitrary generator
    [Fact]
    public void Should_change_order_but_preserve_original_elements()
    {
        for (var i = 0; i < 100; i++)
        {
            var value = Guid.NewGuid().ToString();

            var outcome = new string(value.Shuffle().ToArray());

            outcome.Should().NotBe(value);
            outcome.Should().HaveLength(value.Length);
            outcome.ToArray().Should().Contain(value.ToArray());
        }
    }

    #region Intersperse
    [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
    public void Should_Intersperse_a_value(string[] values)
    {
        var arbitraryString = Strings.Generate(9);

        var outcome = values.Intersperse(arbitraryString);
        
        var outcomeSubset = outcome.Where((number, index) => index % 2 != 0); 

        outcomeSubset.Should().NotBeNullOrEmpty()
            .And.AllBe(arbitraryString);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
    public void Intersperse_randomly_with_chance_zero_yields_original_sequence(string[] values)
    {
        var arbitraryString = Strings.Generate(9);

        var outcome = values.Intersperse(arbitraryString, chance: 0, count: 1);

        outcome.Should().BeEquivalentTo(values);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
    public void Intersperse_randomly_with_count_zero_yields_original_sequence(string[] values)
    {
        var arbitraryString = Strings.Generate(9);

        var outcome = values.Intersperse(arbitraryString, chance: 100, count: 0);

        outcome.Should().BeEquivalentTo(values);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
    public void Should_Intersperse_a_value_randomly(string[] values)
    {
        var arbitraryString = Strings.Generate(9);

        var outcome = values.Intersperse(arbitraryString, chance: 50, count: 2);

        outcome.Should().NotBeNullOrEmpty()
            .And.Contain(arbitraryString)
            .And.Contain(values);
    }
    #endregion
}
