using Bogus;
using EventStreamApp.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventStreamApp.Services;

public interface IFakeDataService
{
    public string GetFakeTransferData();
    public string GetFakeCustomerData();
}

public class FakeDataService : IFakeDataService
{
    private readonly string[] transferTypes = ["BI FAST", "ONLINE", "LLG", "RTGS"];
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public string GetFakeTransferData()
    {
        var fakeTransfer = new Faker<Transfer>("id_ID")
            .StrictMode(true)
            .RuleFor(t => t.Id, f => $"TRF-{f.Finance.Account(4)}")
            .RuleFor(t => t.FromAccount, f => $"{f.Finance.Account(8)}")
            .RuleFor(t => t.ToAccount, f => $"{f.Finance.Account(8)}")
            .RuleFor(t => t.Amount, f => f.Random.Number(50000, 20000001))
            .RuleFor(t => t.BookedDate, f => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            .RuleFor(t => t.Type, f => f.PickRandom(transferTypes));

        return JsonSerializer.Serialize(fakeTransfer.Generate(), jsonSerializerOptions);
    }

    public string GetFakeCustomerData()
    {
        var fakeCustomer = new Faker<Customer>("id_ID")
            .StrictMode(true)
            .RuleFor(t => t.Id, f => $"CST-{f.Finance.Account(4)}")
            .RuleFor(t => t.AccountNo, f => $"{f.Finance.Account(8)}")
            .RuleFor(t => t.Address, f => $"{f.Address.StreetAddress()}, {f.Address.City()}")
            .RuleFor(t => t.Name, f => f.Name.FullName())
            .RuleFor(t => t.RegisteredDate, f => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        return JsonSerializer.Serialize(fakeCustomer.Generate(), jsonSerializerOptions);
    }
}



