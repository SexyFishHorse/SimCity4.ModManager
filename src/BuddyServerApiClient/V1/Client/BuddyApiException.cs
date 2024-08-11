namespace Asser.Sc4Buddy.Server.Api.V1.Client;

using System;
using Asser.Sc4Buddy.Server.Api.V1.Models;

public class BuddyApiException(ApiError apiError) : Exception(apiError.Error.Message)
{
    public ApiError ApiError { get; private set; } = apiError;
}