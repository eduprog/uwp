﻿using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class LoginRequestListener : BaseRequestListener
    {
        private readonly LoginViewModel _loginViewModel;
        private readonly LoginAndCreateAccountPage _loginAndCreateAccountPage;

        // Timer for ignore the received API_EAGAIN (-3) during login
        private DispatcherTimer timerAPI_EAGAIN;
        private bool isFirstAPI_EAGAIN;

        public LoginRequestListener(LoginViewModel loginViewModel, LoginAndCreateAccountPage loginAndCreateAccountPage = null)
        {
            _loginViewModel = loginViewModel;
            _loginAndCreateAccountPage = loginAndCreateAccountPage;

            timerAPI_EAGAIN = new DispatcherTimer();
            timerAPI_EAGAIN.Tick += timerTickAPI_EAGAIN;
            timerAPI_EAGAIN.Interval = new TimeSpan(0, 0, 10);            
        }

        // Method which is call when the timer event is triggered
        private async void timerTickAPI_EAGAIN(object sender, object e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                timerAPI_EAGAIN.Stop();
                //ProgressService.SetProgressIndicator(true, ProgressMessages.PM_ServersTooBusy);
            });
        }

        #region  Base Properties

        protected override string ProgressMessage
        {
            get { return App.ResourceLoaders.ProgressMessages.GetString("PM_Login"); }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return App.ResourceLoaders.AppMessages.GetString("AM_LoginFailed"); }
        }

        protected override string ErrorMessageTitle
        {
            get { return App.ResourceLoaders.AppMessages.GetString("AM_LoginFailed_Title").ToUpper(); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string SuccessMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return true; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { return (typeof(MainPage)); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { return NavigationParameter.Login; }
        }

        #endregion

        #region MRequestListenerInterface

        public async override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                //ProgressService.SetProgressIndicator(false);

                _loginViewModel.ControlState = true;

                timerAPI_EAGAIN.Stop();                
            });            

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                _loginViewModel.SessionKey = api.dumpSession();
            }
            else
            {
                //if (_loginAndCreateAccountPage != null)
                //    Deployment.Current.Dispatcher.BeginInvoke(() => _loginAndCreateAccountPage.SetApplicationBar(true));

                switch (e.getErrorCode())
                {
                    case MErrorType.API_ENOENT: // E-mail unassociated with a MEGA account or Wrong password
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            new CustomMessageDialog(ErrorMessageTitle, App.ResourceLoaders.AppMessages.GetString("AM_WrongEmailPasswordLogin"),
                                App.AppInformation, MessageDialogButtons.Ok).ShowDialogAsync());
                        return;

                    case MErrorType.API_ETOOMANY: // Too many failed login attempts. Wait one hour.
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            new CustomMessageDialog(ErrorMessageTitle,
                                String.Format(App.ResourceLoaders.AppMessages.GetString("AM_TooManyFailedLoginAttempts"), DateTime.Now.AddHours(1).ToString("HH:mm:ss")),
                                App.AppInformation, MessageDialogButtons.Ok).ShowDialogAsync());
                        return;

                    case MErrorType.API_EINCOMPLETE: // Account not confirmed
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            new CustomMessageDialog(ErrorMessageTitle, App.ResourceLoaders.AppMessages.GetString("AM_AccountNotConfirmed"),
                                App.AppInformation, MessageDialogButtons.Ok).ShowDialogAsync());
                        return;

                    case MErrorType.API_EBLOCKED: // Account blocked
                        base.onRequestFinish(api, request, e);
                        return;
                }
            }            

            base.onRequestFinish(api, request, e);
        }

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            this.isFirstAPI_EAGAIN = true;
            base.onRequestStart(api, request);
        }

        public async override void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Starts the timer when receives the first API_EAGAIN (-3)
            if (e.getErrorCode() == MErrorType.API_EAGAIN && this.isFirstAPI_EAGAIN)
            {
                this.isFirstAPI_EAGAIN = false;
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => timerAPI_EAGAIN.Start());
            }

            base.onRequestTemporaryError(api, request, e);
        }

        #endregion

        #region Override Methods

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            SettingsService.SaveMegaLoginData(_loginViewModel.Email, 
                _loginViewModel.SessionKey);

            //// Validate product subscription license on background thread
            //Task.Run(() => LicenseService.ValidateLicenses());
        }

        #endregion
    }
}
