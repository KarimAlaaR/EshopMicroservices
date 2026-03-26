using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Catalog.API.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;
builder.AddServiceDefaults();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddCarter();

builder.AddNpgsqlDataSource("CatalogDb");

builder.Services.AddMarten(sp =>
{
    var opts = new StoreOptions();
    var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
    opts.Connection(dataSource);
    return opts;
}).UseLightweightSessions();

if (builder.Environment.IsDevelopment())
    builder.Services.InitializeMartenWith<CatalogInitialData>();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
var app = builder.Build();

app.MapCarter();
app.UseExceptionHandler(options => { });
app.MapDefaultEndpoints();
app.Run();
