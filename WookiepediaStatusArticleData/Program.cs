using WookiepediaStatusArticleData;

try
{
    using var host = CreateHostBuilder(args).Build();
    await host.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    return 1;
}

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>()
                .ConfigureAppConfiguration(config => { config.AddEnvironmentVariables(); });
        });
}