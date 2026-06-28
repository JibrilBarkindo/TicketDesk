using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Data;
using TicketDesk.Api.Domain;
using TicketDesk.Api.Dtos;
using TicketDesk.Api.Services;
using Xunit;

namespace TicketDesk.Api.Tests;

public class TicketServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static TicketService CreateService(AppDbContext db) => new(db, TimeProvider.System);

    private static CreateTicketDto SampleCreate(
        string title = "Sample ticket",
        Interest priority = Interest.Medium) => new()
    {
        Title = title,
        Description = "A description with enough characters.",
        Priority = priority
    };

    [Fact]
    public async Task CreateTicket_persists_and_defaults_to_open()
    {
        await using var db = CreateContext();
        var service = CreateService(db);

        var result = await service.CreateTicketAsync(SampleCreate(priority: Interest.High));

        result.Id.Should().NotBeEmpty();
        result.Status.Should().Be(ApplicationStatus.Applied);
        result.Priority.Should().Be(Interest.High);
        (await db.Tickets.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetTicketById_returns_null_when_missing()
    {
        await using var db = CreateContext();
        var service = CreateService(db);

        var result = await service.GetTicketByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ChangeStatus_allows_valid_transition()
    {
        await using var db = CreateContext();
        var service = CreateService(db);
        var created = await service.CreateTicketAsync(SampleCreate());

        var updated = await service.ChangeStatusAsync(created.Id, ApplicationStatus.Interviewing);

        updated.Status.Should().Be(ApplicationStatus.Interviewing);
    }

    [Fact]
    public async Task ChangeStatus_rejects_invalid_transition_from_closed()
    {
        await using var db = CreateContext();
        var service = CreateService(db);
        var created = await service.CreateTicketAsync(SampleCreate());
        await service.ChangeStatusAsync(created.Id, ApplicationStatus.Rejected);

        var act = async () => await service.ChangeStatusAsync(created.Id, ApplicationStatus.Interviewing);

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [Fact]
    public async Task GetTickets_filters_by_status_and_paginates()
    {
        await using var db = CreateContext();
        var service = CreateService(db);

        for (var i = 0; i < 5; i++)
            await service.CreateTicketAsync(SampleCreate($"Open ticket {i}"));

        var toResolve = await service.CreateTicketAsync(SampleCreate("Will be resolved"));
        await service.ChangeStatusAsync(toResolve.Id, ApplicationStatus.Interviewing);

        var page = await service.GetTicketsAsync(new TicketQueryParameters
        {
            Status = ApplicationStatus.Applied,
            Page = 1,
            PageSize = 3
        });

        page.TotalCount.Should().Be(5);
        page.Items.Should().HaveCount(3);
        page.Items.Should().OnlyContain(t => t.Status == ApplicationStatus.Applied);
        page.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task UpdateTicket_throws_when_missing()
    {
        await using var db = CreateContext();
        var service = CreateService(db);

        var act = async () => await service.UpdateTicketAsync(Guid.NewGuid(), new UpdateTicketDto
        {
            Title = "New title",
            Description = "New description."
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteTicket_removes_the_ticket()
    {
        await using var db = CreateContext();
        var service = CreateService(db);
        var created = await service.CreateTicketAsync(SampleCreate());

        await service.DeleteTicketAsync(created.Id);

        (await db.Tickets.CountAsync()).Should().Be(0);
    }
}
