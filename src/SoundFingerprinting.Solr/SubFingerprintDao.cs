namespace SoundFingerprinting.Solr
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using SolrNet;
    using SolrNet.Commands.Parameters;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Solr.Converters;
    using SoundFingerprinting.Solr.DAO;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private readonly int fingerprintLength;

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
            var hashinConfig = new DefaultHashingConfig();
            fingerprintLength = hashinConfig.NumberOfLSHTables * hashinConfig.NumberOfMinHashesPerTable;
        }

        public void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference)
        {
            var dtos = hashes.Select(hash => GetSubFingerprintDto(trackReference, hash))
                             .ToList();
            solr.AddRange(dtos);
            solr.Commit();
        }

        public IList<HashedFingerprint> ReadHashedFingerprintsByTrackReference(IModelReference trackReference)
        {
            var query = new SolrQuery(string.Format("trackId:{0}", SolrModelReference.GetId(trackReference)));
            var results = solr.Query(query);
            return results.Select(GetHashedFingerprint).ToList();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprints(long[] hashBins, int thresholdVotes)
        {
            string queryString = solrQueryBuilder.BuildReadQueryForHashes(hashBins);
            var results = solr.Query(
                new SolrQuery(queryString),
                new QueryOptions
                    {
                        ExtraParams = GetThresholdVotes(thresholdVotes)
                    });

            return ConvertResults(results);
        }
        
        // TODO
        // This method is not working in v2.x due to schema limitations
        // Make it functional in 3.x
        public IEnumerable<SubFingerprintData> ReadSubFingerprints(long[] hashBins, int thresholdVotes, string trackGroupId)
        {
            string queryString = solrQueryBuilder.BuildReadQueryForHashes(hashBins);
            var results = solr.Query(
                new SolrQuery(queryString),
                new QueryOptions
                {
                    ExtraParams = GetThresholdVotes(thresholdVotes),
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
            return results.Select(GetSubFingerprintData).ToList();
        }

        private Dictionary<string, string> GetThresholdVotes(int thresholdVotes)
        {
            return new Dictionary<string, string>
                {
                    { "defType", "edismax" },
                    { "mm", thresholdVotes.ToString(CultureInfo.InvariantCulture) }
                };
        }

        private SubFingerprintDTO GetSubFingerprintDto(IModelReference trackReference, HashedFingerprint hash)
        {
            return new SubFingerprintDTO
            {
                SubFingerprintId = Guid.NewGuid().ToString(),
                Hashes = dictionaryToHashConverter.FromHashesToSolrDictionary(hash.HashBins),
                SequenceAt = hash.StartsAt,
                SequenceNumber = hash.SequenceNumber,
                TrackId = SolrModelReference.GetId(trackReference)
            };
        }

        private HashedFingerprint GetHashedFingerprint(SubFingerprintDTO subFingerprintDto)
        {
            long[] hashBins = dictionaryToHashConverter.FromSolrDictionaryToHashes(subFingerprintDto.Hashes);
            byte[] signature = hashConverter.ToBytes(hashBins, fingerprintLength);
            return new HashedFingerprint(signature, hashBins, subFingerprintDto.SequenceNumber, subFingerprintDto.SequenceAt);
        }

        private SubFingerprintData GetSubFingerprintData(SubFingerprintDTO dto)
        {
            long[] resultHashBins = this.dictionaryToHashConverter.FromSolrDictionaryToHashes(dto.Hashes);
            var sub = new SubFingerprintData(
                resultHashBins,
                dto.SequenceNumber,
                dto.SequenceAt,
                new SolrModelReference(dto.SubFingerprintId),
                new SolrModelReference(dto.TrackId));
            return sub;
        }
    }
}
