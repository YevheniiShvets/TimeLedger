using TimeLedger.Core.Models.Events;

namespace TimeLedger.Core.DTOs.Events;

public class RecurrenceRuleDto
{
    public RecurrenceFrequency RecurrenceFrequency { get; set; }
    public int RecurrenceInterval { get; set; } = 1;
    public string? RecurrenceValue { get; set; }
    public DateTime? RecurrenceEndTime { get; set; }
    public int? RecurrenceMaxOccurrences { get; set; }
}