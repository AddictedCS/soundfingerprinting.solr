namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;

    public class SolrModelService : ModelService
    {
        private readonly HashBinDao hashBinDao;

        public SolrModelService() : this(new TrackDao(), new HashBinDao(), new SubFingerprintDao(), new FingerprintDao(), new SpectralImageDao())
        {
        }

        protected SolrModelService(
            ITrackDao trackDao,
            IHashBinDao hashBinDao,
            ISubFingerprintDao subFingerprintDao,
            IFingerprintDao fingerprintDao,
            ISpectralImageDao spectralImageDao)
            : base(trackDao, hashBinDao, subFingerprintDao, fingerprintDao, spectralImageDao)
        {
            this.hashBinDao = (HashBinDao)hashBinDao;
        }

        public override void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference)
        {
            hashBinDao.InsertHashDataForTrack(hashes, trackReference);
        }
    }
}
