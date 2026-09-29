using Booker.Api.Data;
using Booker.Api.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddCors(options =>
{
    options.AddPolicy("BookerApp", policy =>
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseInMemoryDatabase("Booker"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("BookerApp");
app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    database.Database.EnsureCreated();
}

app.MapBookEndpoints();

app.Run();
