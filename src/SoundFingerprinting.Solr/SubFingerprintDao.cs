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
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Solr.Converters;
    using SoundFingerprinting.Solr.DAO;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private readonly ISolrOperations<SubFingerprintDTO> solr;
        private readonly IDictionaryToHashConverter dictionaryToHashConverter;
        private readonly IHashConverter hashConverter;

        private readonly ISolrQueryBuilder solrQueryBuilder;

        public SubFingerprintDao()
            : this(
                DependencyResolver.Current.Get<ISolrOperations<SubFingerprintDTO>>(),
                DependencyResolver.Current.Get<IDictionaryToHashConverter>(),
                DependencyResolver.Current.Get<IHashConverter>(),
                DependencyResolver.Current.Get<ISolrQueryBuilder>())
        {
        }

        protected SubFingerprintDao(ISolrOperations<SubFingerprintDTO> solr, IDictionaryToHashConverter dictionaryToHashConverter, IHashConverter hashConverter, ISolrQueryBuilder solrQueryBuilder)
        {
            this.solr = solr;
            this.dictionaryToHashConverter = dictionaryToHashConverter;
            this.hashConverter = hashConverter;
            this.solrQueryBuilder = solrQueryBuilder;
        }

        public void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference)
        {
            var dtos =
                hashes.Select(
                    hash =>
                    new SubFingerprintDTO
                        {
                            SubFingerprintId = Guid.NewGuid().ToString(),
                            Hashes = dictionaryToHashConverter.FromHashesToSolrDictionary(hash.HashBins),
                            SequenceAt = hash.StartsAt,
                            SequenceNumber = hash.SequenceNumber,
                            TrackId = SolrModelReference.GetId(trackReference)
                        }).ToList();
            solr.AddRange(dtos);
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

        public IEnumerable<SubFingerprintData> ReadSubFingerprints(long[] hashBins, int thresholdVotes)
        {
            string queryString = solrQueryBuilder.BuildReadQueryForHashesAndThreshold(hashBins, thresholdVotes);
            var results = solr.Query(
                new SolrQuery(queryString),
                new QueryOptions
                    {
                        ExtraParams =
                            new Dictionary<string, string>
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
        public IEnumerable<SubFingerprintData> ReadSubFingerprints(long[] hashBins, int thresholdVotes, string trackGroupId)
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

        public ISet<SubFingerprintData> ReadSubFingerprints(IEnumerable<long[]> hashes, int threshold)
        {
            string queryString = solrQueryBuilder.BuildReadQueryForHashesAndThreshold(hashes, threshold);
            var results = solr.Query(queryString);
            return new HashSet<SubFingerprintData>(ConvertResults(results));
        }

        private IEnumerable<SubFingerprintData> ConvertResults(IEnumerable<SubFingerprintDTO> results)
        {
            var all = new List<SubFingerprintData>();
            foreach (var dto in results)
            {
                long[] resultHashBins = dictionaryToHashConverter.FromSolrDictionaryToHashes(dto.Hashes);
                var sub = new SubFingerprintData(
                    resultHashBins,
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
