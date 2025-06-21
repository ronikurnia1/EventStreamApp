namespace EventStreamApp.Models;

public class Transfer
{
    public string Id { get; set; } = string.Empty;
    public string BookedDate { get; set; } = string.Empty;
    public string FromAccount { get; set; } = string.Empty;
    public string ToAccount { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}


public class Customer
{
    public string Id { get; set; } = string.Empty;
    public string RegisteredDate { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
}


public class SalesPromotion
{
    public string Id { get; set; } = string.Empty;
    public string RegisteredDate { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }

}