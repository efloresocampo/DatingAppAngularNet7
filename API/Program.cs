using API.Data;
using API.Extensions;
using API.Middleware;
using Microsoft.EntityFrameworkCore;

string specificAllowedOrigins = "_specificAllowedOrigins";
var builder = WebApplication.CreateBuilder(args);

// CORS policy.
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: specificAllowedOrigins,
        policy =>
        {
            policy
            .WithOrigins("https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(specificAllowedOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Give access to all of the services set in this program class
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    // Reseting the database and re seeding with seed data.
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedUsers(context);
}
catch (System.Exception ex)
{
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error ocurred during migration");
}
app.Run();
