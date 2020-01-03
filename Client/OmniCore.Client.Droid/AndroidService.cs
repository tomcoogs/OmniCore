﻿using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Acr.Logging;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Service.Autofill;
using Java.Lang;
using Java.Util;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Client.Droid;
using OmniCore.Client.Droid.Platform;
using OmniCore.Eros;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Radios.RileyLink;
using OmniCore.Repository.Sqlite;
using OmniCore.Services;
using Unity;
using Notification = Android.App.Notification;

namespace OmniCore.Client.Droid
{
    [Service(Exported = true, Enabled = true, DirectBootAware = true, Name = "net.balya.omnicore.commandservice", Icon="@mipmap/ic_launcher")]
    public class AndroidService : Service, ICoreServiceApi
    {
        private bool AndroidServiceStarted = false;

        public ICoreContainer<IServerResolvable> ServerContainer { get; private set; }
        public ICoreLoggingFunctions LoggingFunctions => ServerContainer.Get<ICoreLoggingFunctions>();
        public ICoreApplicationFunctions ApplicationFunctions => ServerContainer.Get<ICoreApplicationFunctions>();
        public IRepositoryService RepositoryService => ServerContainer.Get<IRepositoryService>();
        public IRadioService RadioService => ServerContainer.Get<IRadioService>();
        public IPodService PodService => ServerContainer.Get<IPodService>();
        public ICoreIntegrationService IntegrationService => ServerContainer.Get<ICoreIntegrationService>();

        private ISubject<ICoreServiceApi> UnexpectedStopRequestSubject;

        public override IBinder OnBind(Intent intent)
        {
            if (!AndroidServiceStarted)
            {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                    StartForegroundService(intent);
                else
                    StartService(intent);
            }
            return new AndroidServiceBinder(this);
        }

        public override void OnCreate()
        {
            UnexpectedStopRequestSubject = new Subject<ICoreServiceApi>();
            ServerContainer = Initializer.AndroidServiceContainer(this);
            base.OnCreate();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!AndroidServiceStarted)
            {
                CreateNotification();
                AndroidServiceStarted = true;

                var t = Task.Run(async () => await StartServices(CancellationToken.None));
                t.Wait();
                if (!t.IsCompletedSuccessfully)
                {
                    //TODO: log
                    throw new OmniCoreWorkflowException(FailureType.ServiceStartupFailure, null, t.Exception);
                }
            }

            return StartCommandResult.Sticky;
        }

        public override bool OnUnbind(Intent intent)
        {
            return base.OnUnbind(intent);
        }

        public override void OnDestroy()
        {
            UnexpectedStopRequested();
            var t = Task.Run(async () => await StopServices(CancellationToken.None));
            t.Wait();
            if (!t.IsCompletedSuccessfully)
            {
                //TODO: log
                throw new OmniCoreWorkflowException(FailureType.ServiceStopFailure, null, t.Exception);
            }
            base.OnDestroy();
        }
        private void CreateNotification()
        {
            var notificationManager = (NotificationManager) GetSystemService(NotificationService);

            var notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.ic_stat_pod)
                .SetContentTitle("OmniCore")
                .SetContentText("Service is running");

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var channelId = "OmniCoreGeneralNotifications";
                var channel = new NotificationChannel(channelId, "Service Notifications", NotificationImportance.Default)
                {
                    Description = "General service notifications."
                };

                notificationManager.CreateNotificationChannel(channel);
                notificationBuilder.SetChannelId(channelId);
            }
            var notification = notificationBuilder.Build();
            notificationManager.Notify(1, notification);
            StartForeground(1, notification);
        }

        public async Task StartServices(CancellationToken cancellationToken)
        {
            await RepositoryService.StartService(cancellationToken);
            await RadioService.StartService(cancellationToken);
            await PodService.StartService(cancellationToken);
            await IntegrationService.StartService(cancellationToken);
            
            var previousState = ApplicationFunctions.ReadPreferences(new []
            {
                ("CoreAndroidService_StopRequested_RunningServices", string.Empty),
            })[0];
            
            if (!string.IsNullOrEmpty(previousState.Value))
            {
                //TODO: check states of requests - create notifications
                
            }
            StoreRunningServicesValue($"{nameof(LoggingFunctions)},{nameof(ApplicationFunctions)}," +
                                      $"{nameof(Repository.Sqlite.RepositoryService)},{nameof(RadioService)}," +
                                      $"{nameof(PodService)},{nameof(IntegrationService)}");
        }

        private void StoreRunningServicesValue(string value)
        {
            ApplicationFunctions.StorePreferences(new []
            {
                ("CoreAndroidService_StopRequested_RunningServices", string.Empty),
            });
        }

        public async Task StopServices(CancellationToken cancellationToken)
        {
            await IntegrationService.StopService(cancellationToken);
            await PodService.StopService(cancellationToken);
            await RadioService.StopService(cancellationToken);
            await RepositoryService.StopService(cancellationToken);
        }
        
        public void UnexpectedStopRequested()
        {
            UnexpectedStopRequestSubject.OnNext(this);
        }
    }
}