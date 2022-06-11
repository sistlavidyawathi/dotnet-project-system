﻿// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using Microsoft.VisualStudio.ProjectSystem;

namespace Microsoft.VisualStudio.Telemetry
{
    /// <summary>
    ///     Provides telemetry property names used throughout this project.
    /// </summary>
    internal static class TelemetryPropertyName
    {
        /// <summary>
        ///     Indicates the prefix (vs.projectsystem.managed) of all property names throughout this project.
        /// </summary>
        public const string Prefix = "vs.projectsystem.managed";

        /// <summary>
        ///     Indicates the reason that a project's last build is considered out-of-date.
        /// </summary>
        public static readonly string UpToDateCheckFailReason = BuildPropertyName(TelemetryEventName.UpToDateCheckFail, "Reason2");

        /// <summary>
        ///     Indicates the duration of the up-to-date check, in milliseconds.
        /// </summary>
        public const string UpToDateCheckDurationMillis = Prefix + ".uptodatecheck.durationmillis";

        /// <summary>
        ///     Indicates the number of file system timestamps that were queried during the up-to-date check.
        /// </summary>
        public const string UpToDateCheckFileCount = Prefix + ".uptodatecheck.filecount";

        /// <summary>
        ///     Indicates the number of (implicitly active) configurations that were included in the the up-to-date check.
        /// </summary>
        /// <remarks>
        ///     The up-to-date check runs for the active configuration only, but will consider state from all
        ///     implicitly active configurations. Generally, for a single targeting project this will equal one,
        ///     and for multi-targeting projects this will equal the number of target frameworks being targeted.
        /// </remarks>
        public const string UpToDateCheckConfigurationCount = Prefix + ".uptodatecheck.configurationcount";

        /// <summary>
        ///     Indicates the user's chosen logging level. Values from the <see cref="LogLevel"/> enum.
        /// </summary>
        public const string UpToDateCheckLogLevel = Prefix + ".uptodatecheck.loglevel";

        /// <summary>
        ///     Indicates any ignore kinds provided to the fast up-to-date check.
        ///     Used to skip analyzers during indirect builds (for debug or unit tests).
        /// </summary>
        public const string UpToDateCheckIgnoreKinds = Prefix + ".uptodatecheck.ignorekinds";

        /// <summary>
        ///     Identifies the project to which data in the telemetry event applies.
        /// </summary>
        public const string UpToDateCheckProject = Prefix + ".uptodatecheck.projectid";

        /// <summary>
        ///     Indicates the number of checks performed for this project so far in the current session, starting at one.
        ///     This number resets when the project is reloaded.
        /// </summary>
        public const string UpToDateCheckNumber = Prefix + ".uptodatecheck.checknumber";

        /// <summary>
        ///     Indicates the project when the dependency tree is updated with all resolved dependencies.
        /// </summary>
        public static readonly string TreeUpdatedResolvedProject = BuildPropertyName(TelemetryEventName.TreeUpdatedResolved, "Project");

        /// <summary>
        ///     Indicates the project when the dependency tree is updated with unresolved dependencies.
        /// </summary>
        public static readonly string TreeUpdatedUnresolvedProject = BuildPropertyName(TelemetryEventName.TreeUpdatedUnresolved, "Project");

        /// <summary>
        ///     Identifies the project to which data in the telemetry event applies.
        /// </summary>
        public static readonly string ProjectUnloadDependenciesProject = BuildPropertyName(TelemetryEventName.ProjectUnloadDependencies, "Project");

        /// <summary>
        ///     Identifies the version of project unload dependencies telemetry being sent.
        /// </summary>
        public static readonly string ProjectUnloadDependenciesVersion = BuildPropertyName(TelemetryEventName.ProjectUnloadDependencies, "Version");

        /// <summary>
        ///     Identifies the time between project load and unload, in milliseconds.
        /// </summary>
        public static readonly string ProjectUnloadProjectAgeMillis = BuildPropertyName(TelemetryEventName.ProjectUnloadDependencies, "ProjectAgeMillis");

        /// <summary>
        ///     Identifies the total number of visible dependencies in the project.
        ///     If a project multi-targets (i.e. <see cref="ProjectUnloadTargetFrameworkCount"/> is greater than one) then the count of dependencies
        ///     in each target is summed together to produce this single value. If a breakdown is required, <see cref="ProjectUnloadDependencyBreakdown"/>
        ///     may be used.
        /// </summary>
        public static readonly string ProjectUnloadTotalDependencyCount = BuildPropertyName(TelemetryEventName.ProjectUnloadDependencies, "TotalDependencyCount");

        /// <summary>
        ///     Identifies the total number of visible unresolved dependencies in the project.
        ///     If a project multi-targets (i.e. <see cref="ProjectUnloadTargetFrameworkCount"/> is greater than one) then the count of unresolved dependencies
        ///     in each target is summed together to produce this single value. If a breakdown is required, <see cref="ProjectUnloadDependencyBreakdown"/>
        ///     may be used.
        /// </summary>
        public static readonly string ProjectUnloadUnresolvedDependencyCount = BuildPropertyName(TelemetryEventName.ProjectUnloadDependencies, "UnresolvedDependencyCount");

        /// <summary>
        ///     Identifies the number of frameworks this project targets.
        /// </summary>
        public static readonly string ProjectUnloadTargetFrameworkCount = BuildPropertyName(TelemetryEventName.ProjectUnloadDependencies, "TargetFrameworkCount");

        /// <summary>
        ///     Contains structured data describing the number of total/unresolved dependencies broken down by target framework and dependency type.
        /// </summary>
        public static readonly string ProjectUnloadDependencyBreakdown = BuildPropertyName(TelemetryEventName.ProjectUnloadDependencies, "DependencyBreakdown");

        /// <summary>
        ///     Indicates whether seen all rules initialized when the dependency tree is updated with all resolved dependencies.
        /// </summary>
        public static readonly string TreeUpdatedResolvedObservedAllRules = BuildPropertyName(TelemetryEventName.TreeUpdatedResolved, "ObservedAllRules");

        /// <summary>
        ///      Indicates whether seen all rules initialized when the dependency tree is updated with unresolved dependencies.
        /// </summary>
        public static readonly string TreeUpdatedUnresolvedObservedAllRules = BuildPropertyName(TelemetryEventName.TreeUpdatedUnresolved, "ObservedAllRules");

        /// <summary>
        ///     Indicates whether a design-time build has completed without errors.
        /// </summary>
        public static readonly string DesignTimeBuildCompleteSucceeded = BuildPropertyName(TelemetryEventName.DesignTimeBuildComplete, "Succeeded");

        /// <summary>
        ///     Indicates the targets and their times during a design-time build.
        /// </summary>
        public static readonly string DesignTimeBuildCompleteTargets = BuildPropertyName(TelemetryEventName.DesignTimeBuildComplete, "Targets");

        /// <summary>
        ///     Indicates the project that contains the SDK version.
        /// </summary>
        public static readonly string SDKVersionProject = BuildPropertyName(TelemetryEventName.SDKVersion, "Project");

        /// <summary>
        ///     Indicates the actual underlying version of .NET Core SDK.
        /// </summary>
        public static readonly string SDKVersionNETCoreSdkVersion = BuildPropertyName(TelemetryEventName.SDKVersion, "NETCoreSdkVersion");

        /// <summary>
        ///     Indicates the number of TempPE DLLs compiled
        /// </summary>
        public static readonly string TempPECompileCount = BuildPropertyName(TelemetryEventName.TempPEProcessQueue, "CompileCount");

        /// <summary>
        ///     Indicates the starting length of the TempPE compilation queue
        /// </summary>
        public static readonly string TempPEInitialQueueLength = BuildPropertyName(TelemetryEventName.TempPEProcessQueue, "QueueLength");

        /// <summary>
        ///     Indicates whether the TempPE compilation was cancelled
        /// </summary>
        public static readonly string TempPECompileWasCancelled = BuildPropertyName(TelemetryEventName.TempPEProcessQueue, "Cancelled");

        /// <summary>
        ///     Indicates the duration of the TempPE compilation
        /// </summary>
        public static readonly string TempPECompileDuration = BuildPropertyName(TelemetryEventName.TempPEProcessQueue, "Duration");

        /// <summary>
        ///     Indicates the reason the project was not up-to-date immediately after build.
        /// </summary>
        public static readonly string IncrementalBuildFailureReason = BuildPropertyName(TelemetryEventName.IncrementalBuildValidationFailure, "Reason");

        /// <summary>
        ///     Indicates the duration of the up-to-date check performed immediately after build to find incremental build breaks.
        /// </summary>
        public static readonly string IncrementalBuildValidationDurationMillis = BuildPropertyName(TelemetryEventName.IncrementalBuildValidationFailure, "DurationMillis");

        private static string BuildPropertyName(string eventName, string propertyName)
        {
            // Property names use the event names, but with slashes replaced by periods.
            // For example, vs/myevent would translate to vs.myevent.myproperty.
            string prefix = eventName.Replace('/', '.');

            Assumes.False(prefix.EndsWith("."));

            return prefix + "." + propertyName.ToLowerInvariant();
        }
    }
}
