using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeLedger.Core.DTOs.Event;
using TimeLedger.Core.Models.Event;

namespace TimeLedger.Desktop.ViewModels
{
    public partial class EventItemViewModel : ObservableObject
    {
        private readonly MainViewModel? _host;

        public EventResponseDto EventDto { get; }

        public int Id => EventDto.Id;
        public string Title => EventDto.Title;
        public string? Description => EventDto.Description;
        public string? Location => EventDto.Location;
        public bool HasLocation => !string.IsNullOrWhiteSpace(Location);
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        public IBrush TypeBorderColor => EventDto.EventType switch
        {
            EventType.Recurrence => Brush.Parse("#10B981"),
            EventType.Deadline => Brush.Parse("#EF4444"),
            _ => Brush.Parse("#3B82F6")
        };

        public bool IsDeadline => EventDto.EventType == EventType.Deadline;
        public bool IsRecurrence => EventDto.EventType == EventType.Recurrence;
        public bool IsOneTime => EventDto.EventType == EventType.OneTime;

        public string TimeText => IsDeadline
            ? $"Due at {EventDto.DueAt?.ToString("HH:mm")}"
            : $"{EventDto.StartTime?.ToString("HH:mm")} - {EventDto.EndTime?.ToString("HH:mm")}";

        public string DurationText => IsDeadline ? string.Empty : CalculateDuration();

        public string RecurrenceRuleText => IsRecurrence
            ? $"every {EventDto.RecurrenceInterval} {EventDto.RecurrenceFrequency} ({EventDto.RecurrenceMaxOccurrences} times)"
            : string.Empty;

        // Constructor used by timeline rendering — wires up navigation
        public EventItemViewModel(EventResponseDto eventDto, MainViewModel host)
        {
            EventDto = eventDto;
            _host = host;
        }

        // Backward-compatible constructor without host, in case something else still uses it without nav
        public EventItemViewModel(EventResponseDto eventDto)
        {
            EventDto = eventDto;
            _host = null;
        }

        [RelayCommand]
        private void ViewDetails() => _host?.NavigateToEventDetails(Id);

        [RelayCommand]
        private void Edit() => _host?.NavigateToEditActivity(Id);

        [RelayCommand]
        private void Delete() => _host?.NavigateToDeleteConfirmation(Id);

        private string CalculateDuration()
        {
            if (EventDto.StartTime == null || EventDto.EndTime == null) return string.Empty;
            var span = EventDto.EndTime.Value - EventDto.StartTime.Value;
            if (span.TotalHours >= 24) return $"{span.TotalDays:0}d";
            if (span.Minutes == 0) return $"{span.Hours}h";
            return $"{span.Hours}h {span.Minutes}m";
        }
    }
}