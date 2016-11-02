namespace SoundFingerprinting.Solr
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using SolrNet;
    using SolrNet.Commands.Parameters;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Solr.Converters;
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

        // TODO Remove in v3.x
        // This method is an aggregation of 2 methods from SubFingerprintDao and HashBinDao
        // It is not present in the ModelService interface (as per v2.3x), but will be once the interfaces
        // SQL and SOLR will be adapted to the same high throughput interface
        public void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference)
        {
            var dtos = hashes.Select(hash => new SubFingerprintDTO { SubFingerprintId = Guid.NewGuid().ToString(), Hashes = this.dictionaryToHashConverter.FromHashesToSolrDictionary(hash.HashBins), SequenceAt = hash.Timestamp, SequenceNumber = hash.SequenceNumber, TrackId = SolrModelReference.GetId(trackReference) })
                             .ToList();
            solr.AddRange(dtos);
            solr.Commit();
        }

        public void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference, IModelReference trackReference)
        {
            var hashTables = dictionaryToHashConverter.FromHashesToSolrDictionary(hashBins);
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
            var query = new SolrQuery(string.Format("trackId:{0}", SolrModelReference.GetId(trackReference)));
            var results = solr.Query(query);
            var fingerprints = new List<HashedFingerprint>();
            foreach (var subFingerprintDto in results)
            {
                long[] resultHashBins = dictionaryToHashConverter.FromSolrDictionaryToHashes(subFingerprintDto.Hashes);
                byte[] signature = hashConverter.ToBytes(resultHashBins, 100); // TODO refactor, extracting this constant 
                var fingerprint = new HashedFingerprint(signature, resultHashBins, subFingerprintDto.SequenceNumber, subFingerprintDto.SequenceAt);
                fingerprints.Add(fingerprint);
            }

            return fingerprints;
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] hashBins, int thresholdVotes)
        {
            string queryString = solrQueryBuilder.BuildReadQueryForHashesAndThresholdEDismax(hashBins, thresholdVotes);
            var results = solr.Query(
                new SolrQuery(queryString),
                new QueryOptions
                    {
                        ExtraParams =
                            new Dictionary<string, string>()
                                {
                                    { "defType", "edismax" },
                                    { "mm", thresholdVotes.ToString(CultureInfo.InvariantCulture) }
                                }
                    });

            return ConvertResults(results);
        }

        // TODO
        // This method is not working in v2.x due to schema limitations
        // Make it functional in 3.x
        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] hashBins, int thresholdVotes, string trackGroupId)
        {
            string queryString = solrQueryBuilder.BuildReadQueryForHashesAndThreshold(hashBins, thresholdVotes);
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
                long[] resultHashBins = dictionaryToHashConverter.FromSolrDictionaryToHashes(dto.Hashes);
                byte[] signature = hashConverter.ToBytes(resultHashBins, 100); // TODO refactor, extracting this constant
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
