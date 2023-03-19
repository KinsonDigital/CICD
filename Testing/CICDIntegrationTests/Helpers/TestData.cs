// <copyright file="TestData.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Reflection;
using Xunit.Sdk;

namespace CICDIntegrationTests.Helpers;

/// <summary>
/// Manages data used for testing and test results.
/// </summary>
internal static class TestData
{
    private static readonly string TextDataDirPath =
        $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/TestData";

    private static readonly string WriteResultDirPath = $"{TextDataDirPath}/TestResults";

    /// <summary>
    /// Loads test data that matches the given <paramref name="fileName"/>.
    /// </summary>
    /// <param name="fileName">The name of the test data file.</param>
    /// <returns>The test data file content.</returns>
    /// <exception cref="AssertActualExpectedException">
    /// Occurs for the following reasons:
    ///     <list type="number">
    ///         <item>When the <paramref name="fileName"/> is null.</item>
    ///         <item>When the <paramref name="fileName"/> is empty.</item>
    ///         <item>When the file path to the test data file does not exist.</item>
    ///     </list>
    /// </exception>
    public static string LoadTestData(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            var actualMsg = fileName is null
                ? "The file name is null."
                : "The file name is empty.";
            throw new AssertActualExpectedException(
                "A file name that is not null or empty.",
                actualMsg,
                "The file path is null or empty.");
        }

        var filePath = $"{TextDataDirPath}/{fileName}";

        if (File.Exists(filePath) is false)
        {
            throw new AssertActualExpectedException(
                "The file to exist.",
                "The file does not exist.",
                $"The text data file '{filePath}' does not exist.");
        }

        return File.ReadAllText(filePath);
    }

    /// <summary>
    /// Loads the test data results that matches the given <paramref name="fileName"/>.
    /// </summary>
    /// <param name="fileName">The name of the test data results file.</param>
    /// <returns>The test data results file content.</returns>
    /// <exception cref="AssertActualExpectedException">
    /// Occurs for the following reasons:
    /// <list type="number">
    ///     <item>When the <paramref name="fileName"/> is null.</item>
    ///     <item>When the <paramref name="fileName"/> is empty.</item>
    ///     <item>When the file path to the test data file does not exist.</item>
    /// </list>
    /// </exception>
    public static string LoadTestResults(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            var actualMsg = fileName is null
                ? "The file name is null."
                : "The file name is empty.";
            throw new AssertActualExpectedException(
                "A file name that is not null or empty.",
                actualMsg,
                "The file path is null or empty.");
        }

        var filePath = $"{WriteResultDirPath}/{fileName}";

        if (File.Exists(filePath) is false)
        {
            throw new AssertActualExpectedException(
                "The results file to exist.",
                "The file results do not exist.",
                $"The result data file '{filePath}' does not exist.");
        }

        return File.ReadAllText(filePath);
    }

    /// <summary>
    /// Writes the given <paramref name="content"/> to the test results directory using the given <paramref name="fileName"/>.
    /// </summary>
    /// <param name="fileName">The name of the test results file.</param>
    /// <param name="content">The content to write to the file.</param>
    /// <exception cref="AssertActualExpectedException">
    /// Occurs for the following reasons:
    ///     <list type="number">
    ///         <item>When the <paramref name="fileName"/> is null.</item>
    ///         <item>When the <paramref name="fileName"/> is empty.</item>
    ///     </list>
    /// </exception>
    /// <remarks>
    ///     If the file already exists, it will be overwritten.
    /// </remarks>
    public static void WriteResult(string? fileName, string content)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            var actualMsg = fileName is null
                ? "The file name is null."
                : "The file name is empty.";
            throw new AssertActualExpectedException(
                "A file name that is not null or empty.",
                actualMsg,
                "The file path is null or empty.");
        }

        if (Directory.Exists(WriteResultDirPath) is false)
        {
            Directory.CreateDirectory(WriteResultDirPath);
        }

        var filePath = $"{WriteResultDirPath}/{fileName}";

        File.WriteAllText(filePath, content);
    }
}
