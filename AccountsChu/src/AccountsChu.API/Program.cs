using AccountsChu.API.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.AddAuthentication();
builder.AddDocumentation();
builder.AddDataContexts();
builder.AddCrossOrigin();
builder.AddServices();
builder.AddRefit();

builder.Services.AddMediatRConfiguration();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.ApplyMigrations();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
    app.ConfigureDevEnvironment();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();