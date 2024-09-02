using Bogus;
using EventStreamApp.Models;
using System.Text.Json;

namespace EventStreamApp.Services;

public interface IFakeDataService
{
    public string GetFakeTransferData();
}

public class FakeDataService : IFakeDataService
{
    private readonly string[] transferTypes = ["BI FAST", "ONLINE", "LLG", "RTGS"];
    private readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public string GetFakeTransferData()
    {
        var fakeTransfer = new Faker<Transfer>("en")
            .StrictMode(true)
            .RuleFor(t => t.Id, f => $"TRF-{f.Finance.Account(4)}")
            .RuleFor(t => t.Source, f => $"{f.Finance.Account(8)}")
            .RuleFor(t => t.Target, f => $"{f.Finance.Account(8)}")
            .RuleFor(t => t.Amount, f => f.Random.Number(50000, 20000001))
            .RuleFor(t => t.BookedDate, f => DateOnly.FromDateTime(f.Date.Between(DateTime.Today.AddDays(-30), DateTime.Today)))
            .RuleFor(t => t.Type, f => f.PickRandom(transferTypes));

        return JsonSerializer.Serialize(fakeTransfer.Generate(), jsonSerializerOptions);
    }


}
