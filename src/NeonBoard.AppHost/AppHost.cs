var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.NeonBoard_Api>("neonboard-api");

builder.Build().Run();
