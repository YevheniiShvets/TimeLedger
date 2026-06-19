using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeLedger.Core.DTOs.Event;
using TimeLedger.Core.Models.Event;
using TimeLedger.Core.Services.Event;

namespace TimeLedger.Desktop.ViewModels
{
    public partial class ActivityFormViewModel : ObservableObject
    {
        private readonly MainViewModel _host;
        private readonly EventService _eventService;
        private readonly EventOwnerType _ownerType;
        private readonly int _ownerId;
        private readonly int? _editingId;

        public bool IsEditMode => _editingId.HasValue;
        public string HeaderTitle => IsEditMode ? "Edit Activity" : "Add Activity";
        public string HeaderSubtitle => IsEditMode ? "Update details for this activity" : "Create a new activity in your schedule";
        public string SubmitButtonText => IsEditMode ? "Save Changes" : "Add Activity";

        [ObservableProperty] private string _title = string.Empty;
        [ObservableProperty] private string? _description;
        [ObservableProperty] private string? _location;
        [ObservableProperty] private EventType _eventType = EventType.OneTime;
        [ObservableProperty] private bool _allowOverlap;
        [ObservableProperty] private string? _errorMessage;

        [ObservableProperty] private DateTimeOffset? _startDate;
        [ObservableProperty] private TimeSpan? _startTimeOfDay;

        [ObservableProperty] private DateTimeOffset? _endDate;
        [ObservableProperty] private TimeSpan? _endTimeOfDay;

        [ObservableProperty] private DateTimeOffset? _dueAtDate;
        [ObservableProperty] private TimeSpan? _dueAtTimeOfDay;

        [ObservableProperty] private DateTimeOffset? _recurrenceEndDate;
        [ObservableProperty] private TimeSpan? _recurrenceEndTimeOfDay;

        [ObservableProperty] private RecurrenceFrequency _recurrenceFrequency = RecurrenceFrequency.Weekly;
        [ObservableProperty] private int _recurrenceInterval = 1;
        [ObservableProperty] private string? _recurrenceValue;
        [ObservableProperty] private int? _recurrenceMaxOccurrences;

        public bool IsDeadline => EventType == EventType.Deadline;
        public bool IsRecurrence => EventType == EventType.Recurrence;
        public bool ShowStartEnd => !IsDeadline;

        public Array EventTypeOptions => Enum.GetValues(typeof(EventType));
        public Array RecurrenceFrequencyOptions => Enum.GetValues(typeof(RecurrenceFrequency));

        partial void OnEventTypeChanged(EventType value)
        {
            OnPropertyChanged(nameof(IsDeadline));
            OnPropertyChanged(nameof(IsRecurrence));
            OnPropertyChanged(nameof(ShowStartEnd));
        }

        private static DateTime? CombineDateTime(DateTimeOffset? date, TimeSpan? time)
        {
            if (date == null) return null;
            var d = date.Value.Date;
            return time.HasValue ? d.Add(time.Value) : d;
        }

        private static DateTimeOffset? ToDateOffset(DateTime? dt)
            => dt.HasValue ? new DateTimeOffset(dt.Value.Date, TimeSpan.Zero) : null;

        // Create mode
        public ActivityFormViewModel(MainViewModel host, EventService eventService, EventOwnerType ownerType, int ownerId)
        {
            _host = host;
            _eventService = eventService;
            _ownerType = ownerType;
            _ownerId = ownerId;
            _editingId = null;
        }

        // Edit mode
        public ActivityFormViewModel(MainViewModel host, EventService eventService, EventOwnerType ownerType, int ownerId, EventResponseDto existing)
        {
            _host = host;
            _eventService = eventService;
            _ownerType = ownerType;
            _ownerId = ownerId;
            _editingId = existing.Id;

            Title = existing.Title;
            Description = existing.Description;
            Location = existing.Location;
            EventType = existing.EventType;
            AllowOverlap = existing.AllowOverlap;

            StartDate = ToDateOffset(existing.StartTime);
            StartTimeOfDay = existing.StartTime?.TimeOfDay;

            EndDate = ToDateOffset(existing.EndTime);
            EndTimeOfDay = existing.EndTime?.TimeOfDay;

            DueAtDate = ToDateOffset(existing.DueAt);
            DueAtTimeOfDay = existing.DueAt?.TimeOfDay;

            if (existing.RecurrenceFrequency.HasValue)
            {
                RecurrenceFrequency = existing.RecurrenceFrequency.Value;
                RecurrenceInterval = existing.RecurrenceInterval ?? 1;
                RecurrenceMaxOccurrences = existing.RecurrenceMaxOccurrences;
                RecurrenceEndDate = ToDateOffset(existing.RecurrenceEndTime);
                RecurrenceEndTimeOfDay = existing.RecurrenceEndTime?.TimeOfDay;
                // RecurrenceValue not exposed by EventResponseDto — user must re-enter if needed
            }
        }

        [RelayCommand]
        private void Submit()
        {
            ErrorMessage = null;

            RecurrenceRuleDto? rule = IsRecurrence
                ? new RecurrenceRuleDto
                {
                    RecurrenceFrequency = RecurrenceFrequency,
                    RecurrenceInterval = RecurrenceInterval,
                    RecurrenceValue = RecurrenceValue,
                    RecurrenceEndTime = CombineDateTime(RecurrenceEndDate, RecurrenceEndTimeOfDay),
                    RecurrenceMaxOccurrences = RecurrenceMaxOccurrences
                }
                : null;

            try
            {
                if (IsEditMode)
                {
                    var dto = new UpdateEventDto
                    {
                        Title = Title,
                        Description = Description,
                        Location = Location,
                        EventType = EventType,
                        StartTime = ShowStartEnd ? CombineDateTime(StartDate, StartTimeOfDay) : null,
                        EndTime = ShowStartEnd ? CombineDateTime(EndDate, EndTimeOfDay) : null,
                        DueAt = IsDeadline ? CombineDateTime(DueAtDate, DueAtTimeOfDay) : null,
                        AllowOverlap = AllowOverlap,
                        RecurrenceRule = rule
                    };

                    var (_, hasOverlap) = _eventService.Update(_editingId!.Value, dto, _ownerType, _ownerId);
                    if (hasOverlap)
                    {
                        ErrorMessage = "This time overlaps with another event.";
                        return;
                    }
                }
                else
                {
                    var dto = new CreateEventDto
                    {
                        Title = Title,
                        Description = Description,
                        Location = Location,
                        EventType = EventType,
                        StartTime = ShowStartEnd ? CombineDateTime(StartDate, StartTimeOfDay) : null,
                        EndTime = ShowStartEnd ? CombineDateTime(EndDate, EndTimeOfDay) : null,
                        DueAt = IsDeadline ? CombineDateTime(DueAtDate, DueAtTimeOfDay) : null,
                        AllowOverlap = AllowOverlap,
                        RecurrenceRule = rule
                    };

                    var (_, hasOverlap) = _eventService.Create(dto, _ownerType, _ownerId);
                    if (hasOverlap)
                    {
                        ErrorMessage = "This time overlaps with another event.";
                        return;
                    }
                }

                _host.NavigateToTimeline();
            }
            catch (ArgumentException ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private void Delete()
        {
            if (_editingId.HasValue)
                _host.NavigateToDeleteConfirmation(_editingId.Value);
        }

        [RelayCommand]
        private void Cancel() => _host.NavigateToTimeline();
    }
}