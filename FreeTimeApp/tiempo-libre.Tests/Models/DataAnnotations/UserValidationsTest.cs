using System.ComponentModel.DataAnnotations;
using tiempo_libre.Models.DataAnnotations;
using Xunit;

namespace tiempo_libre.Models.DataAnnotations.Tests;

public class UserValidationsTest
{
    private readonly UsernameAttribute _attribute = new UsernameAttribute();

    [Theory]
    [InlineData("12345")]
    [InlineData("0")]
    [InlineData("987654321")]
    public void IsValid_ReturnsTrue_ForIntegerStrings(string input)
    {
        Assert.True(_attribute.IsValid(input));
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.co")]
    [InlineData("a@b.com")]
    [InlineData("a@b.com.mx")]
    public void IsValid_ReturnsTrue_ForValidEmails(string input)
    {
        Assert.True(_attribute.IsValid(input));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@domain")]
    [InlineData("user@domain,com")]
    [InlineData("")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string input)
    {
        Assert.False(_attribute.IsValid(input));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(_attribute.IsValid(null));
    }
}
