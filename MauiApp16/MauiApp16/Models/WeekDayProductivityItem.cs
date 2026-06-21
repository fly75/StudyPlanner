
namespace MauiApp16.Models
{
    /// <summary>
    /// Кількість виконаних завдань для одного дня тижня (для графіку).
    /// </summary>
    public class WeekDayProductivityItem
    {
        public string DayName { get; set; }        // "Пн", "Вт", …
        public int Count { get; set; }             // к-сть завдань
        public double BarRatio { get; set; }       // 0..1 відносно максимуму
        public double BarPercent { get; set; }     // 0..100 (для ProgressWidthConverter)
        public bool IsBestDay { get; set; }        // найпродуктивніший день
    }
}