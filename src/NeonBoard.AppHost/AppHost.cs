var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var neonboardDb = postgres.AddDatabase("neonboarddb");

var api = builder.AddProject<Projects.NeonBoard_Api>("neonboard-api")
    .WithReference(neonboardDb)
    .WaitFor(postgres);

builder.AddNpmApp("neonboard-ui", "../NeonBoard.UI", "start")
    .WithHttpEndpoint(port: 4200, isProxied: false)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
