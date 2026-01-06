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