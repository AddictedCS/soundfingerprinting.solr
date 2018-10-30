namespace SoundFingerprinting.Solr
{
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Solr.Infrastructure;

    public class SolrModelService : ModelService
    {
        static SolrModelService()
        {
            var module = new SolrModuleLoader();
            module.LoadAssemblyBindings();
        }

        public SolrModelService() : this(new TrackDao(), new SubFingerprintDao())
        {
        }

        protected SolrModelService(ITrackDao trackDao, ISubFingerprintDao subFingerprintDao) : base(trackDao, subFingerprintDao)
        {
        }
    }
}
