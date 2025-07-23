var builder = WebApplication.CreateBuilder(args);

// ✅ Configure Kestrel to use PORT from Render
builder.WebHost.ConfigureKestrel(options =>
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000"; // Default 5000 if PORT not set
    options.ListenAnyIP(Convert.ToInt32(port));
});

// ✅ Add services
builder.Services.AddSession();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ✅ Enable Session Middleware
app.UseSession();

// ✅ Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// ✅ Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
