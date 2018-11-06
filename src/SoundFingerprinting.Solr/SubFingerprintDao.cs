namespace SoundFingerprinting.Solr
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using CommonServiceLocator;

    using SolrNet;
    using SolrNet.Commands.Parameters;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Solr.Config;
    using SoundFingerprinting.Solr.Converters;
    using SoundFingerprinting.Solr.DAO;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private readonly ISolrOperations<SubFingerprintDTO> solr;
        private readonly IDictionaryToHashConverter dictionaryToHashConverter;

        private readonly ISolrQueryBuilder solrQueryBuilder;
        private readonly ISoundFingerprintingSolrConfig solrConfig;

        public SubFingerprintDao() : this(ServiceLocator.Current.GetInstance<ISolrOperations<SubFingerprintDTO>>(), new DictionaryToHashConverter(), new SolrQueryBuilder(),
                ServiceLocator.Current.GetInstance<ISoundFingerprintingSolrConfig>())
        {
        }

        internal SubFingerprintDao(ISolrOperations<SubFingerprintDTO> solr, IDictionaryToHashConverter dictionaryToHashConverter, ISolrQueryBuilder solrQueryBuilder, ISoundFingerprintingSolrConfig solrConfig)
        {
            this.solr = solr;
            this.dictionaryToHashConverter = dictionaryToHashConverter;
            this.solrQueryBuilder = solrQueryBuilder;
            this.solrConfig = solrConfig;
        }

        public int SubFingerprintsCount
        {
            get
            {
                var query = new SolrQuery("*:*");
                return solr.Query(query).NumFound;
            }
        }

        public IEnumerable<int> HashCountsPerTable
        {
            get
            {
                for (int i = 0; i < 25; ++i)
                {
                    var solrQuery = new SolrQuery("hashTable_{index}:*");
                    yield return solr.Query(solrQuery).NumFound;
                }
            }
        }

        public IEnumerable<SubFingerprintData> InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference)
        {
            var subFingerprints = hashes.Select(hash => new SubFingerprintData(
                    hash.HashBins,
                    hash.SequenceNumber,
                    hash.StartsAt,
                    hash.Clusters,
                    new SolrModelReference(Guid.NewGuid().ToString()),
                    trackReference)).ToList();
            InsertSubFingerprints(subFingerprints);
            return subFingerprints;
        }

        public void InsertSubFingerprints(IEnumerable<SubFingerprintData> subFingerprints)
        {
            var dtos = subFingerprints.Select(GetSubFingerprintDto).ToList();
            solr.AddRange(dtos);
            solr.Commit();
        }

        public int DeleteSubFingerprintsByTrackReference(IModelReference trackReference)
        {
            string trackId = SolrModelReference.GetId(trackReference);
            string readAll = $"trackId:{trackId}";
            var results = solr.Query(new SolrQuery(readAll), new QueryOptions { ExtraParams = new Dictionary<string, string> { { "wt", "xml" } } });
            var ids = results.Select(dto => dto.SubFingerprintId).ToList();
            solr.Delete(ids);
            solr.Commit();
            return ids.Count;
        }

        public IEnumerable<SubFingerprintData> ReadHashedFingerprintsByTrackReference(IModelReference trackReference)
        {
            var query = new SolrQuery($"trackId:{SolrModelReference.GetId(trackReference)}");
            var results = solr.Query(query, new QueryOptions { ExtraParams = new Dictionary<string, string> { { "wt", "xml" } } });
            return results.Select(GetSubFingerprintData).ToList();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprints(IEnumerable<int[]> hashes, QueryConfiguration queryConfiguration)
        {
            var clusters = queryConfiguration.Clusters;
            var threshold = queryConfiguration.ThresholdVotes;
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
                var results = solr.Query(new SolrQuery(queryString),
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

        private SubFingerprintDTO GetSubFingerprintDto(SubFingerprintData subFingerprintData)
        {
            return new SubFingerprintDTO
                       {
                           SubFingerprintId = (string)subFingerprintData.SubFingerprintReference.Id,
                           Hashes =
                               dictionaryToHashConverter.FromHashesToSolrDictionary(
                                   subFingerprintData.Hashes),
                           SequenceAt = subFingerprintData.SequenceAt,
                           SequenceNumber = (int)subFingerprintData.SequenceNumber,
                           TrackId = (string)subFingerprintData.TrackReference.Id,
                           Clusters = subFingerprintData.Clusters
                       };
        }

        private SubFingerprintData GetSubFingerprintData(SubFingerprintDTO dto)
        {
            int[] resultHashBins = dictionaryToHashConverter.FromSolrDictionaryToHashes(dto.Hashes);
            return new SubFingerprintData(resultHashBins, (uint)dto.SequenceNumber, (float)dto.SequenceAt, dto.Clusters, new SolrModelReference(dto.SubFingerprintId), new SolrModelReference(dto.TrackId));
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
