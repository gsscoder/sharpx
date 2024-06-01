#pragma warning disable 0618

using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Equivalency;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using SharpX;
using SharpX.Extensions;
using Xunit;

namespace Outcomes;

public class UnitSpecs
{
    [Fact]
    public void Unit_values_should_be_equals()
    {
        var sut1 = new Unit();
        var sut2 = new Unit();

        var outcome = sut1.Equals(sut2);

        outcome.Should().BeTrue();
    }

    [Fact]
    public void Unit_values_should_compare_to_equality()
    {
        var sut1 = new Unit();
        var sut2 = new Unit();

        var outcome = sut1.CompareTo(sut2);
        
        outcome.Should().Be(0);
    }

    [Fact]
    public void Do_executes_a_delegate_and_returns_Unit_value()
    {
        var evidence = 0;

        var outcome = Unit.Do(() => evidence++);

        evidence.Should().Be(1);
        outcome.Should().Be(Unit.Default);
    }

    [Fact]
    public async Task Do_executes_an_async_delegate_and_returns_Unit_value()
    {
        var evidence = 0;

        var outcome = await Unit.DoAsync(() => {
            evidence++;
            return Task.CompletedTask;
        });

        evidence.Should().Be(1);
        outcome.Should().Be(Unit.Default);
    }        

    [Property(MaxTest=1)]
    public FsCheck.Property Chain_Unit_values_with_And_extension()
    {
        return FsCheck.Fluent.Prop.ToProperty(
                Unit.Do(() => VoidMethod())
                .And(DoSomething().ToUnit()).Equals(Unit.Default));

        void VoidMethod() {}
        int DoSomething() => 3 * 2;
    }
}
