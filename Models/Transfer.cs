namespace EventStreamApp.Models;

public class Transfer
{
    public string Id { get; set; } = string.Empty;
    public DateTime BookedDate { get; set; } = DateTime.Now;
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
