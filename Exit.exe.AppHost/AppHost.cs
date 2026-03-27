using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Exit_exe_Web>("api")
    .WithHttpsEndpoint(port: 7007)
    .WithHttpEndpoint(port: 5019);

var frontend = builder.AddViteApp("frontend", "../frontend")
    .WithNpm()
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
