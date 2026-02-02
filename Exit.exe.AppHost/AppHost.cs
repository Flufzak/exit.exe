using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Exit_exe_Web>("api");

var frontend = builder.AddNpmApp("frontend", "../frontend", "dev")
    .WithReference(api)
    .WaitFor(api);

await builder.Build().RunAsync();
