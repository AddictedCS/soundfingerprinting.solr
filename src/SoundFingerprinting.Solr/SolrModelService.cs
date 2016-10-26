namespace SoundFingerprinting.Solr
{
    public class SolrModelService : ModelService
    {
        public SolrModelService()
            : base(new TrackDao(), new HashBinDao(), new SubFingerprintDao(), new FingerprintDao(), new SpectralImageDao())
        {
        }
    }
}
