﻿using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using MegaApp.Classes;

namespace MegaApp.Services
{
    static class NetworkService
    {
        /// <summary>
        /// Returns if there is an available network connection.
        /// </summary>        
        /// <param name="showMessageDialog">Boolean parameter to indicate if show a message if no Intenert connection</param>
        /// <returns>True if there is an available network connection., False in other case.</returns>
        public static async Task<bool> IsNetworkAvailable(bool showMessageDialog = false)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                if (showMessageDialog)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        new CustomMessageDialog(
                            App.ResourceLoaders.UiResources.GetString("UI_NoInternetConnection"),
                            App.ResourceLoaders.AppMessages.GetString("AM_NoInternetConnectionMessage"),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialogAsync();
                    });
                }

                return false;
            }

            return true;
        }

        // Code to detect if the IP has changed and refresh all open connections on this case
        public static void CheckChangesIP()
        {
            List<string> ipAddresses = null;

            // Find the IP of all network devices
            try
            {
                ipAddresses = new List<string>();
                var hostnames = NetworkInformation.GetHostNames();
                foreach (var hn in hostnames)
                {
                    if (hn.IPInformation != null)// && hn.Type == Windows.Networking.HostNameType.Ipv4)
                    {
                        string ipAddress = hn.DisplayName;
                        ipAddresses.Add(ipAddress);
                    }
                }
            }
            catch (ArgumentException) { return; }

            // If no network device is connected, do nothing
            if ((ipAddresses == null) || (ipAddresses.Count < 1))
            {
                App.IpAddress = null;
                return;
            }

            // If the primary IP has changed
            if (ipAddresses[0] != App.IpAddress)
            {
                App.MegaSdk.reconnect();        // Refresh all open connections
                App.IpAddress = ipAddresses[0]; // Storage the new primary IP address
            }
        }
    }
}