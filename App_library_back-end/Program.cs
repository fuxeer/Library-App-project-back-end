using App_library_back_end.Data;
using App_library_back_end.Services;

var builder = WebApplication.CreateBuilder(args);

// Register ReservationService for controllers
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    string conn = config.GetConnectionString("DefaultConnection");
    return new ReservationService(conn);
});

// Register the background worker
builder.Services.AddHostedService<RentalStatusService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    string conn = config.GetConnectionString("DefaultConnection");
    return new RentalStatusService(conn);
});

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
