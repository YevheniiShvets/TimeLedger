using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeLedger.Core.DTOs.Event;
using TimeLedger.Core.Models.Event;
using TimeLedger.Core.Services.Event;
using TimeLedger.Infrastructure.SyncServices;

namespace TimeLedger.Desktop.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _navigationHost;
        private readonly EventService _eventService;
        private readonly SyncService _syncService;

        private readonly int? _userId;
        public bool IsOfflineMode => _userId == null;
        public bool IsOnlineMode => !IsOfflineMode;

        [ObservableProperty] private string _userName;
        [ObservableProperty] private string _userEmail;
        [ObservableProperty] private string _syncStatusMessage;

        public ObservableCollection<TimelineRowViewModel> Timeline { get; } = new();
        public IEnumerable<EventResponseDto> Events { get; set; } = [];

        private readonly EventOccurrenceService _occurrenceService;

        [ObservableProperty] private object _currentPage = null!;

        public MainViewModel(MainWindowViewModel navigationHost, int? userId, string userEmail = null, string userName = null)
        {
            _navigationHost = navigationHost;
            _userId = userId;
            _userEmail = IsOfflineMode ? "Offline Mode" : userEmail;
            _userName = IsOfflineMode ? "Local Session" : (userName ?? "User");

            _eventService = App.Services.GetService<EventService>();
            _syncService = App.Services.GetService<SyncService>();
            _occurrenceService = App.Services.GetService<EventOccurrenceService>();

            LoadEvents();
            CurrentPage = this; // Timeline is "this" (MainViewModel itself), since Timeline/Sidebar already bind to it
        }

        public void LoadEvents()
        {
            var ownerId = IsOfflineMode ? 0 : _userId.Value;
            var ownerType = EventOwnerType.User;

            var rangeStart = DateTime.Today.AddMonths(-3);
            var rangeEnd = DateTime.Today.AddMonths(3);

            var recurrenceEvents = _occurrenceService.GetOccurrencesInRange(ownerType, ownerId, rangeStart, rangeEnd).ToList();

            var oneTimeAndDeadlineEvents = _eventService.GetAll(ownerType, ownerId)
                .Where(e => e.EventType == EventType.OneTime || e.EventType == EventType.Deadline)
                .Select(e => new EventResponseDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Location = e.Location,
                    StartTime = e.StartTime ?? e.DueAt.Value,
                    EndTime = e.EndTime ?? e.DueAt.Value.AddSeconds(1),
                    OwnerType = e.OwnerType,
                    OwnerId = e.OwnerId,
                    EventType = e.EventType,
                    DueAt = e.DueAt,
                    RecurrenceInfo = e.RecurrenceInfo
                })
                .ToList();

            Events = recurrenceEvents.Concat(oneTimeAndDeadlineEvents)
                .OrderBy(e => e.StartTime)
                .ThenBy(e => e.EndTime)
                .ToList();

            var grouped = Events
                .Where(e => !e.IsDeleted)
                .GroupBy(e => GetEventDate(e))
                .OrderBy(g => g.Key);

            var flat = new List<TimelineRowViewModel>();
            foreach (var group in grouped)
            {
                flat.Add(new DayHeaderRowViewModel(group.Key));
                foreach (var e in group)
                    flat.Add(new EventRowViewModel(new EventItemViewModel(e, this)));
            }

            Timeline.Clear();
            foreach (var row in flat)
                Timeline.Add(row);
        }

        [RelayCommand(CanExecute = nameof(IsOnlineMode))]
        public void SyncNow()
        {
            if (IsOfflineMode) return;

            SyncStatusMessage = "Syncing with remote...";
            try
            {
                var result = _syncService.Sync(_userId.Value);
                if (result.ErrorMessage != null)
                {
                    SyncStatusMessage = $"Sync Failed: {result.ErrorMessage}";
                }
                else
                {
                    SyncStatusMessage = $"Synced! Pushed: {result.PushedCount}, Pulled: {result.PulledCount}";
                    LoadEvents();
                }
            }
            catch (Exception ex)
            {
                SyncStatusMessage = $"Sync error: {ex.Message}";
            }
        }

        [RelayCommand]
        public void LogOut()
        {
            _navigationHost.NavigateToLogin();
        }

        private DateTime GetEventDate(EventResponseDto e)
        {
            if (e.EventType == EventType.Deadline && e.DueAt.HasValue)
                return e.DueAt.Value.Date;
            if (e.StartTime.HasValue)
                return e.StartTime.Value.Date;
            return DateTime.Today;
        }
        private EventOwnerType OwnerType => EventOwnerType.User;
        private int OwnerId => IsOfflineMode ? 0 : _userId!.Value;

        [RelayCommand]
        public void NavigateToAddActivity()
        {
            CurrentPage = new ActivityFormViewModel(this, _eventService, OwnerType, OwnerId);
        }

        public void NavigateToEditActivity(int eventId)
        {
            var dto = _eventService.GetById(eventId, OwnerType, OwnerId);
            if (dto == null) return;
            CurrentPage = new ActivityFormViewModel(this, _eventService, OwnerType, OwnerId, dto);
        }

        public void NavigateToEventDetails(int eventId)
        {
            var dto = _eventService.GetById(eventId, OwnerType, OwnerId);
            if (dto == null) return;
            CurrentPage = new EventDetailsViewModel(this, _eventService, OwnerType, OwnerId, dto);
        }

        public void NavigateToDeleteConfirmation(int eventId)
        {
            var dto = _eventService.GetById(eventId, OwnerType, OwnerId);
            if (dto == null) return;
            CurrentPage = new DeleteActivityViewModel(this, _eventService, OwnerType, OwnerId, dto);
        }

        public void NavigateToTimeline()
        {
            LoadEvents(); // refresh after any CRUD operation
            CurrentPage = this;
        }
    }

    public abstract class TimelineRowViewModel { }

    public class DayHeaderRowViewModel : TimelineRowViewModel
    {
        public DateTime Date { get; }
        public string DateHeader => Date.ToString("dddd, dd MMM yyyy");
        public DayHeaderRowViewModel(DateTime date) => Date = date;
    }

    public class EventRowViewModel : TimelineRowViewModel
    {
        public EventItemViewModel Event { get; }
        public EventRowViewModel(EventItemViewModel evt) => Event = evt;
    }
    
}