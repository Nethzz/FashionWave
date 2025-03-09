using Microsoft.Extensions.DependencyInjection;
using RegClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure session settings to keep it for a long time (e.g., 24 hours)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(32); // Extend session timeout to 24 hours
    options.Cookie.HttpOnly = true; // Prevents JavaScript from accessing the cookie
    options.Cookie.IsEssential = true; // Mark the session cookie as essential
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Send cookie over HTTPS only
});

// Add HttpClient for the API gateway with Base URL configuration
builder.Services.AddHttpClient<APIGateway>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7165"); // Set your API base URL
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromSeconds(30); // Set timeout for API calls
});

// Register APIGateway as a scoped service
builder.Services.AddScoped<APIGateway>();

var app = builder.Build();

// Configure middleware based on environment
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Show detailed errors in development
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Redirect to error page in production
    app.UseHsts(); // Add HSTS headers for security
}

app.UseHttpsRedirection(); // Enforce HTTPS
app.UseStaticFiles(); // Enable serving of static files (CSS, JS, images)

app.UseRouting(); // Enable routing

app.UseSession(); // Enable session support
app.UseAuthorization(); // Enable authorization

// Configure routing and default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");

// Run the application
app.Run();
