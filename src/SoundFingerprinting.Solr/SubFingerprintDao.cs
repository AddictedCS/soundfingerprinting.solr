namespace SoundFingerprinting.Solr
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;

    using CommonServiceLocator;

    using SolrNet;
    using SolrNet.Commands.Parameters;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Solr.Config;
    using SoundFingerprinting.Solr.Converters;
    using SoundFingerprinting.Solr.DAO;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private readonly ISolrOperations<SubFingerprintDTO> solr;
        private readonly IDictionaryToHashConverter dictionaryToHashConverter;

        private readonly ISolrQueryBuilder solrQueryBuilder;
        private readonly SoundFingerprintingSolrConfiguration solrConfig;

        public SubFingerprintDao()
            : this(
                ServiceLocator.Current.GetInstance<ISolrOperations<SubFingerprintDTO>>(),
                new DictionaryToHashConverter(),
                new SolrQueryBuilder(),
                ServiceLocator.Current.GetInstance<SoundFingerprintingSolrConfiguration>())
        {
        }

        internal SubFingerprintDao(ISolrOperations<SubFingerprintDTO> solr, IDictionaryToHashConverter dictionaryToHashConverter, ISolrQueryBuilder solrQueryBuilder, SoundFingerprintingSolrConfiguration solrConfig)
        {
            this.solr = solr;
            this.dictionaryToHashConverter = dictionaryToHashConverter;
            this.solrQueryBuilder = solrQueryBuilder;
            this.solrConfig = solrConfig;
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
            var query = new SolrQuery($"trackId:{SolrModelReference.GetId(trackReference)}");
            var results = solr.Query(
                query,
                new QueryOptions { ExtraParams = new Dictionary<string, string> { { "wt", "xml" } } });
            return results.Select(GetHashedFingerprint).ToList();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprints(int[] hashBins, int thresholdVotes, IEnumerable<string> clusters)
        {
            string queryString = solrQueryBuilder.BuildReadQueryForHashes(hashBins);
            var results = solr.Query(
                new SolrQuery(queryString),
                new QueryOptions
                    {
                        ExtraParams = GetThresholdVotes(thresholdVotes),
                        FilterQueries = GetFilterQueries(clusters)
                    });

            return ConvertResults(results);
        }
        
        public ISet<SubFingerprintData> ReadSubFingerprints(IEnumerable<int[]> hashes, int threshold, IEnumerable<string> clusters)
        {
            var enumerable = hashes as List<int[]> ?? hashes.ToList();
            int total = enumerable.Count;
            var result = new HashSet<SubFingerprintData>();
            var filterQuery = GetFilterQueries(clusters);
            int batchSize = solrConfig.QueryBatchSize;
            bool preferLocalShards = solrConfig.PreferLocalShards;
            for (int i = 0; i < total; i += batchSize)
            {
                var batch = enumerable.Skip(i).Take(batchSize);
                string queryString = solrQueryBuilder.BuildReadQueryForHashesAndThreshold(batch, threshold);
                var results = solr.Query(
                    new SolrQuery(queryString),
                    new QueryOptions
                    {
                        FilterQueries = filterQuery,
                        ExtraParams =
                            new Dictionary<string, string>
                            {
                                {
                                    "preferLocalShards", preferLocalShards.ToString().ToLower()
                                },
                                { "wt", "xml" }
                            }
                    });
                result.UnionWith(ConvertResults(results));
            }

            return result;
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
                       { "mm", thresholdVotes.ToString(CultureInfo.InvariantCulture) },
                       { "wt", "xml" }
                   };
        }

        private SubFingerprintDTO GetSubFingerprintDto(IModelReference trackReference, HashedFingerprint hash)
        {
            return new SubFingerprintDTO
            {
                SubFingerprintId = Guid.NewGuid().ToString(),
                Hashes = dictionaryToHashConverter.FromHashesToSolrDictionary(hash.HashBins),
                SequenceAt = hash.StartsAt,
                SequenceNumber = (int)hash.SequenceNumber,
                TrackId = SolrModelReference.GetId(trackReference),
                Clusters = hash.Clusters
            };
        }

        private HashedFingerprint GetHashedFingerprint(SubFingerprintDTO subFingerprintDto)
        {
            int[] hashBins = dictionaryToHashConverter.FromSolrDictionaryToHashes(subFingerprintDto.Hashes);
            return new HashedFingerprint(hashBins, (uint)subFingerprintDto.SequenceNumber, (float)subFingerprintDto.SequenceAt, subFingerprintDto.Clusters);
        }

        private SubFingerprintData GetSubFingerprintData(SubFingerprintDTO dto)
        {
            int[] resultHashBins = dictionaryToHashConverter.FromSolrDictionaryToHashes(dto.Hashes);
            return new SubFingerprintData(
                resultHashBins,
                (uint)dto.SequenceNumber,
                (float)dto.SequenceAt,
                new SolrModelReference(dto.SubFingerprintId),
                new SolrModelReference(dto.TrackId));
        }

        private ICollection<ISolrQuery> GetFilterQueries(IEnumerable<string> clusters)
        {
            var values = clusters as List<string> ?? clusters.ToList();
            if (!values.Any())
            {
                return new Collection<ISolrQuery>();
            }

            return new List<ISolrQuery> 
                {
                    new SolrQuery(solrQueryBuilder.BuildQueryForClusters(values))
                };
        }
    }
}
