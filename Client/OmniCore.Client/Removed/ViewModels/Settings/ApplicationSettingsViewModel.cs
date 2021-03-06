﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Settings
{
    public class ApplicationSettingsViewModel : PageViewModel
    {
        public bool AcceptAAPSCommands { get; set; }

        //private ApplicationSettings Settings;

        public ApplicationSettingsViewModel(Page page) : base(page)
        {
        }

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        protected async override Task OnAppearing()
        {
        }

        protected async override Task OnDisappearing()
        {
            //Settings.AcceptCommandsFromAAPS = this.AcceptAAPSCommands;
            //var repo = XamarinApp.Instance.AppRepository;
            //await repo.SaveOmniCoreSettings(Settings);
        }

        protected async override Task<BaseViewModel> BindData()
        {
            //var repo = await ErosRepository.GetInstance();
            //Settings = await repo.GetOmniCoreSettings();
            //this.AcceptAAPSCommands = Settings.AcceptCommandsFromAAPS;
            return this;
        }

        protected override void OnDisposeManagedResources()
        {
        }
    }
}
