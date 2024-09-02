﻿namespace EventStreamApp.Models;

public class Transfer
{
    public string Id { get; set; } = string.Empty;
    public DateOnly BookedDate { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
