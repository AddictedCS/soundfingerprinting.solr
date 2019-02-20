namespace SoundFingerprinting.Solr.Infrastructure
{
    using CommonServiceLocator;
    using Ninject;
    using Ninject.Integration.SolrNet;

    using SolrNet;
    using SolrNet.Impl;

    using SoundFingerprinting.Solr.Config;

    internal class SolrModuleLoader
    {
        public void LoadAssemblyBindings()
        {
            var kernel = new StandardKernel();


            var solrServers = SolrConfigReader.GetServersFromLocalConfig();
            
            kernel.Load(new SolrNetModule(solrServers));

            var solrConfig = SolrConfigReader.GetSolrConfig();
            
            kernel.Bind<ISoundFingerprintingSolrConfig>().ToConstant(solrConfig);

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
