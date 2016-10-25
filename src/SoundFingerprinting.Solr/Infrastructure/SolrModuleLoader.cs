namespace SoundFingerprinting.Solr.Infrastructure
{
    using Ninject;
    using Ninject.Integration.SolrNet;

    using SolrNet;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Solr.DAO;

    public class SolrModuleLoader : IModuleLoader
    {
        public void LoadAssemblyBindings(IKernel kernel)
        {
            // TODO Refactor, extract the URLs in App.config file
            // E.g. https://github.com/mausch/SolrNet/blob/master/StructureMap.SolrNetIntegration.Tests/StructureMapFixture.cs
            Startup.Init<SubFingerprintDTO>("http://localhost:8983/solr/sound_fingerprinting");
        }
    }
}
