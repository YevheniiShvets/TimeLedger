using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TimeLedger.Core.Interfaces;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Interfaces.Groups;
using TimeLedger.Core.Interfaces.Users;
using TimeLedger.Core.Services;
using TimeLedger.Core.Services.Event;
using TimeLedger.Infrastructure.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupInvitationRepository, GroupInvitationRepository>();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IGroupEventService, GroupEventService>();
builder.Services.AddScoped<IRecurrenceService, RecurrenceService>();
builder.Services.AddScoped<IEventOccurrenceService, EventOccurrenceService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IGroupInvitationService, GroupInvitationService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthorization();



app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();