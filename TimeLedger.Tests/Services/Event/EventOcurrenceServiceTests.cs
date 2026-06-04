using NSubstitute;
using TimeLedger.Core.DTOs.Events;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Models.Events;
using TimeLedger.Core.Services.Event;
using EventModel = TimeLedger.Core.Models.Events.Event;

namespace TimeLedger.Tests.Services.Event;

[TestFixture]
public class EventOcurrenceServiceTests
{
    private IEventRepository _eventRepository = null!;
    private IRecurrenceService _recurrenceService = null!;
    private IEventOccurrenceService _service = null!;

    private const EventOwnerType OwnerType = EventOwnerType.User;
    private const int OwnerId = 42;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static EventModel MakeRecurrenceEntity(
        int id = 1,
        string title = "Weekly Sync",
        DateTime? startTime = null,
        DateTime? endTime = null,
        RecurrenceFrequency? frequency = RecurrenceFrequency.Weekly,
        int? interval = 1,
        string? recurrenceValue = "1,3",
        DateTime? recurrenceEndTime = null,
        int? recurrenceMaxOccurrences = 10) => new()
    {
        Id = id,
        OwnerType = OwnerType,
        OwnerId = OwnerId,
        Title = title,
        Description = "Description",
        Location = "Room 1",
        EventType = EventType.Recurrence,
        StartTime = startTime ?? new DateTime(2026, 6, 1, 9, 0, 0),
        EndTime = endTime ?? new DateTime(2026, 6, 1, 10, 0, 0),
        RecurrenceFrequency = frequency,
        RecurrenceInterval = interval,
        RecurrenceValue = recurrenceValue,
        RecurrenceEndTime = recurrenceEndTime,
        RecurrenceMaxOccurrences = recurrenceMaxOccurrences
    };

    private static EventModel MakeInvalidRecurrenceEntity(
        int id = 1,
        bool missingStart = false,
        bool missingEnd = false,
        bool missingFrequency = false) => new()
    {
        Id = id,
        OwnerType = OwnerType,
        OwnerId = OwnerId,
        Title = "Invalid Recurrence",
        EventType = EventType.Recurrence,
        StartTime = missingStart ? null : new DateTime(2026, 6, 1, 9, 0, 0),
        EndTime = missingEnd ? null : new DateTime(2026, 6, 1, 10, 0, 0),
        RecurrenceFrequency = missingFrequency ? null : RecurrenceFrequency.Weekly,
        RecurrenceInterval = 1,
        RecurrenceValue = "1,3",
        RecurrenceEndTime = new DateTime(2026, 12, 31, 0, 0, 0),
        RecurrenceMaxOccurrences = 10
    };

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
        _eventRepository = Substitute.For<IEventRepository>();
        _recurrenceService = Substitute.For<IRecurrenceService>();
        _service = new EventOccurrenceService(_eventRepository, _recurrenceService);
    }

    // =========================================================================
    // GetOccurrencesInRange
    // =========================================================================

    [Test]
    public void GetOccurrencesInRange_ReturnsEmptyCollection_WhenRepositoryHasNoRecurrenceEvents()
    {
        // Arrange
        _eventRepository.GetByType(EventType.Recurrence, OwnerType, OwnerId)
            .Returns(Array.Empty<EventModel>());

        // Act
        var result = _service.GetOccurrencesInRange(OwnerType, OwnerId,
            new DateTime(2026, 6, 1), new DateTime(2026, 6, 30));

        // Assert
        Assert.That(result, Is.Empty);
        _recurrenceService.DidNotReceive().GenerateOccurrences(
            Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<RecurrenceRuleDto>(),
            Arg.Any<DateTime>(), Arg.Any<DateTime>());
    }

    [Test]
    public void GetOccurrencesInRange_SkipsEvents_WithMissingRequiredRecurrenceData()
    {
        // Arrange
        _eventRepository.GetByType(EventType.Recurrence, OwnerType, OwnerId)
            .Returns([
                MakeInvalidRecurrenceEntity(id: 1, missingStart: true),
                MakeInvalidRecurrenceEntity(id: 2, missingEnd: true),
                MakeInvalidRecurrenceEntity(id: 3, missingFrequency: true)
            ]);

        // Act
        var result = _service.GetOccurrencesInRange(OwnerType, OwnerId,
            new DateTime(2026, 6, 1), new DateTime(2026, 6, 30));

        // Assert
        Assert.That(result, Is.Empty);
        _recurrenceService.DidNotReceiveWithAnyArgs().GenerateOccurrences(default, default, null!, default, default);
    }

    [Test]
    public void GetOccurrencesInRange_UsesRecurrenceService_WithExpectedRuleAndRange()
    {
        // Arrange
        var entity = MakeRecurrenceEntity(
            recurrenceEndTime: new DateTime(2026, 7, 1, 0, 0, 0),
            recurrenceMaxOccurrences: 5);

        var rangeStart = new DateTime(2026, 6, 1, 0, 0, 0);
        var rangeEnd = new DateTime(2026, 6, 30, 23, 59, 59);
        var generated = new[]
        {
            (new DateTime(2026, 6, 3, 9, 0, 0), new DateTime(2026, 6, 3, 10, 0, 0))
        };

        _eventRepository.GetByType(EventType.Recurrence, OwnerType, OwnerId)
            .Returns([entity]);
        _recurrenceService.GenerateOccurrences(
                entity.StartTime!.Value,
                entity.EndTime!.Value,
                Arg.Any<RecurrenceRuleDto>(),
                rangeStart,
                rangeEnd)
            .Returns(generated);

        // Act
        var result = _service.GetOccurrencesInRange(OwnerType, OwnerId, rangeStart, rangeEnd).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        _recurrenceService.Received(1).GenerateOccurrences(
            entity.StartTime.Value,
            entity.EndTime.Value,
            Arg.Is<RecurrenceRuleDto>(rule =>
                rule.RecurrenceFrequency == RecurrenceFrequency.Weekly &&
                rule.RecurrenceInterval == 1 &&
                rule.RecurrenceValue == "1,3" &&
                rule.RecurrenceEndTime == entity.RecurrenceEndTime &&
                rule.RecurrenceMaxOccurrences == 5),
            rangeStart,
            rangeEnd);
    }

    [Test]
    public void GetOccurrencesInRange_MapsGeneratedOccurrences_ToResponseDtos()
    {
        // Arrange
        var entity = MakeRecurrenceEntity(id: 10, title: "Team Meeting");
        var rangeStart = new DateTime(2026, 6, 1, 0, 0, 0);
        var rangeEnd = new DateTime(2026, 6, 30, 23, 59, 59);
        var start = new DateTime(2026, 6, 3, 9, 0, 0);
        var end = new DateTime(2026, 6, 3, 10, 0, 0);

        _eventRepository.GetByType(EventType.Recurrence, OwnerType, OwnerId)
            .Returns([entity]);
        _recurrenceService.GenerateOccurrences(
                Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<RecurrenceRuleDto>(), rangeStart, rangeEnd)
            .Returns([(start, end)]);

        // Act
        var result = _service.GetOccurrencesInRange(OwnerType, OwnerId, rangeStart, rangeEnd).Single();

        Assert.Multiple(() => // Something something optimisation
        {
            // Assert
            Assert.That(result.Id, Is.EqualTo(10));
            Assert.That(result.Title, Is.EqualTo("Team Meeting"));
            Assert.That(result.StartTime, Is.EqualTo(start));
            Assert.That(result.EndTime, Is.EqualTo(end));
            Assert.That(result.OwnerType, Is.EqualTo(OwnerType));
            Assert.That(result.OwnerId, Is.EqualTo(OwnerId));
            Assert.That(result.EventType, Is.EqualTo(EventType.Recurrence));
            Assert.That(result.RecurrenceInfo, Is.EqualTo("weekly (10 times)"));
        });
    }

    [Test]
    public void GetOccurrencesInRange_OrdersResults_ByStartTimeThenEndTime()
    {
        // Arrange
        var entity = MakeRecurrenceEntity();
        var rangeStart = new DateTime(2026, 6, 1, 0, 0, 0);
        var rangeEnd = new DateTime(2026, 6, 30, 23, 59, 59);

        _eventRepository.GetByType(EventType.Recurrence, OwnerType, OwnerId)
            .Returns([entity]);
        _recurrenceService.GenerateOccurrences(
                Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<RecurrenceRuleDto>(), rangeStart, rangeEnd)
            .Returns([
                (new DateTime(2026, 6, 4, 9, 0, 0), new DateTime(2026, 6, 4, 10, 0, 0)),
                (new DateTime(2026, 6, 3, 9, 0, 0), new DateTime(2026, 6, 3, 11, 0, 0)),
                (new DateTime(2026, 6, 3, 9, 0, 0), new DateTime(2026, 6, 3, 10, 0, 0))
            ]);

        // Act
        var result = _service.GetOccurrencesInRange(OwnerType, OwnerId, rangeStart, rangeEnd).ToList();

        // Assert
        Assert.That(result.Select(x => x.StartTime), Is.EqualTo(new[]
        {
            new DateTime(2026, 6, 3, 9, 0, 0),
            new DateTime(2026, 6, 3, 9, 0, 0),
            new DateTime(2026, 6, 4, 9, 0, 0)
        }));
        Assert.That(result[0].EndTime, Is.EqualTo(new DateTime(2026, 6, 3, 10, 0, 0)));
        Assert.That(result[1].EndTime, Is.EqualTo(new DateTime(2026, 6, 3, 11, 0, 0)));
    }

    [Test]
    public void GetOccurrencesInRange_ReturnsEmpty_WhenRecurrenceServiceGeneratesNoMatches()
    {
        // Arrange
        var entity = MakeRecurrenceEntity();
        var rangeStart = new DateTime(2026, 6, 1, 0, 0, 0);
        var rangeEnd = new DateTime(2026, 6, 30, 23, 59, 59);

        _eventRepository.GetByType(EventType.Recurrence, OwnerType, OwnerId)
            .Returns([entity]);
        _recurrenceService.GenerateOccurrences(
                Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<RecurrenceRuleDto>(), rangeStart, rangeEnd)
            .Returns(Array.Empty<(DateTime start, DateTime end)>());

        // Act
        var result = _service.GetOccurrencesInRange(OwnerType, OwnerId, rangeStart, rangeEnd);

        // Assert
        Assert.That(result, Is.Empty);
    }

    // =========================================================================
    // GetOccurrencesForWeek
    // =========================================================================

    [Test]
    public void GetOccurrencesForWeek_ReturnsOccurrencesInSevenDayRange()
    {
        // Arrange
        var entity = MakeRecurrenceEntity();
        var weekStartDate = new DateTime(2026, 6, 1, 0, 0, 0);
        var weekEndDate = weekStartDate.AddDays(7);
        var generated = new[]
        {
            (new DateTime(2026, 6, 2, 9, 0, 0), new DateTime(2026, 6, 2, 10, 0, 0))
        };

        _eventRepository.GetByType(EventType.Recurrence, OwnerType, OwnerId)
            .Returns([entity]);
        _recurrenceService.GenerateOccurrences(
                Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<RecurrenceRuleDto>(), weekStartDate, weekEndDate)
            .Returns(generated);

        // Act
        var result = _service.GetOccurrencesForWeek(OwnerType, OwnerId, weekStartDate).Single();

        // Assert
        Assert.That(result.StartTime, Is.EqualTo(new DateTime(2026, 6, 2, 9, 0, 0)));
        Assert.That(result.EndTime, Is.EqualTo(new DateTime(2026, 6, 2, 10, 0, 0)));
        _recurrenceService.Received(1).GenerateOccurrences(
            entity.StartTime!.Value,
            entity.EndTime!.Value,
            Arg.Any<RecurrenceRuleDto>(),
            weekStartDate,
            weekEndDate);
    }

    [Test]
    public void GetOccurrencesForWeek_ReturnsEmpty_WhenNoRecurrenceEventsExist()
    {
        // Arrange
        var weekStartDate = new DateTime(2026, 6, 1, 0, 0, 0);

        _eventRepository.GetByType(EventType.Recurrence, OwnerType, OwnerId)
            .Returns([]);

        // Act
        var result = _service.GetOccurrencesForWeek(OwnerType, OwnerId, weekStartDate).ToList();

        // Assert
        Assert.That(result, Is.Empty);
        _recurrenceService.DidNotReceive().GenerateOccurrences(
            Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<RecurrenceRuleDto>(),
            Arg.Any<DateTime>(), Arg.Any<DateTime>());
    }
}

