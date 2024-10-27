var builder = DistributedApplication.CreateBuilder(args);

var userNameParam = builder.AddParameter("username", "postgres", publishValueAsDefault: true);
var passwordParam = builder.AddParameter("password", "LakeFSPassword", publishValueAsDefault: true);

var postgres = builder.AddPostgres("postgres", userName: userNameParam, password: passwordParam, port: 54332)
    .WithPgWeb();

var postgresdb = postgres.AddDatabase("postgredb", "postgres");

var lakefs = builder.AddLakeFS("lakefs", postgres: postgres.Resource)
    .WithImageTag("1.39.2")
    .WaitFor(postgresdb);

var apiService = builder.AddProject<Projects.AspireAppLakeFS_ApiService>("apiservice")
     .WithReference(lakefs).WaitFor(lakefs);

builder.AddProject<Projects.AspireAppLakeFS_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService).WaitFor(apiService);

builder.Build().Run();