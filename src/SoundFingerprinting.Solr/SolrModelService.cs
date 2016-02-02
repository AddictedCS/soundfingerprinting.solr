namespace SoundFingerprinting.Solr
{
    using SoundFingerprinting.DAO;

    public class SolrModelService : ModelService
    {
        public SolrModelService(ITrackDao trackDao, IHashBinDao hashBinDao, ISubFingerprintDao subFingerprintDao, IFingerprintDao fingerprintDao, ISpectralImageDao spectralImageDao)
            : base(trackDao, hashBinDao, subFingerprintDao, fingerprintDao, spectralImageDao)
        {
        }
    }
}
