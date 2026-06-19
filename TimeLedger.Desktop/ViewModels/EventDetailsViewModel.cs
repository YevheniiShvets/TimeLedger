using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeLedger.Core.DTOs.Event;
using TimeLedger.Core.Models.Event;
using TimeLedger.Core.Services.Event;

namespace TimeLedger.Desktop.ViewModels
{
    public partial class EventDetailsViewModel(
        MainViewModel host,
        EventService eventService,
        EventOwnerType ownerType,
        int ownerId,
        EventResponseDto eventDto) : ObservableObject
    {
        public EventResponseDto Event => eventDto;

        public string Title => eventDto.Title;
        public string EventTypeText => eventDto.EventType.ToString();
        public string TimeRangeText => eventDto.EventType == EventType.Deadline
            ? $"Due {eventDto.DueAt:dd/MM/yyyy HH:mm}"
            : $"{eventDto.StartTime:dd/MM/yyyy HH:mm} - {eventDto.EndTime:dd/MM/yyyy HH:mm}";
        public string? Location => eventDto.Location;
        public bool HasLocation => !string.IsNullOrWhiteSpace(Location);
        public string? Description => eventDto.Description;
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
        public string? RecurrenceInfo => eventDto.RecurrenceInfo;
        public bool HasRecurrenceInfo => !string.IsNullOrWhiteSpace(RecurrenceInfo);

        [RelayCommand]
        private void Edit() => host.NavigateToEditActivity(eventDto.Id);

        [RelayCommand]
        private void Delete() => host.NavigateToDeleteConfirmation(eventDto.Id);

        [RelayCommand]
        private void Back() => host.NavigateToTimeline();
    }
}