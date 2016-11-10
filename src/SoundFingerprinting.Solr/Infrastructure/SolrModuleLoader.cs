namespace SoundFingerprinting.Solr.Infrastructure
{
    using System;
    using System.Configuration;
    using System.Linq;

    using Ninject;
    using Ninject.Integration.SolrNet;
    using Ninject.Integration.SolrNet.Config;
    using Ninject.Parameters;

    using SolrNet;
    using SolrNet.Impl;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Solr.Converters;

    public class SolrModuleLoader : IModuleLoader
    {
        public void LoadAssemblyBindings(IKernel kernel)
        {
            var solrConfig = (SolrConfigurationSection)ConfigurationManager.GetSection("solr");
            kernel.Load(new SolrNetModule(solrConfig.SolrServers));

            kernel.Bind<IDictionaryToHashConverter>().To<DictionaryToHashConverter>();
            kernel.Bind<ISolrQueryBuilder>().To<SolrQueryBuilder>();


            var tracksConnection = (SolrConnection)kernel.Get<ISolrConnection>(metadata =>
                {
                    object result = metadata.Get<object>("CoreId");
                    return "tracks".Equals(result);
                });

            var fingerprintsConnection =
                (SolrConnection)
                kernel.Get<ISolrConnection>(metadata =>
                    {
                        object result = metadata.Get<object>("CoreId");
                        return "fingerprints".Equals(result);
                    });

            kernel.Unbind<ISolrConnection>();

            kernel.Bind<ISolrConnection>().ToConstant<PostSolrConnection>(new PostSolrConnection(tracksConnection, tracksConnection.ServerURL))
                .WithMetadata("CoreId", (object)"tracks");

            kernel.Bind<ISolrConnection>().ToConstant<PostSolrConnection>(new PostSolrConnection(fingerprintsConnection, fingerprintsConnection.ServerURL))
                .WithMetadata("CoreId", (object)"fingerprints");


        }
    }
}
