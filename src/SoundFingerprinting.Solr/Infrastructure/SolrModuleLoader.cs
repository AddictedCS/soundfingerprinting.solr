namespace SoundFingerprinting.Solr.Infrastructure
{
    using Microsoft.Practices.ServiceLocation;

    using Ninject;

    using SolrNet;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Solr.DAO;

    public class SolrModuleLoader : IModuleLoader
    {
        public void LoadAssemblyBindings(IKernel kernel)
        {
            // TODO Refactor, extract the URLs in App.config file
            // E.g. https://github.com/mausch/SolrNet/blob/master/StructureMap.SolrNetIntegration.Tests/StructureMapFixture.cs
            Startup.Init<SubFingerprintDTO>("http://localhost:8983/solr/sf_fingerprints");
            var solrForSubDao = ServiceLocator.Current.GetInstance<ISolrOperations<SubFingerprintDTO>>();
            kernel.Bind<ISolrOperations<SubFingerprintDTO>>().ToConstant(solrForSubDao);

            Startup.Init<TrackDTO>("http://localhost:8983/solr/sf_tracks");
            var solrForTrackDao = ServiceLocator.Current.GetInstance<ISolrOperations<TrackDTO>>();
            kernel.Bind<ISolrOperations<TrackDTO>>().ToConstant(solrForTrackDao);
        }
    }
}
