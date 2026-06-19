using NSubstitute;
using NSubstitute.ReturnsExtensions;
using TimeLedger.Core.DTOs.Event;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Models.Event;
using TimeLedger.Core.Services.Event;

namespace TimeLedger.Tests.Services.Event;


[TestFixture]
public class EventServiceTests
{
    private IEventRepository _repo = null!;
    private EventService _service = null!;

    private const EventOwnerType OwnerType = EventOwnerType.User;
    private const int OwnerId = 42;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Core.Models.Event.Event MakeEntity(
        int id = 1,
        EventType type = EventType.OneTime,
        DateTime? start = null,
        DateTime? end = null,
        DateTime? dueAt = null) => new()
    {
        Id        = id,
        OwnerType = OwnerType,
        OwnerId   = OwnerId,
        Title     = "Sample Event",
        EventType = type,
        StartTime = type == EventType.Deadline ? null  : start ?? new DateTime(2026, 6, 1, 9, 0, 0),
        EndTime   = type == EventType.Deadline ? null  : end   ?? new DateTime(2026, 6, 1, 10, 0, 0),
        DueAt     = type == EventType.Deadline ? (dueAt ?? new DateTime(2026, 6, 1, 17, 0, 0)) : null,
    };

    private static CreateEventDto MakeCreateDto(
        EventType type = EventType.OneTime,
        DateTime? start = null,
        DateTime? end = null,
        DateTime? dueAt = null) => new()
    {
        Title     = "New Event",
        EventType = type,
        StartTime = type == EventType.Deadline ? null  : (start ?? new DateTime(2026, 6, 1, 9, 0, 0)),
        EndTime   = type == EventType.Deadline ? null  : (end   ?? new DateTime(2026, 6, 1, 10, 0, 0)),
        DueAt     = type == EventType.Deadline ? (dueAt ?? new DateTime(2026, 6, 1, 17, 0, 0)) : null,
    };

    private static UpdateEventDto MakeUpdateDto(
        EventType type = EventType.OneTime,
        DateTime? start = null,
        DateTime? end = null,
        DateTime? dueAt = null) => new()
    {
        Title     = "Updated Event",
        EventType = type,
        StartTime = type == EventType.Deadline ? null  : (start ?? new DateTime(2026, 6, 1, 9, 0, 0)),
        EndTime   = type == EventType.Deadline ? null  : (end   ?? new DateTime(2026, 6, 1, 10, 0, 0)),
        DueAt     = type == EventType.Deadline ? (dueAt ?? new DateTime(2026, 6, 1, 17, 0, 0)) : null,
    };

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
        _repo    = Substitute.For<IEventRepository>();
        _service = new EventService(_repo);
    }

    // =========================================================================
    // GetAll
    // =========================================================================

    [Test]
    public void GetAll_ReturnsMappedDtos_ForEachEntityInRepository()
    {
        _repo.GetAll(OwnerType, OwnerId).Returns([MakeEntity(1), MakeEntity(2)]);

        var result = _service.GetAll(OwnerType, OwnerId).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetAll_ReturnsEmptyCollection_WhenRepositoryHasNoEvents()
    {
        _repo.GetAll(OwnerType, OwnerId).Returns([]);

        var result = _service.GetAll(OwnerType, OwnerId);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetAll_MapsEventType_Correctly()
    {
        _repo.GetAll(OwnerType, OwnerId).Returns([MakeEntity(type: EventType.Recurrence)]);

        var result = _service.GetAll(OwnerType, OwnerId).Single();

        Assert.That(result.EventType, Is.EqualTo(EventType.Recurrence));
    }

    [Test]
    public void GetAll_MapsDueAt_ForDeadlineEvent()
    {
        var due = new DateTime(2026, 7, 1, 12, 0, 0);
        _repo.GetAll(OwnerType, OwnerId).Returns([MakeEntity(type: EventType.Deadline, dueAt: due)]);

        var result = _service.GetAll(OwnerType, OwnerId).Single();

        Assert.That(result.DueAt, Is.EqualTo(due));
    }

    // =========================================================================
    // GetById
    // =========================================================================

    [Test]
    public void GetById_ReturnsDto_WhenEventExists()
    {
        _repo.GetById(1, OwnerType, OwnerId).Returns(MakeEntity(id: 1));

        var result = _service.GetById(1, OwnerType, OwnerId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
    }

    [Test]
    public void GetById_ReturnsNull_WhenEventDoesNotExist()
    {
        _repo.GetById(99, OwnerType, OwnerId).ReturnsNull();

        var result = _service.GetById(99, OwnerType, OwnerId);

        Assert.That(result, Is.Null);
    }

    // =========================================================================
    // Create — field validation
    // =========================================================================

    [Test]
    public void Create_Throws_WhenTitleIsEmpty()
    {
        var dto = MakeCreateDto();
        dto.Title = "";

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId),
            Throws.ArgumentException.With.Message.Contains("Title"));
    }

    [Test]
    public void Create_Throws_WhenTitleIsWhitespace()
    {
        var dto = MakeCreateDto();
        dto.Title = "   ";

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId),
            Throws.ArgumentException.With.Message.Contains("Title"));
    }

    [Test]
    public void Create_Throws_WhenTitleExceeds200Characters()
    {
        var dto = MakeCreateDto();
        dto.Title = new string('x', 201);

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId), Throws.ArgumentException);
    }

    [Test]
    public void Create_DoesNotThrow_WhenTitleIsExactly200Characters()
    {
        var dto = MakeCreateDto();
        dto.Title = new string('x', 200);
        _repo.HasOverlap(Arg.Any<DateTime>(), Arg.Any<DateTime>(), null, OwnerType, OwnerId).Returns(false);
        _repo.Add(Arg.Any<Core.Models.Event.Event>()).Returns(MakeEntity());

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId), Throws.Nothing);
    }

    [Test]
    public void Create_Throws_WhenDescriptionExceeds1000Characters()
    {
        var dto = MakeCreateDto();
        dto.Description = new string('x', 1001);

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId), Throws.ArgumentException);
    }

    [Test]
    public void Create_DoesNotThrow_WhenDescriptionIsNull()
    {
        var dto = MakeCreateDto();
        dto.Description = null;
        _repo.HasOverlap(Arg.Any<DateTime>(), Arg.Any<DateTime>(), null, OwnerType, OwnerId).Returns(false);
        _repo.Add(Arg.Any<Core.Models.Event.Event>()).Returns(MakeEntity());

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId), Throws.Nothing);
    }

    [Test]
    public void Create_Throws_WhenLocationExceeds300Characters()
    {
        var dto = MakeCreateDto();
        dto.Location = new string('x', 301);

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId), Throws.ArgumentException);
    }

    // =========================================================================
    // Create — time-range validation (OneTime / Recurrence)
    // =========================================================================

    [Test]
    public void Create_Throws_WhenStartTimeIsNull()
    {
        var dto = MakeCreateDto();
        dto.StartTime = null;

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId), Throws.ArgumentException);
    }

    [Test]
    public void Create_Throws_WhenEndTimeIsNull()
    {
        var dto = MakeCreateDto();
        dto.EndTime = null;

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId), Throws.ArgumentException);
    }

    [Test]
    public void Create_Throws_WhenStartTimeEqualsEndTime()
    {
        var time = new DateTime(2026, 6, 1, 9, 0, 0);
        var dto = MakeCreateDto(start: time, end: time);

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId), Throws.ArgumentException);
    }

    [Test]
    public void Create_Throws_WhenStartTimeIsAfterEndTime()
    {
        var dto = MakeCreateDto(
            start: new DateTime(2026, 6, 1, 11, 0, 0),
            end:   new DateTime(2026, 6, 1, 9,  0, 0));

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId), Throws.ArgumentException);
    }

    // =========================================================================
    // Create — Deadline validation
    // =========================================================================

    [Test]
    public void Create_Throws_ForDeadlineEvent_WhenDueAtIsNull()
    {
        var dto = MakeCreateDto(type: EventType.Deadline);
        dto.DueAt = null;

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId),
            Throws.ArgumentException.With.Message.Contains("Due date"));
    }

    [Test]
    public void Create_DoesNotValidateTimeRange_ForDeadlineEvent()
    {
        var dto = MakeCreateDto(type: EventType.Deadline);
        _repo.Add(Arg.Any<Core.Models.Event.Event>()).Returns(MakeEntity(type: EventType.Deadline));

        Assert.That(() => _service.Create(dto, OwnerType, OwnerId), Throws.Nothing);
    }

    // =========================================================================
    // Create — overlap logic
    // =========================================================================

    [Test]
    public void Create_ReturnsHasOverlapTrue_AndDoesNotSave_WhenOverlapExists()
    {
        var dto = MakeCreateDto();
        _repo.HasOverlap(dto.StartTime!.Value, dto.EndTime!.Value, null, OwnerType, OwnerId)
             .Returns(true);

        var (_, hasOverlap) = _service.Create(dto, OwnerType, OwnerId);

        Assert.That(hasOverlap, Is.True);
        _repo.DidNotReceive().Add(Arg.Any<Core.Models.Event.Event>());
    }

    [Test]
    public void Create_ReturnsHasOverlapFalse_AndSaves_WhenNoOverlapExists()
    {
        var dto   = MakeCreateDto();
        var saved = MakeEntity();
        _repo.HasOverlap(Arg.Any<DateTime>(), Arg.Any<DateTime>(), null, OwnerType, OwnerId)
             .Returns(false);
        _repo.Add(Arg.Any<Core.Models.Event.Event>()).Returns(saved);

        var (result, hasOverlap) = _service.Create(dto, OwnerType, OwnerId);

        Assert.That(hasOverlap, Is.False);
        Assert.That(result, Is.Not.Null);
        _repo.Received(1).Add(Arg.Any<Core.Models.Event.Event>());
    }

    [Test]
    public void Create_SkipsOverlapCheck_AndSaves_WhenAllowOverlapIsTrue()
    {
        var dto = MakeCreateDto();
        dto.AllowOverlap = true;
        _repo.Add(Arg.Any<Core.Models.Event.Event>()).Returns(MakeEntity());

        var (_, hasOverlap) = _service.Create(dto, OwnerType, OwnerId);

        Assert.That(hasOverlap, Is.False);
        _repo.DidNotReceive()
             .HasOverlap(Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                         Arg.Any<int?>(), Arg.Any<EventOwnerType>(), Arg.Any<int>());
        _repo.Received(1).Add(Arg.Any<Core.Models.Event.Event>());
    }

    [Test]
    public void Create_AssignsOwnership_ToSavedEntity()
    {
        var dto = MakeCreateDto();
        _repo.HasOverlap(Arg.Any<DateTime>(), Arg.Any<DateTime>(), null, OwnerType, OwnerId)
             .Returns(false);
        _repo.Add(Arg.Do<Core.Models.Event.Event>(e =>
        {
            Assert.That(e.OwnerType, Is.EqualTo(OwnerType));
            Assert.That(e.OwnerId,   Is.EqualTo(OwnerId));
        })).Returns(MakeEntity());

        _service.Create(dto, OwnerType, OwnerId);
    }

    // =========================================================================
    // Update — not found
    // =========================================================================

    [Test]
    public void Update_Throws_KeyNotFoundException_WhenEventDoesNotExist()
    {
        _repo.GetById(99, OwnerType, OwnerId).ReturnsNull();

        Assert.That(
            () => _service.Update(99, MakeUpdateDto(), OwnerType, OwnerId),
            Throws.TypeOf<KeyNotFoundException>());
    }

    // =========================================================================
    // Update — field validation
    // =========================================================================

    [Test]
    public void Update_Throws_WhenTitleIsEmpty()
    {
        _repo.GetById(1, OwnerType, OwnerId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Title = "";

        Assert.That(() => _service.Update(1, dto, OwnerType, OwnerId),
            Throws.ArgumentException.With.Message.Contains("Title"));
    }

    [Test]
    public void Update_Throws_WhenTitleExceeds200Characters()
    {
        _repo.GetById(1, OwnerType, OwnerId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Title = new string('x', 201);

        Assert.That(() => _service.Update(1, dto, OwnerType, OwnerId), Throws.ArgumentException);
    }

    [Test]
    public void Update_Throws_WhenDescriptionExceeds1000Characters()
    {
        _repo.GetById(1, OwnerType, OwnerId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Description = new string('x', 1001);

        Assert.That(() => _service.Update(1, dto, OwnerType, OwnerId), Throws.ArgumentException);
    }

    [Test]
    public void Update_Throws_WhenLocationExceeds300Characters()
    {
        _repo.GetById(1, OwnerType, OwnerId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Location = new string('x', 301);

        Assert.That(() => _service.Update(1, dto, OwnerType, OwnerId), Throws.ArgumentException);
    }

    [Test]
    public void Update_Throws_WhenStartTimeIsAfterEndTime()
    {
        _repo.GetById(1, OwnerType, OwnerId).Returns(MakeEntity());
        var dto = MakeUpdateDto(
            start: new DateTime(2026, 6, 1, 11, 0, 0),
            end:   new DateTime(2026, 6, 1, 9,  0, 0));

        Assert.That(() => _service.Update(1, dto, OwnerType, OwnerId), Throws.ArgumentException);
    }

    [Test]
    public void Update_Throws_WhenStartTimeEqualsEndTime()
    {
        _repo.GetById(1, OwnerType, OwnerId).Returns(MakeEntity());
        var time = new DateTime(2026, 6, 1, 9, 0, 0);
        var dto = MakeUpdateDto(start: time, end: time);

        Assert.That(() => _service.Update(1, dto, OwnerType, OwnerId), Throws.ArgumentException);
    }

    // =========================================================================
    // Update — Deadline: DueAt required, overlap skipped
    // =========================================================================

    [Test]
    public void Update_Throws_ForDeadlineDto_WhenDueAtIsNull()
    {
        _repo.GetById(1, OwnerType, OwnerId).Returns(MakeEntity(type: EventType.OneTime));
        var dto = MakeUpdateDto(type: EventType.Deadline);
        dto.DueAt = null;

        Assert.That(() => _service.Update(1, dto, OwnerType, OwnerId),
            Throws.ArgumentException.With.Message.Contains("Due date"));
    }

    [Test]
    public void Update_SkipsOverlapCheck_WhenExistingEntityIsDeadline()
    {
        // The overlap guard checks entity.EventType (the stored type), not the DTO type.
        _repo.GetById(1, OwnerType, OwnerId).Returns(MakeEntity(type: EventType.Deadline));
        var dto = MakeUpdateDto(type: EventType.Deadline);
        _repo.Update(Arg.Any<Core.Models.Event.Event>()).Returns(MakeEntity(type: EventType.Deadline));

        _service.Update(1, dto, OwnerType, OwnerId);

        _repo.DidNotReceive()
             .HasOverlap(Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                         Arg.Any<int?>(), Arg.Any<EventOwnerType>(), Arg.Any<int>());
    }

    // =========================================================================
    // Update — overlap logic (non-Deadline)
    // =========================================================================

    [Test]
    public void Update_ReturnsHasOverlapTrue_AndDoesNotSave_WhenOverlapExists()
    {
        _repo.GetById(1, OwnerType, OwnerId).Returns(MakeEntity());
        _repo.HasOverlap(Arg.Any<DateTime>(), Arg.Any<DateTime>(), 1, OwnerType, OwnerId)
             .Returns(true);

        var (_, hasOverlap) = _service.Update(1, MakeUpdateDto(), OwnerType, OwnerId);

        Assert.That(hasOverlap, Is.True);
        _repo.DidNotReceive().Update(Arg.Any<Core.Models.Event.Event>());
    }

    [Test]
    public void Update_ReturnsHasOverlapFalse_AndSaves_WhenNoOverlapExists()
    {
        var entity = MakeEntity();
        _repo.GetById(1, OwnerType, OwnerId).Returns(entity);
        _repo.HasOverlap(Arg.Any<DateTime>(), Arg.Any<DateTime>(), 1, OwnerType, OwnerId)
             .Returns(false);
        _repo.Update(Arg.Any<Core.Models.Event.Event>()).Returns(entity);

        var (_, hasOverlap) = _service.Update(1, MakeUpdateDto(), OwnerType, OwnerId);

        Assert.That(hasOverlap, Is.False);
        _repo.Received(1).Update(Arg.Any<Core.Models.Event.Event>());
    }

    [Test]
    public void Update_SkipsOverlapCheck_AndSaves_WhenAllowOverlapIsTrue()
    {
        var entity = MakeEntity();
        _repo.GetById(1, OwnerType, OwnerId).Returns(entity);
        _repo.Update(Arg.Any<Core.Models.Event.Event>()).Returns(entity);
        var dto = MakeUpdateDto();
        dto.AllowOverlap = true;

        var (_, hasOverlap) = _service.Update(1, dto, OwnerType, OwnerId);

        Assert.That(hasOverlap, Is.False);
        _repo.DidNotReceive()
             .HasOverlap(Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                         Arg.Any<int?>(), Arg.Any<EventOwnerType>(), Arg.Any<int>());
        _repo.Received(1).Update(Arg.Any<Core.Models.Event.Event>());
    }

    [Test]
    public void Update_PassesExcludeId_ToOverlapCheck_SoEventDoesNotConflictWithItself()
    {
        _repo.GetById(1, OwnerType, OwnerId).Returns(MakeEntity(id: 1));
        _repo.HasOverlap(Arg.Any<DateTime>(), Arg.Any<DateTime>(), 1, OwnerType, OwnerId)
             .Returns(false);
        _repo.Update(Arg.Any<Core.Models.Event.Event>()).Returns(MakeEntity(id: 1));

        _service.Update(1, MakeUpdateDto(), OwnerType, OwnerId);

        _repo.Received(1)
             .HasOverlap(Arg.Any<DateTime>(), Arg.Any<DateTime>(), 1, OwnerType, OwnerId);
    }

    // =========================================================================
    // Delete
    // =========================================================================

    [Test]
    public void Delete_Throws_KeyNotFoundException_WhenEventDoesNotExist()
    {
        _repo.GetById(99, OwnerType, OwnerId).ReturnsNull();

        Assert.That(() => _service.Delete(99, OwnerType, OwnerId),
            Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void Delete_CallsRepoDelete_WithCorrectEntity_WhenEventExists()
    {
        var entity = MakeEntity(id: 1);
        _repo.GetById(1, OwnerType, OwnerId).Returns(entity);

        _service.Delete(1, OwnerType, OwnerId);

        _repo.Received(1).Delete(entity);
    }

    [Test]
    public void Delete_DoesNotCallRepoDelete_WhenEventNotFound()
    {
        _repo.GetById(99, OwnerType, OwnerId).ReturnsNull();

        try { _service.Delete(99, OwnerType, OwnerId); } catch (KeyNotFoundException) { }

        _repo.DidNotReceive().Delete(Arg.Any<Core.Models.Event.Event>());
    }

    // =========================================================================
    // EventResponseDto.Duration — computed property
    // =========================================================================

    [Test]
    public void Duration_ReturnsNull_WhenStartTimeIsNull()
    {
        var dto = new EventResponseDto { StartTime = null, EndTime = new DateTime(2026, 6, 1, 10, 0, 0) };

        Assert.That(dto.Duration, Is.Null);
    }

    [Test]
    public void Duration_ReturnsNull_WhenEndTimeIsNull()
    {
        var dto = new EventResponseDto { StartTime = new DateTime(2026, 6, 1, 9, 0, 0), EndTime = null };

        Assert.That(dto.Duration, Is.Null);
    }

    [Test]
    public void Duration_ReturnsMinutesOnly_WhenUnderOneHour()
    {
        var dto = new EventResponseDto
        {
            StartTime = new DateTime(2026, 6, 1, 9,  0, 0),
            EndTime   = new DateTime(2026, 6, 1, 9, 45, 0)
        };

        Assert.That(dto.Duration, Is.EqualTo("45m"));
    }

    [Test]
    public void Duration_ReturnsHoursOnly_WhenExactHours()
    {
        var dto = new EventResponseDto
        {
            StartTime = new DateTime(2026, 6, 1, 9,  0, 0),
            EndTime   = new DateTime(2026, 6, 1, 11, 0, 0)
        };

        Assert.That(dto.Duration, Is.EqualTo("2h"));
    }

    [Test]
    public void Duration_ReturnsHoursAndMinutes_WhenUnderOneDay()
    {
        var dto = new EventResponseDto
        {
            StartTime = new DateTime(2026, 6, 1, 9,  0,  0),
            EndTime   = new DateTime(2026, 6, 1, 11, 30, 0)
        };

        Assert.That(dto.Duration, Is.EqualTo("2h 30m"));
    }

    [Test]
    public void Duration_ReturnsDaysOnly_WhenExactWholeDays()
    {
        var dto = new EventResponseDto
        {
            StartTime = new DateTime(2026, 6, 1, 9, 0, 0),
            EndTime   = new DateTime(2026, 6, 3, 9, 0, 0)
        };

        Assert.That(dto.Duration, Is.EqualTo("2d"));
    }

    [Test]
    public void Duration_ReturnsDaysAndHours_WhenOverOneDayWithRemainder()
    {
        var dto = new EventResponseDto
        {
            StartTime = new DateTime(2026, 6, 1, 9,  0, 0),
            EndTime   = new DateTime(2026, 6, 2, 11, 0, 0)
        };

        Assert.That(dto.Duration, Is.EqualTo("1d 2h"));
    }
}