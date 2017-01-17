﻿using System;
using System.Threading.Tasks;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
{
    public class LoginViewModel : BaseSdkViewModel
    {
        public LoginViewModel()
        {
            this.ControlState = true;
        }

        #region Methods

        public async void LoginAsync()
        {
            if (!CheckInputParametersAsync()) return;

            var login = new LoginRequestListenerAsync();
            LoginResult result;
            try
            {
                this.ControlState = false;
                this.IsBusy = true;
                
                result = await login.ExecuteAsync(() => this.MegaSdk.login(this.Email, this.Password, login));
            }
            catch (BlockedAccountException)
            {
                // Do nothing, app is already logging out
                return;
            }
            finally
            {
                this.ControlState = true;
                this.IsBusy = false;
            }
           
            // Set default error content
            var errorContent = ResourceService.AppMessages.GetString("AM_LoginFailed");
            switch (result)
            {
                case LoginResult.Success:
                    {
                        SettingsService.SaveMegaLoginData(this.Email, this.MegaSdk.dumpSession());
                        // Navigate to the main page to load the main application for the user
                        NavigateService.Instance.Navigate(
                            typeof(MainPage), 
                            true,
                            NavigationObject.Create(this.GetType(), NavigationActionType.Login));
                        return;
                    }
                case LoginResult.UnassociatedEmailOrWrongPassword:
                    {
                        errorContent = ResourceService.AppMessages.GetString("AM_WrongEmailPasswordLogin");
                        break;
                    }
                case LoginResult.TooManyLoginAttempts:
                    {
                        // Too many failed login attempts. Wait one hour.
                        errorContent = string.Format(ResourceService.AppMessages.GetString("AM_TooManyFailedLoginAttempts"),
                            DateTime.Now.AddHours(1).ToString("HH:mm:ss"));
                        break;
                    }
                case LoginResult.AccountNotConfirmed:
                    {
                        errorContent = ResourceService.AppMessages.GetString("AM_AccountNotConfirmed");
                        break;
                    }
                case LoginResult.Unknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Show error message
            OnUiThread(async () => await DialogService.ShowAlertAsync(this.LoginText, errorContent));
        }

        private bool CheckInputParametersAsync()
        {
            if (string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.Password))
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(this.LoginText,
                        ResourceService.AppMessages.GetString("AM_EmptyRequiredFields"));
                });
                return false;
            }
            
            if(!ValidationService.IsValidEmail(this.Email))
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(this.LoginText,
                        ResourceService.AppMessages.GetString("AM_MalformedEmail"));
                });
                return false;
            }

            return true;
        }
        
        #endregion
        
        #region Properties

        public string Email { get; set; }
        public string Password { get; set; }
        public string SessionKey { get; set; }

        #endregion

        #region UiResources

        public string EmailWatermarkText => ResourceService.UiResources.GetString("UI_EmailWatermark");
        public string LoginText => ResourceService.UiResources.GetString("UI_Login");
        public string PasswordWatermarkText => ResourceService.UiResources.GetString("UI_PasswordWatermark");

        #endregion

        #region ProgressMessages
       
        public string ProgressHeaderText => ResourceService.ProgressMessages.GetString("PM_LoginHeader");
        public string ProgressSubHeaderText => ResourceService.ProgressMessages.GetString("PM_LoginSubHeader");

        #endregion
    }
}
