using System;

namespace SimCity4.ModManager.App.Remote.Models;

public class BuddyServerException(string message, SimCity4.ModManager.App.Remote.Utils.ApiConnectCodes code) : Exception(message)
{
    public SimCity4.ModManager.App.Remote.Utils.ApiConnectCodes Code { get; set; } = code;
}