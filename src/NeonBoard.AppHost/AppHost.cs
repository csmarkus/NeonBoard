var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var neonboardDb = postgres.AddDatabase("neonboarddb");

var api = builder.AddProject<Projects.NeonBoard_Api>("neonboard-api")
    .WithReference(neonboardDb)
    .WaitFor(postgres);

builder.AddNpmApp("neonboard-ui", "../NeonBoard.UI")
    .WithReference(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
