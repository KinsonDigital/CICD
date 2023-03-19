// <copyright file="HtmlToMarkdownServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Services;
using FluentAssertions;
using Xunit;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="HtmlToMarkDownService"/> class.
/// </summary>
public class HtmlToMarkdownServiceTests
{
    #region Method Tests
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void LinkToMarkDown_WithNullOrEmptyParam_ThrowException(string html)
    {
        // Act
        var service = new HtmlToMarkDownService();

        // Arrange
        var act = () => service.LinkToMarkDown(html);

        // Act
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'linkHtml')");
    }

    [Fact]
    public void LinkToMarkDown_WithNoLinkTagFound_ThrowException()
    {
        // Act
        var service = new HtmlToMarkDownService();

        // Arrange
        var act = () => service.LinkToMarkDown("content with no matches");

        // Act
        act.Should().Throw<ArgumentException>()
            .WithMessage("The HTML does not contain a valid link tag. (Parameter 'linkHtml')");
    }

    [Fact]
    public void LinkToMarkDown_WithMoreThanOneLinkTagFound_ThrowException()
    {
        // Act
        var html = $"<a href='url'>text</a>{Environment.NewLine}<a href='url'>text</a>";
        var service = new HtmlToMarkDownService();

        // Arrange
        var act = () => service.LinkToMarkDown(html);

        // Act
        act.Should().Throw<ArgumentException>()
            .WithMessage("Contains too many link tags. (Parameter 'linkHtml')");
    }

    [Theory]
    [InlineData("<a href='www.test1.com'>text1</a>", "[text1](www.test1.com)")]
    [InlineData("<a href=\"www.test2.com'>text2</a>", "[text2](www.test2.com)")]
    [InlineData("<a href='www.test3.com\">text3</a>", "[text3](www.test3.com)")]
    [InlineData("<a href=\"www.test4.com\">text4</a>", "[text4](www.test4.com)")]
    [InlineData("<a href='www.test5.com'><img src='./images/img5.png'></a>", "[![image](./images/img5.png)](www.test5.com)")]
    [InlineData("<a href='www.test6.com'><img src='./images/img6.png' height='96'></a>", "[![image](./images/img6.png)](www.test6.com)")]
    [InlineData("<a href='www.test7.com'>text7</a>", "[text7](www.test7.com)")]
    public void LinkToMarkDown_WhenInvoked_ReturnsCorrectResult(string content, string expected)
    {
        // Act
        var service = new HtmlToMarkDownService();

        // Arrange
        var actual = service.LinkToMarkDown(content);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ImgToMarkDown_WithNullOrEmptyParam_ThrowException(string html)
    {
        // Act
        var service = new HtmlToMarkDownService();

        // Arrange
        var act = () => service.ImgToMarkDown(html);

        // Act
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'imgHtml')");
    }

    [Fact]
    public void ImgToMarkDown_WithNoLinkTagFound_ThrowException()
    {
        // Act
        var service = new HtmlToMarkDownService();

        // Arrange
        var act = () => service.ImgToMarkDown("content with no matches");

        // Act
        act.Should().Throw<ArgumentException>()
            .WithMessage("The HTML does not contain a valid image tag. (Parameter 'imgHtml')");
    }

    [Fact]
    public void ImgToMarkDown_WithMoreThanOneLinkTagFound_ThrowException()
    {
        // Act
        var html = $"<img src='url'>{Environment.NewLine}<img src='url'>";
        var service = new HtmlToMarkDownService();

        // Arrange
        var act = () => service.ImgToMarkDown(html);

        // Act
        act.Should().Throw<ArgumentException>()
            .WithMessage("Contains too many image tags. (Parameter 'imgHtml')");
    }

    [Theory]
    [InlineData("<img src='www.test1.com'>", "![image](www.test1.com)")]
    [InlineData("<img src=\"www.test1.com'>", "![image](www.test1.com)")]
    [InlineData("<img src='www.test1.com\">", "![image](www.test1.com)")]
    [InlineData("<img src=\"www.test1.com\">", "![image](www.test1.com)")]
    [InlineData("<img height='96' src='www.test2.com'>", "![image](www.test2.com)")]
    public void ImgToMarkDown_WhenInvoked_ReturnsCorrectResult(string content, string expected)
    {
        // Act
        var service = new HtmlToMarkDownService();

        // Arrange
        var actual = service.ImgToMarkDown(content);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void HeaderToMarkDown_WithNullOrEmptyParam_ThrowException(string headerHtml)
    {
        // Act
        var service = new HtmlToMarkDownService();

        // Arrange
        var act = () => service.HeaderToMarkDown(headerHtml);

        // Act
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'headerHtml')");
    }

    [Fact]
    public void HeaderToMarkDown_WithNoLinkTagFound_ThrowException()
    {
        // Act
        var service = new HtmlToMarkDownService();

        // Arrange
        var act = () => service.HeaderToMarkDown("content with no matches");

        // Act
        act.Should().Throw<ArgumentException>()
            .WithMessage("The HTML does not contain a valid image tag. (Parameter 'headerHtml')");
    }

    [Fact]
    public void HeaderToMarkDown_WithMoreThanOneLinkTagFound_ThrowException()
    {
        // Act
        var html = $"<h1>header1</h1>{Environment.NewLine}<h2>header2</h2>";
        var service = new HtmlToMarkDownService();

        // Arrange
        var act = () => service.HeaderToMarkDown(html);

        // Act
        act.Should().Throw<ArgumentException>()
            .WithMessage("Contains too many image tags. (Parameter 'headerHtml')");
    }

    [Theory]
    [InlineData("<h1>test-header1</h1>", "# test-header1")]
    [InlineData("<h2>test-header2</h2>", "## test-header2")]
    [InlineData("<h3>test-header3</h3>", "### test-header3")]
    [InlineData("<h4>test-header4</h4>", "#### test-header4")]
    [InlineData("<h5>test-header5</h5>", "##### test-header5")]
    [InlineData("<h6>test-header6</h6>", "###### test-header6")]
    [InlineData("<h1 style='font-weight:bold'>test-header7</h1>", "# **test-header7**")]
    [InlineData("<h1 style='font-weight: bold'>test-header8</h1>", "# **test-header8**")]
    [InlineData("<h1 style='color:blue;font-weight:  bold'>test-header9</h1>", "# **test-header9**")]
    [InlineData("<h1 style='font-weight:bold;color:blue;'>test-header10</h1>", "# **test-header10**")]
    [InlineData("<h1 style=\"font-weight:bold'>test-header11</h1>", "# **test-header11**")]
    [InlineData("<h1 style='font-weight:bold\">test-header12</h1>", "# **test-header12**")]
    [InlineData("<h1 style=\"font-weight:bold\">test-header13</h1>", "# **test-header13**")]
    public void HeaderToMarkDown_WhenInvoked_ReturnsCorrectResult(string content, string expected)
    {
        // Act
        var service = new HtmlToMarkDownService();

        // Arrange
        var actual = service.HeaderToMarkDown(content);

        // Assert
        actual.Should().Be(expected);
    }
    #endregion
}
