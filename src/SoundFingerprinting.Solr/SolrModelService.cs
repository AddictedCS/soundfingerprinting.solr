namespace SoundFingerprinting.Solr
{
    using SoundFingerprinting.DAO;

    public class SolrModelService : ModelService
    {
        public SolrModelService() : this(new TrackDao(), new SubFingerprintDao())
        {
        }

        protected SolrModelService(ITrackDao trackDao, ISubFingerprintDao subFingerprintDao) : base(trackDao, subFingerprintDao)
        {
        }
    }
}
