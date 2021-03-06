﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Eros
{
    public class ErosPod : IPod
    {

        private readonly IUnityContainer Container;
        private readonly IPodRequestRepository PodRequestRepository;
        private readonly ITaskQueue TaskQueue;

        public ErosPod(IUnityContainer container,
            IPodRequestRepository podRequestRepository,
            ITaskQueue taskQueue)
        {
            Container = container;
            PodRequestRepository = podRequestRepository;
            TaskQueue = taskQueue;
        }

        public IPodEntity Entity { get; set; }
        public Task Archive()
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<IPodRequest>> GetActiveRequests()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestPair()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestPrime()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestInsert()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestStatus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestConfigureAlerts()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestAcknowledgeAlerts()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestSetBasalSchedule()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestCancelBasal()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestSetTempBasal()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestCancelTempBasal()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestBolus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestCancelBolus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestStartExtendedBolus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestCancelExtendedBolus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestDeactivate()
        {
            throw new System.NotImplementedException();
        }

        public async Task StartQueue()
        {
            await TaskQueue.Startup();
        }

        public async Task StopQueue()
        {
            await TaskQueue.Shutdown();
        }

        private async Task<IPodRequest> CreatePodRequest()
        {
            var request = Container.Resolve<IPodRequest>(RegistrationConstants.OmnipodEros);
            request.Pod = this;

            request.Entity = PodRequestRepository.New();
            request.Entity.Pod = this.Entity;

            return request;
        }
    }
}