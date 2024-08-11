namespace Asser.Sc4Buddy.Server.Api.V1.Models;

using System;

public class ApiError
{
    public DateTime Timestamp { get; set; }

    public Status Status { get; set; }

    public Error Error { get; set; }
}