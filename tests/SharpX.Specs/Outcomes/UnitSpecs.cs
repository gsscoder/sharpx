using FluentAssertions;
using SharpX;
using Xunit;

namespace Outcomes
{
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
    }
}
