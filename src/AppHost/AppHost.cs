var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL + pgAdmin integrado
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("postgres-data")
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));

var database = postgres.AddDatabase("appdb");

// Backend (API Rest)
var api = builder.AddProject<Projects.API>("api")
    .WithReference(database)
    .WaitFor(database);

// Frontend (Node.js)
builder.AddNpmApp("web", "../Web", "dev")
    .WithReference(api)
    .WithEnvironment("VITE_API_URL", api.GetEndpoint("https"))
    .WithHttpEndpoint(env: "PORT", port: 5173)
    .PublishAsDockerFile();

builder.Build().Run();
