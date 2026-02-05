var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var neonboardDb = postgres.AddDatabase("neonboarddb");

builder.AddProject<Projects.NeonBoard_Api>("neonboard-api")
    .WithReference(neonboardDb)
    .WaitFor(postgres);

builder.Build().Run();
