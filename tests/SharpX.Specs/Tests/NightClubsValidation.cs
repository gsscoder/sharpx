// From https://github.com/fsprojects/fsharpx/blob/master/tests/FSharpx.CSharpTests/ValidationExample.cs.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SharpX;
using Xunit;

namespace Tests;

enum Sobriety { Sober, Tipsy, Drunk, Paralytic, Unconscious }
enum Gender { Male, Female }

class Person
{
    public Gender Gender { get; private set; }
    public int Age { get; private set; }
    public List<string> Clothes { get; private set; }
    public Sobriety Sobriety { get; private set; }

    public Person(Gender gender, int age, List<string> clothes, Sobriety sobriety)
    {
        Gender = gender;
        Age = age;
        Clothes = clothes;
        Sobriety = sobriety;
    }
}

class Club
{
    public static Result<Person, string> CheckAge(Person p)
    {
        if (p.Age < 18) {
            return Result<Person, string>.FailWith("Too young!");
        }
        if (p.Age > 40) {
            return Result<Person, string>.FailWith("Too old!");
        }
        return Result<Person, string>.Succeed(p);
    }

    public static Result<Person, string> CheckClothes(Person p)
    {
        if (p.Gender == Gender.Male && !p.Clothes.Contains("Tie")) {
            return Result<Person, string>.FailWith("Smarten up!");
        }
        if (p.Gender == Gender.Female && p.Clothes.Contains("Trainers")) {
            return Result<Person, string>.FailWith("Wear high heels!");
        }
        return Result<Person, string>.Succeed(p);
    }

    public static Result<Person, string> CheckSobriety(Person p)
    {
        if (new[] { Sobriety.Drunk, Sobriety.Paralytic, Sobriety.Unconscious }
            .Contains(p.Sobriety)) {
            return Result<Person, string>.FailWith("Sober up!");
        }
        return Result<Person, string>.Succeed(p);
    }
}

class ClubbedToDeath
{
    public static Result<decimal, string> CostToEnter(Person p)
    {
        return from a in Club.CheckAge(p)
                from b in Club.CheckClothes(a)
                from c in Club.CheckSobriety(b)
                select c.Gender == Gender.Female ? 0m : 5m;
    }
}

public class Test1
{
    [Fact]
    public void Part1()
    {
        var Dave = new Person(Gender.Male, 41, new List<string> { "Tie", "Jeans" }, Sobriety.Sober);
        var costDave = ClubbedToDeath.CostToEnter(Dave);
        "Too old!".Should().Be(costDave.FailedWith().First());

        var Ken = new Person(Gender.Male, 28, new List<string> { "Tie", "Shirt" }, Sobriety.Tipsy);
        var costKen = ClubbedToDeath.CostToEnter(Ken);
        5m.Should().Be(costKen.SucceededWith());

        var Ruby = new Person(Gender.Female, 25, new List<string> { "High heels" }, Sobriety.Tipsy);
        var costRuby = ClubbedToDeath.CostToEnter(Ruby);
        costRuby.Match(
            (x, msgs) =>
            {
                0m.Should().Be(x);
            },
            msgs =>
            {
                Assert.True(false, "fail");
            });

        var Ruby17 = new Person(Ruby.Gender, 17, Ruby.Clothes, Ruby.Sobriety);
        var costRuby17 = ClubbedToDeath.CostToEnter(Ruby17);
        "Too young!".Should().Be(costRuby17.FailedWith().First());

        var KenUnconscious = new Person(Ken.Gender, Ken.Age, Ken.Clothes, Sobriety.Unconscious);
        var costKenUnconscious = ClubbedToDeath.CostToEnter(KenUnconscious);
        costKenUnconscious.Match(
            (x, msgs) =>
            {
                Assert.True(false, "fail");
            },
            msgs =>
            {
                "Sober up!".Should().Be(msgs.First());
            });
    }
}
