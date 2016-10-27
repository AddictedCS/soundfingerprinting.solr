namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;

    using SolrNet;
    using SolrNet.Commands.Parameters;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Solr.DAO;

    internal class HashBinDao : IHashBinDao
    {
        private readonly ISolrOperations<SubFingerprintDTO> solr;
        private readonly SolrQueryBuilder solrQueryBuilder = new SolrQueryBuilder();
        private readonly DictionaryToHashConverter dictionaryToHashConverter = new DictionaryToHashConverter();
        private readonly HashConverter hashConverter = new HashConverter();

        public HashBinDao() : this(DependencyResolver.Current.Get<ISolrOperations<SubFingerprintDTO>>())
        {
        }

        protected HashBinDao(ISolrOperations<SubFingerprintDTO> solr)
        {
            this.solr = solr;
        }

        public void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference, IModelReference trackReference)
        {
            var hashTables = dictionaryToHashConverter.FromHashes(hashBins);
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
            string queryString = solrQueryBuilder.BuildReadQueryFor(hashBins, thresholdVotes);
            var results = solr.Query(new SolrQuery(queryString));
            return ConvertResults(results);
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] hashBins, int thresholdVotes, string trackGroupId)
        {
            string queryString = solrQueryBuilder.BuildReadQueryFor(hashBins, thresholdVotes);
            var results = solr.Query(
                new SolrQuery(queryString),
                new QueryOptions 
                {
                        FilterQueries = new ISolrQuery[]
                        {
                            new SolrQueryByField("groupId", trackGroupId)
                        }
                });

            return ConvertResults(results);
        }

        private IEnumerable<SubFingerprintData> ConvertResults(IEnumerable<SubFingerprintDTO> results)
        {
            var all = new List<SubFingerprintData>();
            foreach (var dto in results)
            {
                long[] resultHashBins = this.dictionaryToHashConverter.FromSolrDictionary(dto.Hashes);
                byte[] signature = this.hashConverter.ToBytes(resultHashBins, 100); // TODO refactor, extracting this constant
                var sub = new SubFingerprintData(
                    signature,
                    dto.SequenceNumber,
                    dto.SequenceAt,
                    new SolrModelReference(dto.SubFingerprintId),
                    new SolrModelReference(dto.TrackId));
                all.Add(sub);
            }

            return all;
        }
    }
}
