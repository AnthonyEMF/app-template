var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL + pgAdmin
var postgres = builder.AddPostgres("postgres")
    .WithImage("postgres", "17-alpine")
    .WithDataVolume("apptemplate-postgres-data")
    .WithPgAdmin(pgAdmin => pgAdmin
    .WithHostPort(5050)
    .WithImage("dpage/pgadmin4", "latest"));

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
