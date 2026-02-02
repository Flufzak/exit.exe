using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var web = builder.AddProject<Exit_exe_Web>("web");

await builder.Build().RunAsync();
