namespace SoundFingerprinting.Solr.DAO
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public class HashBinDao : IHashBinDao
    {
        public void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference, IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public IList<HashedFingerprint> ReadHashedFingerprintsByTrackReference(IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] hashBins, int thresholdVotes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] hashBuckets, int thresholdVotes, string trackGroupId)
        {
            throw new System.NotImplementedException();
        }
    }
}
