using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeLedger.Core.DTOs.Event;
using TimeLedger.Core.Models.Event;
using TimeLedger.Core.Services.Event;

namespace TimeLedger.Desktop.ViewModels
{
    public partial class DeleteActivityViewModel(
        MainViewModel host,
        EventService eventService,
        EventOwnerType ownerType,
        int ownerId,
        EventResponseDto eventToDelete) : ObservableObject
    {
        public string Title => eventToDelete.Title;
        public string TimeRangeText => eventToDelete.EventType == EventType.Deadline
            ? $"Due {eventToDelete.DueAt:dd/MM/yyyy HH:mm}"
            : $"{eventToDelete.StartTime:dd/MM/yyyy HH:mm} - {eventToDelete.EndTime:dd/MM/yyyy HH:mm}";
        public string? Location => eventToDelete.Location;
        public bool HasLocation => !string.IsNullOrWhiteSpace(Location);

        [RelayCommand]
        private void ConfirmDelete()
        {
            eventService.Delete(eventToDelete.Id, ownerType, ownerId);
            host.NavigateToTimeline();
        }

        [RelayCommand]
        private void Cancel() => host.NavigateToTimeline();
    }
}