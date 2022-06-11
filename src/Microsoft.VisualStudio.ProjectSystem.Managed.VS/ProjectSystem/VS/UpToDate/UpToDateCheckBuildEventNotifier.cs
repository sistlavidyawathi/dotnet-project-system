﻿// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using Microsoft.VisualStudio.ProjectSystem.UpToDate;
using Microsoft.VisualStudio.ProjectSystem.VS.Build;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using IAsyncDisposable = System.IAsyncDisposable;

namespace Microsoft.VisualStudio.ProjectSystem.VS.UpToDate
{
    /// <summary>
    /// Listens for build events and notifies the fast up-to-date check of them
    /// via <see cref="IBuildUpToDateCheckProviderInternal"/>.
    /// </summary>
    [Export(ExportContractNames.Scopes.UnconfiguredProject, typeof(IProjectDynamicLoadComponent))]
    [AppliesTo(BuildUpToDateCheck.AppliesToExpression)]
    internal sealed class UpToDateCheckBuildEventNotifier : OnceInitializedOnceDisposedAsync, IVsUpdateSolutionEvents2, IProjectDynamicLoadComponent
    {
        private readonly IProjectService _projectService;
        private readonly ISolutionBuildManager _solutionBuildManager;
        private readonly IProjectThreadingService _threadingService;
        private readonly IProjectFaultHandlerService _faultHandlerService;
        private IAsyncDisposable? _solutionBuildEventsSubscription;

        [ImportingConstructor]
        public UpToDateCheckBuildEventNotifier(
            JoinableTaskContext joinableTaskContext,
            IProjectService projectService,
            IProjectThreadingService threadingService,
            IProjectFaultHandlerService faultHandlerService,
            ISolutionBuildManager solutionBuildManager)
            : base(new(joinableTaskContext))
        {
            _projectService = projectService;
            _threadingService = threadingService;
            _faultHandlerService = faultHandlerService;
            _solutionBuildManager = solutionBuildManager;
        }

        public Task LoadAsync() => InitializeAsync();
        public Task UnloadAsync() => DisposeAsync();

        protected override async Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            _solutionBuildEventsSubscription = await _solutionBuildManager.SubscribeSolutionEventsAsync(this);
        }

        protected override Task DisposeCoreAsync(bool initialized)
        {
            if (initialized)
            {
                if (_solutionBuildEventsSubscription is not null)
                {
                    return _solutionBuildEventsSubscription.DisposeAsync().AsTask();
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Called right before a project configuration starts building.
        /// </summary>
        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            if (IsBuild(dwAction, out _))
            {
                IEnumerable<IBuildUpToDateCheckProviderInternal>? providers = FindActiveConfiguredProviders(pHierProj, out _);

                if (providers is not null)
                {
                    foreach (IBuildUpToDateCheckProviderInternal provider in providers)
                    {
                        provider.NotifyBuildStarting(DateTime.UtcNow);
                    }
                }
            }

            return HResult.OK;
        }

        /// <summary>
        /// Called right after a project configuration is finished building.
        /// </summary>
        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            if (fCancel == 0 && IsBuild(dwAction, out bool isRebuild))
            {
                IEnumerable<IBuildUpToDateCheckProviderInternal>? providers = FindActiveConfiguredProviders(pHierProj, out UnconfiguredProject? unconfiguredProject);

                if (providers is not null)
                {
                    JoinableTask task = _threadingService.JoinableTaskFactory.RunAsync(async () =>
                    {
                        // Do this work off the main thread
                        await TaskScheduler.Default;

                        foreach (IBuildUpToDateCheckProviderInternal provider in providers)
                        {
                            await provider.NotifyBuildCompletedAsync(wasSuccessful: fSuccess != 0, isRebuild);
                        }
                    });

                    _faultHandlerService.Forget(task.Task, unconfiguredProject);
                }
            }

            return HResult.OK;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="options"/> indicates either a build or rebuild.
        /// </summary>
        private static bool IsBuild(uint options, out bool isRebuild)
        {
            const VSSOLNBUILDUPDATEFLAGS anyBuildFlags = VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD;
            const VSSOLNBUILDUPDATEFLAGS rebuildFlags = VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD | VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_FORCE_UPDATE;

            var operation = (VSSOLNBUILDUPDATEFLAGS)options;

            isRebuild = (operation & rebuildFlags) == rebuildFlags;

            return (operation & anyBuildFlags) == anyBuildFlags;
        }

        private IEnumerable<IBuildUpToDateCheckProviderInternal>? FindActiveConfiguredProviders(IVsHierarchy vsHierarchy, out UnconfiguredProject? unconfiguredProject)
        {
            unconfiguredProject = _projectService.GetUnconfiguredProject(vsHierarchy, appliesToExpression: BuildUpToDateCheck.AppliesToExpression);

            if (unconfiguredProject is not null)
            {
                IActiveConfiguredProjectProvider activeConfiguredProjectProvider = unconfiguredProject.Services.ExportProvider.GetExportedValue<IActiveConfiguredProjectProvider>();

                ConfiguredProject? configuredProject = activeConfiguredProjectProvider.ActiveConfiguredProject;

                if (configuredProject is not null)
                {
                    return configuredProject.Services.ExportProvider.GetExportedValues<IBuildUpToDateCheckProviderInternal>();
                }
            }

            return null;
        }

        #region IVsUpdateSolutionEvents stubs

        int IVsUpdateSolutionEvents.UpdateSolution_StartUpdate(ref int pfCancelUpdate) => HResult.OK;
        int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate) => HResult.OK;
        int IVsUpdateSolutionEvents.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand) => HResult.OK;
        int IVsUpdateSolutionEvents.UpdateSolution_Cancel() => HResult.OK;
        int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => HResult.OK;

        int IVsUpdateSolutionEvents2.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => HResult.OK;
        int IVsUpdateSolutionEvents2.UpdateSolution_Begin(ref int pfCancelUpdate) => HResult.OK;
        int IVsUpdateSolutionEvents2.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand) => HResult.OK;
        int IVsUpdateSolutionEvents2.UpdateSolution_StartUpdate(ref int pfCancelUpdate) => HResult.OK;
        int IVsUpdateSolutionEvents2.UpdateSolution_Cancel() => HResult.OK;

        #endregion
    }
}
