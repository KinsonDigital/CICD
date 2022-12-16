// <copyright file="IXmlService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Parses XML.
/// </summary>
internal interface IXmlService
{
    /// <summary>
    /// Gets the value of the first occurrence of an XML tag that matches the given <paramref name="tagName"/>.
    /// </summary>
    /// <param name="xmlFilePath">The full file path to the XML file to process.</param>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <returns>The XML tag value.</returns>
    string GetTagValue(string xmlFilePath, string tagName);
}
