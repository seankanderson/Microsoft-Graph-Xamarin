﻿//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Resources;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using XamarinConnect;
using Xamarin.Forms;
using System.IO;
using System.Diagnostics;

namespace XamarinConnect
{
    public partial class MainPage : ContentPage
    {
        private static GraphServiceClient graphClient = null;
        private MailHelper _mailHelper = new MailHelper();

        public MainPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            SignInSignOutBtn.Text = "connect";

            // Developer code - if you haven't registered the app yet, we warn you. 
            if (App.ClientID == "")
            {
                InfoText.Text = AppResources.NoClientIdMessage;
                SignInSignOutBtn.IsEnabled = false;
            }
            else
            {
                InfoText.Text = AppResources.ConnectPrompt;
                SignInSignOutBtn.IsEnabled = true;
            }

        }
        async void OnSignInSignOut(object sender, EventArgs e)
        {

            if (SignInSignOutBtn.Text == "connect")
            {

                graphClient = AuthenticationHelper.GetAuthenticatedClient();
                User currentUserObject = null;
                try
                {
                    currentUserObject = await graphClient.Me.Request().GetAsync();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.StackTrace);
                    
                    return;
                }
                App.Username = currentUserObject.DisplayName;
                App.UserEmail = currentUserObject.UserPrincipalName;

                InfoText.Text = "Hello, " + App.Username + ". " + AppResources.SendMailPrompt;
                MailButton.IsVisible = true;
                EmailAddressBox.IsVisible = true;
                EmailAddressBox.Text = App.UserEmail;
                SignInSignOutBtn.Text = "disconnect";
                PeopleButton.IsVisible = true;
            }
            else
            {
                AuthenticationHelper.SignOut();
                InfoText.Text = AppResources.ConnectPrompt;
                SignInSignOutBtn.Text = "connect";
                MailButton.IsVisible = false;
                EmailAddressBox.Text = "";
                EmailAddressBox.IsVisible = false;
                PeopleButton.IsVisible = false;
            }
        }



        private async void MailButton_Click(object sender, EventArgs e)
        {
            var mailAddress = EmailAddressBox.Text;
            try
            {
                byte[] pic = await _mailHelper.ComposeAndSendMailAsync(AppResources.MailSubject, AppResources.MailContents, mailAddress);
                InfoText.Text = string.Format(AppResources.SendMailSuccess, mailAddress);
                var graphClient = AuthenticationHelper.GetAuthenticatedClient();
                ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(pic));
                ProfileImage.IsVisible = true;
                

            }
            catch (ServiceException exception)
            {
                InfoText.Text = AppResources.MailErrorMessage;
                throw new Exception("We could not send the message: " + exception.Error == null ? "No error message returned." : exception.Error.Message);
                
            }


        }

        private async void PeopleButton_Clicked(object sender, EventArgs e)
        {

            graphClient = AuthenticationHelper.GetAuthenticatedClient();
            User currentUserObject = null;
            try
            {
                currentUserObject = await graphClient.Me.Request().GetAsync();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.StackTrace);
                return;
            }

            await Navigation.PushAsync(new PeoplePage(currentUserObject));
        }
    }
}
