﻿// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Windows.Controls;
using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace Microsoft.VisualStudio.ProjectSystem.VS.PropertyPages
{
    /// <summary>
    /// Implementation of ILaunchSettingsUIProvider for the Project launch type.
    /// </summary>
    [Export(typeof(ILaunchSettingsUIProvider))]
    [AppliesTo(ProjectCapability.LaunchProfiles)]
    [Order(Order.Lowest)] // Lowest priority to allow this to be overridden
    internal class ProjectLaunchSettingsUIProvider : ILaunchSettingsUIProvider
    {
        [ImportingConstructor]
        public ProjectLaunchSettingsUIProvider(UnconfiguredProject _) // force MEF scope
        {
        }

        public string CommandName => LaunchSettingsProvider.RunProjectCommandName;

        public string FriendlyName => PropertyPageResources.ProfileKindProjectName;

        public bool ShouldEnableProperty(string propertyName)
        {
            // Disable the executable and launch url controls
            return !string.Equals(propertyName, UIProfilePropertyName.Executable, StringComparisons.UIPropertyNames) &&
                   !string.Equals(propertyName, UIProfilePropertyName.LaunchUrl, StringComparisons.UIPropertyNames);
        }

        /// <inheritdoc />
        /// <remarks>This implementation does not provide any UI.</remarks>
        public UserControl? CustomUI => null;

        public void ProfileSelected(IWritableLaunchSettings curSettings)
        {
        }
    }
}
