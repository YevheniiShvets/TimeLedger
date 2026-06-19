using BusinessCollaboration.Interfaces.Event;
using BusinessCollaboration.Interfaces.Group;
using BusinessCollaboration.Interfaces.User;
using BusinessCollaboration.Services.Event;
using BusinessCollaboration.Services.Group;
using BusinessCollaboration.Services.User;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Services.Event;
using TimeLedger.Infrastructure.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Services for Event Management
builder.Services.AddScoped<EventService>();

builder.Services.AddScoped<IRecurrenceService, RecurrenceService>();
builder.Services.AddScoped<EventOccurrenceService>();

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IRemoteEventRepository, EventRepository>();


// Services for Group Management
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();

builder.Services.AddScoped<IGroupEventService, GroupEventService>();

builder.Services.AddScoped<IGroupInvitationService, GroupInvitationService>();
builder.Services.AddScoped<IGroupInvitationRepository, GroupInvitationRepository>();


// Services for User Management
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();



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