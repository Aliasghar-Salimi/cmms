var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

.WithOpenApi();

app.MapGet("/", () => Results.Ok(new { 
    Message = "CMMS Audit Log Service is running",
    Endpoints = new {
        Health = "/health",
        Version = "/api/v1/version",
        Swagger = "/swagger",
        Api = "/api/v1"
    },
    Timestamp = DateTime.UtcNow,
    Version = VersionInfo.GetVersionInfo()
}));

app.MapGet("/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Service = "CMMS Audit Log Service",
    Version = VersionInfo.InformationalVersion,
    BuildVersion = VersionInfo.Version
}));

app.Run();

