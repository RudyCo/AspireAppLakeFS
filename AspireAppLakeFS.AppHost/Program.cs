var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("lakefs-postgres")
    .WithPgWeb();

var postgresdb = postgres.AddDatabase("lakefs-postgredb", "postgres");

var lakefs = builder.AddLakeFS("lakefs", database: postgresdb.Resource)
    .WithImageTag("1.39.2")
    .WaitFor(postgresdb);

var apiService = builder.AddProject<Projects.AspireAppLakeFS_ApiService>("apiservice")
     .WithReference(lakefs).WaitFor(lakefs);

builder.AddProject<Projects.AspireAppLakeFS_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService).WaitFor(apiService);

builder.Build().Run();