using System;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using NUnit.Framework;

namespace CricketClub.Tests
{
    public abstract class IntegrationTestSupport
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(IntegrationTestSupport));

        
        [OneTimeSetUp]
        public void Init()
        {
            // For .NET 8: Set connection string via environment variable if App.config isn't loaded
            // This is a workaround for .NET 8 where ConfigurationManager may not load App.config in test scenarios
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ConnectionStrings__TestDB")))
            {
                // For Azure SQL with firewall restrictions, use proxy mode (no explicit port)
                var testConnectionString = "Server=thevillagecc.database.windows.net;Database=thevilla_scorebook;User Id=thevillagecc_admin;Password=JVbB7ujECUxS2tm;Connect Timeout=120;Max Pool Size=50;Encrypt=True;TrustServerCertificate=True;";
                Environment.SetEnvironmentVariable("ConnectionStrings__TestDB", testConnectionString);
                Log.Info("Set TestDB connection string via environment variable for .NET 8 compatibility");
            }
            
            var layout = new PatternLayout("%date %-5level %logger - %message%newline");
            layout.ActivateOptions();

            var console = new ConsoleAppender { Layout = layout };
            console.ActivateOptions();

            BasicConfigurator.Configure(console);
            // optional: set root level
            var repo = LogManager.GetRepository();
            ((log4net.Repository.Hierarchy.Hierarchy)repo).Root.Level = log4net.Core.Level.Debug;
        }
    }
}