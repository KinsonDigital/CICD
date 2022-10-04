// <copyright file="IBranchValidatorService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System;

namespace CICDSystem.Services;

/// <summary>
/// Validates GIT branches using a <c>Fluent API</c>.
/// </summary>
internal interface IBranchValidatorService
{
    /// <summary>
    /// Validates that the given <paramref name="branch"/> has the correct syntax using the given <paramref name="branchPattern"/>.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <param name="branchPattern">The syntax pattern.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService ValidSyntax(string branch, string branchPattern);

    /// <summary>
    /// Validates that the given <paramref name="branch"/> has the correct syntax using the given <paramref name="branchPattern"/> and
    /// combines the given <paramref name="andPredicates"/> results with the function result.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <param name="branchPattern">The syntax pattern.</param>
    /// <param name="andPredicates">The list of predicates to include in the end result.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    /// <remarks>
    ///     If any of the <paramref name="andPredicates"/> return false, the entire function will return false.
    /// </remarks>
    IBranchValidatorService ValidSyntax(string branch, string branchPattern, params Predicate<string>[] andPredicates);

    /// <summary>
    /// Validates that the given <paramref name="branch"/> matches the given <paramref name="branchType"/>.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <param name="branchType">The type of branch to expect.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService ValidSyntax(string branch, BranchType branchType);

    /// <summary>
    /// Validates that the given <paramref name="branch"/> matches the given <paramref name="branchType"/> and
    /// combines the given <paramref name="andPredicates"/> results with the function result.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <param name="branchType">The type of branch to expect.</param>
    /// <param name="andPredicates">The list of predicates to include in the end result.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    /// <remarks>
    ///     If any of the <paramref name="andPredicates"/> return false, the entire function will return false.
    /// </remarks>
    IBranchValidatorService ValidSyntax(string branch, BranchType branchType, params Predicate<string>[] andPredicates);

    /// <summary>
    /// Validates if the given <paramref name="branch"/> is a <c>master</c> branch.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService IsMasterBranch(string branch);

    /// <summary>
    /// Validates if the given <paramref name="branch"/> is a <see cref="BranchType.Master"/> branch and
    /// combines the given <paramref name="andPredicates"/> results with the function result.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <param name="andPredicates">The list of predicates to include in the end result.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    /// <remarks>
    ///     If any of the <paramref name="andPredicates"/> return false, the entire function will return false.
    /// </remarks>
    IBranchValidatorService IsMasterBranch(string branch, params Predicate<string>[] andPredicates);

    /// <summary>
    /// Validates if the given <paramref name="branch"/> is a <see cref="BranchType.Develop"/> branch.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService IsDevelopBranch(string branch);

    /// <summary>
    /// Validates if the given <paramref name="branch"/> is a <see cref="BranchType.Develop"/> branch and
    /// combines the given <paramref name="andPredicates"/> results with the function result.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <param name="andPredicates">The list of predicates to include in the end result.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    /// <remarks>
    ///     If any of the <paramref name="andPredicates"/> return false, the entire function will return false.
    /// </remarks>
    IBranchValidatorService IsDevelopBranch(string branch, params Predicate<string>[] andPredicates);

    /// <summary>
    /// Validates if the given <paramref name="branch"/> is a <see cref="BranchType.Feature"/> branch.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService IsFeatureBranch(string branch);

    /// <summary>
    /// Validates if the given <paramref name="branch"/> is a <see cref="BranchType.Feature"/> branch and
    /// combines the given <paramref name="andPredicates"/> results with the function result.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <param name="andPredicates">The list of predicates to include in the end result.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    /// <remarks>
    ///     If any of the <paramref name="andPredicates"/> return false, the entire function will return false.
    /// </remarks>
    IBranchValidatorService IsFeatureBranch(string branch, params Predicate<string>[] andPredicates);

    /// <summary>
    /// Validates that the given <paramref name="branch"/> is a <see cref="BranchType.Preview"/> branch.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService IsPreviewBranch(string branch);

    /// <summary>
    /// Validates that the given <paramref name="branch"/> is a <see cref="BranchType.Preview"/> branch and
    /// combines the given <paramref name="andPredicates"/> results with the function result.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <param name="andPredicates">The list of predicates to include in the end result.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    /// <remarks>
    ///     If any of the <paramref name="andPredicates"/> return false, the entire function will return false.
    /// </remarks>
    IBranchValidatorService IsPreviewBranch(string branch, params Predicate<string>[] andPredicates);

    /// <summary>
    /// Validates that the given <paramref name="branch"/> is a <see cref="BranchType.PreviewFeature"/> branch.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService IsPreviewFeatureBranch(string branch);

    /// <summary>
    /// Validates that the given <paramref name="branch"/> is a <see cref="BranchType.PreviewFeature"/> branch and
    /// combines the given <paramref name="andPredicates"/> results with the function result.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <param name="andPredicates">The list of predicates to include in the end result.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    /// <remarks>
    ///     If any of the <paramref name="andPredicates"/> return false, the entire function will return false.
    /// </remarks>
    IBranchValidatorService IsPreviewFeatureBranch(string branch, params Predicate<string>[] andPredicates);

    /// <summary>
    /// Validates that the given <paramref name="branch"/> is a <see cref="BranchType.Release"/> branch.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService IsReleaseBranch(string branch);

    /// <summary>
    /// Validates that the given <paramref name="branch"/> is a <see cref="BranchType.Release"/> branch and
    /// combines the given <paramref name="andPredicates"/> results with the function result.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <param name="andPredicates">The list of predicates to include in the end result.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    /// <remarks>
    ///     If any of the <paramref name="andPredicates"/> return false, the entire function will return false.
    /// </remarks>
    IBranchValidatorService IsReleaseBranch(string branch, params Predicate<string>[] andPredicates);

    /// <summary>
    /// Validates that the given <paramref name="branch"/> is a <see cref="BranchType.HotFix"/> branch.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService IsHotFixBranch(string branch);

    /// <summary>
    /// Validates that the given <paramref name="branch"/> is a <see cref="BranchType.HotFix"/> branch and
    /// combines the given <paramref name="andPredicates"/> results with the function result.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <param name="andPredicates">The list of predicates to include in the end result.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    /// <remarks>
    ///     If any of the <paramref name="andPredicates"/> return false, the entire function will return false.
    /// </remarks>
    IBranchValidatorService IsHotFixBranch(string branch, params Predicate<string>[] andPredicates);

    /// <summary>
    /// Validates that the source branch of a pull request that matches the given <paramref name="prNumber"/>
    /// matches the given <paramref name="branchType"/>.
    /// </summary>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="branchType">The type of branch to expect.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService PRSourceBranchCorrect(int prNumber, BranchType branchType);

    /// <summary>
    /// Validates that the source branch of a pull request that matches the given <paramref name="prNumber"/> matches
    /// the given <paramref name="branchType"/> and combines the given <paramref name="andPredicates"/> results with the function result.
    /// </summary>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="branchType">The type of branch to expect.</param>
    /// <param name="andPredicates">The list of predicates to include in the end result.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    /// <remarks>
    ///     If any of the <paramref name="andPredicates"/> return false, the entire function will return false.
    /// </remarks>
    IBranchValidatorService PRSourceBranchCorrect(int prNumber, BranchType branchType, params Predicate<string>[] andPredicates);

    /// <summary>
    /// Validates that the target branch of a pull request that matches the given <paramref name="prNumber"/>
    /// matches the given <paramref name="branchType"/>.
    /// </summary>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="branchType">The type of branch to expect.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService PRTargetBranchCorrect(int prNumber, BranchType branchType);

    /// <summary>
    /// Validates that the target branch of a pull request that matches the given <paramref name="prNumber"/> matches
    /// the given <paramref name="branchType"/> and combines the given <paramref name="andPredicates"/> results with the function result.
    /// </summary>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="branchType">The type of branch to expect.</param>
    /// <param name="andPredicates">The list of predicates to include in the end result.</param>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    /// <remarks>
    ///     If any of the <paramref name="andPredicates"/> return false, the entire function will return false.
    /// </remarks>
    IBranchValidatorService PRTargetBranchCorrect(int prNumber, BranchType branchType, params Predicate<string>[] andPredicates);

    /// <summary>
    /// Provides the ability to use <c>or</c> logic with the results between other <c>Fluent API</c> methods.
    /// </summary>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService Or();

    /// <summary>
    /// Returns a <c>bool</c> value indicating whether or not the given <paramref name="branch"/> that <c>might</c> contain
    /// and issue number exists.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <returns><c>true</c> if the issue number exists.</returns>
    bool BranchIssueNumberExists(string branch);

    /// <summary>
    /// Gets the type of the given <paramref name="branch"/>.
    /// </summary>
    /// <param name="branch">The GIT branch.</param>
    /// <returns>The type of branch.</returns>
    BranchType GetBranchType(string branch);

    /// <summary>
    /// Returns a <c>bool</c> value indicating whether or not the currently checked out GIT branch matches
    /// the given <paramref name="branchPattern"/>.
    /// </summary>
    /// <param name="branchPattern">The syntax pattern.</param>
    /// <returns><c>true</c> if the a match.</returns>
    bool CurrentBranchIsValid(string branchPattern);

    /// <summary>
    /// Returns a <c>bool</c> value indicating whether or not the currently checked out GIT branch
    /// matches the given <paramref name="branchType"/>.
    /// </summary>
    /// <param name="branchType">The type of branch to expect.</param>
    /// <returns><c>true</c> if the a match.</returns>
    bool CurrentBranchIsValid(BranchType branchType);

    /// <summary>
    /// Resets the internal state of the objects logic.
    /// </summary>
    /// <returns>The current object to continue the <c>Fluent API</c> chain.</returns>
    IBranchValidatorService Reset();

    /// <summary>
    /// Returns the result of the <c>Fluent API</c> calls.
    /// </summary>
    /// <returns>The logical result.</returns>
    bool GetValue();
}
