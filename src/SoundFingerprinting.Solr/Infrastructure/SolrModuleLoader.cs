namespace SoundFingerprinting.Solr.Infrastructure
{
    using CommonServiceLocator;
    using Microsoft.Extensions.Configuration;

    using Ninject;

    using SolrNet;
    using SolrNet.Impl;

    using SoundFingerprinting.Solr.Config;

    using System.IO;
    using System.Reflection;

    internal class SolrModuleLoader
    {
        public void LoadAssemblyBindings()
        {
            var kernel = new StandardKernel();

            var config = new ConfigurationBuilder()
               .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
               .AddJsonFile("appsettings.json", false, true)
               .Build();

            var solrConfig = new SoundFingerprintingSolrConfiguration();
            config.GetSection("solr").Bind(solrConfig);

            kernel.Load(new SolrNetModule(solrConfig.Servers));

            kernel.Bind<SoundFingerprintingSolrConfiguration>().ToConstant(solrConfig);

            var tracksConnection = GetTracksConnection(kernel);
            var fingerprintsConnection = GetFingerprintsConnection(kernel);

            kernel.Unbind<ISolrConnection>();

             kernel.Bind<ISolrConnection>()
                  .ToConstant(new PostSolrConnection(tracksConnection, tracksConnection.ServerURL))
                  .WithMetadata("CoreId", "tracks");

            kernel.Bind<ISolrConnection>()
                  .ToConstant(new PostSolrConnection(fingerprintsConnection, fingerprintsConnection.ServerURL))
                  .WithMetadata("CoreId", "fingerprints");

            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(kernel));
        }

        private static SolrConnection GetFingerprintsConnection(IKernel kernel)
        {
            var fingerprintsConnection = (SolrConnection)kernel.Get<ISolrConnection>(
                metadata =>
                    {
                        object result = metadata.Get<object>("CoreId");
                        return "fingerprints".Equals(result);
                    });
            return fingerprintsConnection;
        }

        private static SolrConnection GetTracksConnection(IKernel kernel)
        {
            var tracksConnection = (SolrConnection)kernel.Get<ISolrConnection>(
                metadata =>
                    {
                        object result = metadata.Get<object>("CoreId");
                        return "tracks".Equals(result);
                    });
            return tracksConnection;
        }
    }
}
