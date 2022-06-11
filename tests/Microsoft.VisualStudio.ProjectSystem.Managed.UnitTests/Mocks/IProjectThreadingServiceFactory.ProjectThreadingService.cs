﻿// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.ProjectSystem
{
    internal partial class IProjectThreadingServiceFactory
    {
        private class ProjectThreadingService : IProjectThreadingService
        {
            private readonly bool _verifyOnUIThread;

            public ProjectThreadingService(bool verifyOnUIThread = true) => _verifyOnUIThread = verifyOnUIThread;

            public JoinableTaskContextNode JoinableTaskContext { get; } = new JoinableTaskContextNode(new JoinableTaskContext());

            public JoinableTaskFactory JoinableTaskFactory
            {
                get { return JoinableTaskContext.Factory; }
            }

            public bool IsOnMainThread
            {
                get
                {
                    if (!_verifyOnUIThread)
                        return true;

                    return JoinableTaskContext.IsOnMainThread;
                }
            }

            public void ExecuteSynchronously(Func<Task> asyncAction)
            {
                JoinableTaskFactory.Run(asyncAction);
            }

            public T ExecuteSynchronously<T>(Func<Task<T>> asyncAction)
            {
                return JoinableTaskFactory.Run(asyncAction);
            }

            public void VerifyOnUIThread()
            {
                if (!_verifyOnUIThread)
                    return;

                if (!IsOnMainThread)
                    throw new InvalidOperationException();
            }

            public IDisposable SuppressProjectExecutionContext()
            {
                return new DisposableObject();
            }

            public void Fork(Func<Task> asyncAction,
                      JoinableTaskFactory? factory = null,
                      UnconfiguredProject? project = null,
                      ConfiguredProject? configuredProject = null,
                      ErrorReportSettings? watsonReportSettings = null,
                      ProjectFaultSeverity faultSeverity = ProjectFaultSeverity.Recoverable,
                      ForkOptions options = ForkOptions.Default)
            {
                JoinableTaskFactory.Run(asyncAction);
            }

            private class DisposableObject : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}
