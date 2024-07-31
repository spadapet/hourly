using Hourly.Utility;

namespace Hourly.Data;

public sealed class ViewModel
{
    public Func<Task> PunchClockAction { get; set; }
    private readonly Action stateChangedAction;

    public ViewModel(Action stateChangedAction = null)
    {
        this.stateChangedAction = stateChangedAction;
    }

    public User User { get; set; }
    public bool Admin { get; set; }

    private DateTime forDayLocal = TimeUtility.LocalDate;
    public DateTime ForDayLocal
    {
        get => this.forDayLocal;
        set => this.SetProperty(ref this.forDayLocal, value.Date);
    }

    private void SetProperty<T>(ref T property, T value)
    {
        if (!EqualityComparer<T>.Default.Equals(property, value))
        {
            property = value;
            this.stateChangedAction?.Invoke();
        }
    }
}
