namespace CloudShopping.Application.Features.Reports;
public static class ReportPeriod
{
    public static (DateTime Start, DateTime End) Range(DateOnly from, DateOnly to)
    {
        if (to < from || to.DayNumber - from.DayNumber > 366 || to == DateOnly.MaxValue)
            throw new ArgumentException("Período inválido: máximo de 367 dias.");
        return (DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
            DateTime.SpecifyKind(to.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc));
    }
}
