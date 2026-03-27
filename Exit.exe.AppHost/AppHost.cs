using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Exit_exe_Web>("api")
    .WithEndpoint("https", e => e.Port = 7007)
    .WithEndpoint("http", e => e.Port = 5019);

var frontend = builder.AddViteApp("frontend", "../frontend")
    .WithNpm()
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
