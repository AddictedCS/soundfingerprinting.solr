namespace SoundFingerprinting.Solr
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CommonServiceLocator;

    using SolrNet;
    using SolrNet.Commands.Parameters;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Solr.DAO;

    internal class TrackDao : ITrackDao
    {
        private readonly ISolrOperations<TrackDTO> solrForTracksCore;

        private readonly ISolrQueryBuilder solrQueryBuilder;

        public TrackDao() : this(ServiceLocator.Current.GetInstance<ISolrOperations<TrackDTO>>(), new SolrQueryBuilder())
        {
        }

        protected TrackDao(ISolrOperations<TrackDTO> solrForTracksCore, ISolrQueryBuilder solrQueryBuilder)
        {
            this.solrForTracksCore = solrForTracksCore;
            this.solrQueryBuilder = solrQueryBuilder;
        }

        public int Count
        {
            get
            {
                var query = new SolrQuery("*:*");
                return solrForTracksCore.Query(query).NumFound;
            }
        }

        public TrackData InsertTrack(TrackInfo track)
        {
            var id = Guid.NewGuid().ToString();
            var trackReference = new SolrModelReference(id);
            var trackData = new TrackData(track.Id, track.Artist, track.Title, string.Empty, 0, track.DurationInSeconds, trackReference);
            InsertTrack(trackData);
            return trackData;
        }

        public void InsertTrack(TrackData trackData)
        {
            var dto = new TrackDTO
                          {
                              Id = (string)trackData.TrackReference.Id,
                              Artist = trackData.Artist,
                              ISRC = trackData.ISRC,
                              Title = trackData.Title,
                              TrackLengthSec = trackData.Length
                          };

            solrForTracksCore.Add(dto);
            solrForTracksCore.Commit();
        }

        public TrackData ReadTrack(IModelReference trackReference)
        {
            var trackId = SolrModelReference.GetId(trackReference);
            var query = new SolrQuery($"trackId:{trackId}");

            var results = solrForTracksCore.Query(query, new QueryOptions { ExtraParams = new Dictionary<string, string> { { "wt", "xml" } } });
            return FirstFromResultSet(results);
        }

        public IEnumerable<TrackData> ReadTrackByReferences(IEnumerable<IModelReference> ids)
        {
            string sids = string.Join(",", ids.Select(id => $"\"{SolrModelReference.GetId(id)}\""));
            var query = new SolrQuery($"trackId:({sids})");
            var results = solrForTracksCore.Query(query, new QueryOptions { ExtraParams = new Dictionary<string, string> { { "wt", "xml" } } });
            return AllFromResultSet(results).ToList();
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            solrForTracksCore.Delete(SolrModelReference.GetId(trackReference));
            solrForTracksCore.Commit();
            return 1;
        }

        public IEnumerable<TrackData> ReadTrackByTitle(string title)
        {
            var query = solrQueryBuilder.BuildReadQueryForTitle(title);
            var results = solrForTracksCore.Query(
                query,
                new QueryOptions { ExtraParams = new Dictionary<string, string> { { "wt", "xml" } } });
            return AllFromResultSet(results);
        }
        
        public TrackData ReadTrackById(string id)
        {
            var query = new SolrQuery($"isrc:{id}");
            var results = solrForTracksCore.Query(query, new QueryOptions { ExtraParams = new Dictionary<string, string> { { "wt", "xml" } } });
            return FirstFromResultSet(results);
        }

        public IEnumerable<TrackData> ReadAll()
        {
            var query = new SolrQuery("*:*");
            var results = solrForTracksCore.Query(query, new QueryOptions { ExtraParams = new Dictionary<string, string> { { "wt", "xml" } } });
            return AllFromResultSet(results);
        }

        private TrackData Convert(TrackDTO dto)
        {
            var track = new TrackData(
                dto.ISRC,
                dto.Artist,
                dto.Title,
                dto.Album,
                dto.ReleaseYear,
                dto.TrackLengthSec,
                new SolrModelReference(dto.Id));
            return track;
        }

        private TrackData FirstFromResultSet(ICollection<TrackDTO> results)
        {
            if (results.Count > 0)
            {
                var dto = results.First();
                return Convert(dto);
            }

            return default(TrackData);
        }

        private IList<TrackData> AllFromResultSet(ICollection<TrackDTO> results)
        {
            if (results.Count == 0)
            {
                return new List<TrackData>();
            }

            return results.Select(Convert).ToList();
        }
    }
}
