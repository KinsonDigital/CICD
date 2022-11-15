// <copyright file="ReadmeProcessingExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Exceptions;
using FluentAssertions;
using Xunit;

namespace CICDSystemTests.Exceptions;

/// <summary>
/// Tests the <see cref="ReadmeProcessingException"/> class.
/// </summary>
public class ReadmeProcessingExceptionTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WithNoParam_CorrectlySetsExceptionMessage()
    {
        // Act
        var exception = new ReadmeProcessingException();

        // Assert
        exception.Message.Should().Be("There was an issue with pre-processing the README file.");
    }

    [Fact]
    public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMessage()
    {
        // Act
        var exception = new ReadmeProcessingException("test-message");

        // Assert
        exception.Message.Should().Be("test-message");
    }

    [Fact]
    public void Ctor_WhenInvokedWithMessageAndInnerException_ThrowsException()
    {
        // Arrange
        var innerException = new Exception("inner-exception");

        // Act
        var deviceException = new ReadmeProcessingException("test-exception", innerException);

        // Assert
        deviceException.InnerException.Message.Should().Be("inner-exception");
        deviceException.Message.Should().Be("test-exception");
    }
    #endregion
}
