// <copyright file="ReactableFake.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Reactables.Core;

namespace CICDSystemTests.Fakes;

/// <summary>
/// Used for testing the abstract <see cref="Reactable{TData}"/> class.
/// </summary>
/// <typeparam name="T">The type of notification to set.</typeparam>
internal sealed class ReactableFake<T> : Reactable<T>
{
}
