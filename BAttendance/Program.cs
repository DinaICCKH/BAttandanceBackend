using BAttendance.Models;
using BAttendance.Models; // <-- Your actual DbContext namespace
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SessionExpireFilter>();
});

// ✅ Register DbContext with connection string
builder.Services.AddDbContext<_DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Db")));
//builder.Services.AddDbContext<SAPContext>(options =>
//options.UseSqlServer(builder.Configuration.GetConnectionString("SAPConnection")));

// ✅ Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Add HttpContextAccessor (recommended)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Enable Session before Authorization
app.UseSession();

app.UseAuthorization();

// 7. Default route pointing to Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
