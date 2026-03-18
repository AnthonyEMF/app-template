var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL
var postgres = builder.AddPostgres("postgres")
    .WithImage("postgres", "17-alpine")
    .WithDataVolume("apptemplate-postgres-data");

// pgAdmin 
postgres.WithPgAdmin(pgAdmin => pgAdmin
    .WithHostPort(5050)
    .WithImage("dpage/pgadmin4", "latest")
    .WithParentRelationship(postgres));

var appDb = postgres.AddDatabase("appdb");

// MongoDB + MongoExpress
var mongo = builder.AddMongoDB("mongo")
    .WithImage("mongo", "8.2")
    .WithDataVolume("apptemplate-mongo-data")
    .WithMongoExpress(express => { express.WithHostPort(8081); }, "mongo-express");

var auditDb = mongo.AddDatabase("auditdb");

// Backend (API Rest)
var api = builder.AddProject<Projects.API>("api")
    .WithReference(appDb)
    .WithReference(auditDb)
    .WaitFor(appDb)
    .WaitFor(auditDb);

// Frontend (Node.js)
builder.AddNpmApp("web", "../Web", "dev")
    .WithReference(api)
    .WithEnvironment("VITE_API_URL", api.GetEndpoint("https"))
    .WithHttpEndpoint(env: "PORT", port: 5173)
    .PublishAsDockerFile();

builder.Build().Run();
