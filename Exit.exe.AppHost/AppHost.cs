using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Exit_exe_Web>("api");

builder.AddNpmApp("frontend", "../frontend")
    .WithReference(api)
    .WaitFor(api);

await builder.Build().RunAsync();
