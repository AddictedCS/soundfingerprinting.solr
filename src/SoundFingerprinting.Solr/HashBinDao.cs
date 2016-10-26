namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;
    using System.Linq;

    using SolrNet;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Solr.DAO;

    internal class HashBinDao : IHashBinDao
    {
        private readonly ISolrOperations<SubFingerprintDTO> solr;

        public HashBinDao() : this(DependencyResolver.Current.Get<ISolrOperations<SubFingerprintDTO>>())
        {
        }

        protected HashBinDao(ISolrOperations<SubFingerprintDTO> solr)
        {
            this.solr = solr;
        }

        public void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference, IModelReference trackReference)
        {
            var hashTables = hashBins.Select((hash, index) => new { index, hash }).ToDictionary(
                x => x.index, x => x.hash);
            solr.Add(
                new SubFingerprintDTO
                    {
                        SubFingerprintId = SolrModelReference.GetId(subFingerprintReference),
                        Hashes = hashTables,
                        TrackId = SolrModelReference.GetId(trackReference)
                    });
            solr.Commit();
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
