using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

var builder = DistributedApplication.CreateBuilder(args);

//var usernameDefault = "postgres";
//var passwordDefault = "***lakefs_pwd_2000***";

//var usernameParam = builder.AddParameter("username", usernameDefault, true);
//var passwordParam = builder.AddParameter("password", passwordDefault, true);

//var postgres = builder.AddPostgres("postgres", usernameParam, passwordParam, 54332)
//    .WithPgAdmin();

//builder.AddContainer("lakefs", "treeverse/lakefs")
//    .WithHttpEndpoint(8000, 8000)
//    .WithEnvironment("LAKEFS_DATABASE_TYPE", "postgres")
//    .WithEnvironment("LAKEFS_DATABASE_POSTGRES_CONNECTION_STRING", $"postgres://{usernameDefault}:{passwordDefault}@postgres:5432/{usernameDefault}?sslmode=disable")
//    .WithEnvironment("LAKEFS_AUTH_ENCRYPT_SECRET_KEY", "10a718b3f285d89c36e9864494cdd1507f3bc85b342df24736ea81f9a1134bcc")
//    .WithEnvironment("LAKEFS_BLOCKSTORE_TYPE", "local")
//    .WithEnvironment("LAKEFS_BLOCKSTORE_TYPE_LOCAL_PATH", "~/lakefs/dev/data")
//    .WithArgs(["run"])
//    .WithReference(postgres).WaitFor(postgres);

var lakefs = builder.AddLakeFS("lakefs");

var apiService = builder.AddProject<Projects.AspireAppLakeFS_ApiService>("apiservice")
     .WithReference(lakefs).WaitFor(lakefs);

builder.AddProject<Projects.AspireAppLakeFS_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService).WaitFor(apiService);

builder.Build().Run();
