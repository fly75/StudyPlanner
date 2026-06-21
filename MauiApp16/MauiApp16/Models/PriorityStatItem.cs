
namespace MauiApp16.Models
{
    /// <summary>
    /// Статистика завдань одного пріоритету.
    /// </summary>
    public class PriorityStatItem
    {
        public string Label { get; set; }           // "🔴 Високий" / …
        public int Count { get; set; }
        public double BarRatio { get; set; }        // 0..1 відносно Total
        public double BarPercent { get; set; }      // 0..100
        public string BarColor { get; set; }        // hex-рядок, напр. "#FF5252"
    }
}
