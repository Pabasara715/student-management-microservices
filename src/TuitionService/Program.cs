IHost host = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.UseRabbitMQMessageHandler(hostContext.Configuration);

        services.AddTransient<ITuitionBillRepository>((svc) =>
        {
            var sqlConnectionString = hostContext.Configuration.GetConnectionString("TuitionServiceCN");
            return new SqlServerTuitionBillRepository(sqlConnectionString);
        });

        services.AddTransient<IEmailCommunicator>((svc) =>
        {
            var mailConfigSection = hostContext.Configuration.GetSection("Email");
            string mailHost = mailConfigSection["Host"];
            int mailPort = Convert.ToInt32(mailConfigSection["Port"]);
            string mailUserName = mailConfigSection["User"];
            string mailPassword = mailConfigSection["Pwd"];
            return new SMTPEmailCommunicator(mailHost, mailPort, mailUserName, mailPassword);
        });

        services.AddHostedService<TuitionWorker>();
    })
    .UseSerilog((hostContext, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration);
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();
