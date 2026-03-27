using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Exit_exe_Web>("api")
    .WithEndpoint("https", e => { e.Port = 7007; e.IsProxied = false; })
    .WithEndpoint("http", e => { e.Port = 5019; e.IsProxied = false; });

var frontend = builder.AddViteApp("frontend", "../frontend")
    .WithNpm()
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
