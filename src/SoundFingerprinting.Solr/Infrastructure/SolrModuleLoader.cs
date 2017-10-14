namespace SoundFingerprinting.Solr.Infrastructure
{
    using System.Configuration;

    using Ninject;
    using Ninject.Integration.SolrNet;

    using SolrNet;
    using SolrNet.Impl;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Solr.Config;
    using SoundFingerprinting.Solr.Converters;

    public class SolrModuleLoader : IModuleLoader
    {
        public void LoadAssemblyBindings(IKernel kernel)
        {
            var solrConfig = (SoundFingerprintingSolrConfigurationSection)ConfigurationManager.GetSection("solr");
            
            kernel.Load(new SolrNetModule(solrConfig.SolrServers));
            kernel.Bind<IDictionaryToHashConverter>().To<DictionaryToHashConverter>().InSingletonScope();
            kernel.Bind<ISolrQueryBuilder>().To<SolrQueryBuilder>().InSingletonScope();
            kernel.Bind<ISoundFingerprintingSolrConfig>().ToConstant(
                new SoundFingerprintingSolrConfig(solrConfig.QueryBatchSize, solrConfig.PreferLocalShards));

            var tracksConnection = GetTracksConnection(kernel);
            var fingerprintsConnection = GetFingerprintsConnection(kernel);

            kernel.Unbind<ISolrConnection>();

            kernel.Bind<ISolrConnection>()
                  .ToConstant<PostSolrConnection>(new PostSolrConnection(tracksConnection, tracksConnection.ServerURL))
                  .WithMetadata("CoreId", (object)"tracks");

            kernel.Bind<ISolrConnection>()
                  .ToConstant<PostSolrConnection>(new PostSolrConnection(fingerprintsConnection, fingerprintsConnection.ServerURL))
                  .WithMetadata("CoreId", (object)"fingerprints");
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
