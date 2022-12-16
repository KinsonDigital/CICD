// <copyright file="XmlService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services;

using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Guards;
using Interfaces;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
internal sealed class XmlService : IXmlService
{
    /// <inheritdoc />
    public string GetTagValue(string xmlFilePath, string tagName)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(xmlFilePath, nameof(xmlFilePath));

        if (string.IsNullOrEmpty(tagName))
        {
            return string.Empty;
        }

        var reader = new XmlTextReader(xmlFilePath);

        while (reader.Read())
        {
            if (reader.Name != tagName)
            {
                continue;
            }

            var tagValue = reader.ReadInnerXml();
            tagValue = tagValue.Replace("\r", string.Empty);
            tagValue = tagValue.Replace("\n", string.Empty);

            return tagValue.Trim();
        }

        return string.Empty;
    }
}
