namespace Hourly.Data;

public sealed class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public string Partition { get; set; }
    public PayPeriodType PayPeriodType { get; set; }
    public DateTime FirstWorkDay { get; set; }
    public bool DevOnly { get; set; }
}

public sealed class Users
{
    public Dictionary<string, User> ById { get; } = new();
}
