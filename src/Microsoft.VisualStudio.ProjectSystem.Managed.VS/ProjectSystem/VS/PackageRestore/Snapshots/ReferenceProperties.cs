﻿// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using NuGet.SolutionRestoreManager;

namespace Microsoft.VisualStudio.ProjectSystem.VS.PackageRestore
{
    /// <summary>
    ///     Immutable collection of <see cref="IVsReferenceProperty"/> objects.
    /// </summary>
    internal class ReferenceProperties : ImmutablePropertyCollection<IVsReferenceProperty>, IVsReferenceProperties
    {
        public ReferenceProperties(IEnumerable<IVsReferenceProperty> items)
            : base(items, item => item.Name)
        {
        }
    }
}
