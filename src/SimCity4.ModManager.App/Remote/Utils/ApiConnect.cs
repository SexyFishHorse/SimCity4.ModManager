using System.Net.NetworkInformation;
using RestSharp;

namespace SimCity4.ModManager.App.Remote.Utils;

public static class ApiConnect
{
    public static void ThrowErrorOnConnectionOrDisabledFeature(string feature)
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
        {
            throw new SimCity4.ModManager.App.Remote.Models.BuddyServerException("No internet connection available.", ApiConnectCodes.NoNetworkConnection);
        }

        if (string.IsNullOrWhiteSpace(SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.ApiBaseUrl)))
        {
            throw new SimCity4.ModManager.App.Remote.Models.BuddyServerException("Api base url is not defined.", ApiConnectCodes.NoBaseApiDefined);
        }

        if (!SimCity4.ModManager.App.Configuration.Settings.Get<bool>(feature))
        {
            throw new SimCity4.ModManager.App.Remote.Models.BuddyServerException("The feature " + feature + " is disabled.", ApiConnectCodes.FeatureDisabled);
        }
    }

    public static bool HasConnectionAndIsFeatureEnabled(string feature)
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.ApiBaseUrl)))
        {
            return false;
        }

        return SimCity4.ModManager.App.Configuration.Settings.Get<bool>(feature);
    }

    public static IRestClient GetClient()
    {
        return new RestClient(SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.ApiBaseUrl));
    }
}