// <copyright file="ExtensionMethodsTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDTests;

using FluentAssertions;
using Xunit;

public class ExtensionMethodsTests
{
    [Theory]
    [InlineData("CONVERTOKEBAB", "CONVERTOKEBAB")]
    [InlineData("converttokebab", "converttokebab")]
    [InlineData("Convert To Kebab", "convert-to-kebab")]
    [InlineData("ConvertTo Kebab", "convert-to-kebab")]
    [InlineData("Convert ToKebab", "convert-to-kebab")]
    [InlineData("ConvertToKebab", "convert-to-kebab")]
    [InlineData("convert to kebab", "convert-to-kebab")]
    [InlineData("convertTo kebab", "convert-to-kebab")]
    [InlineData("convert toKebab", "convert-to-kebab")]
    [InlineData("convert4to4kebab", "convert4to4kebab")]
    public void ToKebabCase_WhenInvoked_ReturnsCorrectResult(string value, string expected)
    {
        // Arrange & Act
        var actual = value.ToKebabCase();

        // Assert
        actual.Should().Be(expected);
    }
}
