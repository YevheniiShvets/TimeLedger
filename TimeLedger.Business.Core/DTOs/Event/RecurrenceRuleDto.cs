using TimeLedger.Core.Models.Event;

namespace TimeLedger.Core.DTOs.Event;

public class RecurrenceRuleDto
{
    public RecurrenceFrequency RecurrenceFrequency { get; set; }
    public int RecurrenceInterval { get; set; } = 1;
    public string? RecurrenceValue { get; set; }
    public DateTime? RecurrenceEndTime { get; set; }
    public int? RecurrenceMaxOccurrences { get; set; }
}