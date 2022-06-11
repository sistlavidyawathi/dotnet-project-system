﻿// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using Microsoft.VisualStudio.LanguageServices.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.VS;

namespace Microsoft.VisualStudio.ProjectSystem.LanguageServices
{
    internal static class IEvaluationHandlerFactory
    {
        public static IProjectEvaluationHandler ImplementProjectEvaluationRule(string evaluationRule)
        {
            var mock = new Mock<IProjectEvaluationHandler>();

            mock.SetupGet(h => h.ProjectEvaluationRule)
                .Returns(evaluationRule);

            return mock.Object;
        }

        public static IProjectEvaluationHandler ImplementHandle(string evaluationRule, Action<IWorkspaceProjectContext, ProjectConfiguration, IComparable, IProjectChangeDescription, ContextState, IProjectDiagnosticOutputService> action)
        {
            var mock = new Mock<IProjectEvaluationHandler>();

            mock.SetupGet(h => h.ProjectEvaluationRule)
                .Returns(evaluationRule);

            mock.Setup(h => h.Handle(It.IsAny<IWorkspaceProjectContext>(), It.IsAny<ProjectConfiguration>(), It.IsAny<IComparable>(), It.IsAny<IProjectChangeDescription>(), It.IsAny<ContextState>(), It.IsAny<IProjectDiagnosticOutputService>()))
                .Callback(action);

            return mock.Object;
        }
    }
}
