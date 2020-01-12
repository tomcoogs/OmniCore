﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface ICoreService : IDisposablesContainer, IServerResolvable
    {
        bool IsStarted { get; }
        bool IsPaused { get; }
        void RegisterDependentServices(params ICoreService[] dependentServices);
        Task StartService(CancellationToken cancellationToken);
        Task OnBeforeStopRequest();
        Task StopService(CancellationToken cancellationToken);
        Task PauseService(CancellationToken cancellationToken);
        Task ResumeService(CancellationToken cancellationToken);
    }
}
