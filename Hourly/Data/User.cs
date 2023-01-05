namespace Hourly.Data;

public sealed class User
{
    public string Name { get; set; }
    public string Password { get; set; }
    public string AdminPassword { get; set; }
    public string Partition { get; set; } // in storage table
    public PayPeriodType PayPeriodType { get; set; }
    public DateTime FirstWorkDayLocal { get; set; }
}

public sealed class Users
{
    public Dictionary<string, User> ById { get; } = new();
}
