// <copyright file="ReadmeProcessingException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System;

namespace CICDSystem.Exceptions;

/// <summary>
/// Thrown when there is an issue with pre-processing a README file.
/// </summary>
internal sealed class ReadmeProcessingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadmeProcessingException"/> class.
    /// </summary>
    public ReadmeProcessingException()
        : base("There was an issue with pre-processing the README file.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadmeProcessingException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ReadmeProcessingException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadmeProcessingException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    ///     The <see cref="Exception"/> instance that caused the current exception.
    /// </param>
    public ReadmeProcessingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
