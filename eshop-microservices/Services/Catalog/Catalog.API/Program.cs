using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
builder.AddNpgsqlDataSource("CatalogDb");

builder.Services.AddMarten(sp =>
{
    var opts = new StoreOptions();
    var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
    opts.Connection(dataSource);
    return opts;
}).UseLightweightSessions();
var app = builder.Build();

app.MapDefaultEndpoints();

app.MapCarter();
app.Run();
