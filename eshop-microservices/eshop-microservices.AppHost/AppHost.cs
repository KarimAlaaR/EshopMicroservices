var builder = DistributedApplication.CreateBuilder(args);
// 1. Setup Postgres
// "catalogdb" becomes the resource name.
// We add a data volume so your data persists just like in Docker Compose.
var catalogPostgresServer = builder.AddPostgres("catalogPostgresServer")
                      .WithDataVolume("postgres_catalog"); // Matches your 'volumes' line

var catalogDb = catalogPostgresServer.AddDatabase("CatalogDb");
builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WaitFor(catalogPostgresServer)
    .WithReference(catalogDb);

builder.Build().Run();
