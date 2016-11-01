namespace SoundFingerprinting.Solr.Infrastructure
{
    using System.Configuration;

    using Ninject;
    using Ninject.Integration.SolrNet;
    using Ninject.Integration.SolrNet.Config;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Solr.Converters;

    public class SolrModuleLoader : IModuleLoader
    {
        public void LoadAssemblyBindings(IKernel kernel)
        {
            var solrConfig = (SolrConfigurationSection)ConfigurationManager.GetSection("solr");
            kernel.Load(new SolrNetModule(solrConfig.SolrServers));

            kernel.Bind<IHashConverter>().To<HashConverter>();
            kernel.Bind<IDictionaryToHashConverter>().To<DictionaryToHashConverter>();
            kernel.Bind<ISolrQueryBuilder>().To<SolrQueryBuilder>();
        }
    }
}
