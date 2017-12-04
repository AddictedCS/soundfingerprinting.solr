namespace SoundFingerprinting.Solr
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Practices.ServiceLocation;

    using SolrNet;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Solr.DAO;

    internal class TrackDao : ITrackDao
    {
        private readonly ISolrOperations<TrackDTO> solrForTracksCore;
        private readonly ISolrOperations<SubFingerprintDTO> solrForSubfingerprintsCore;
        private readonly ISolrQueryBuilder solrQueryBuilder;

        public TrackDao()
            : this(
                ServiceLocator.Current.GetInstance<ISolrOperations<TrackDTO>>(),
                ServiceLocator.Current.GetInstance<ISolrOperations<SubFingerprintDTO>>(),
                new SolrQueryBuilder())
        {
        }

        protected TrackDao(
            ISolrOperations<TrackDTO> solrForTracksCore,
            ISolrOperations<SubFingerprintDTO> solrForSubfingerprintsCore,
            ISolrQueryBuilder solrQueryBuilder)
        {
            this.solrForTracksCore = solrForTracksCore;
            this.solrForSubfingerprintsCore = solrForSubfingerprintsCore;
            this.solrQueryBuilder = solrQueryBuilder;
        }

        public IModelReference InsertTrack(TrackData track)
        {
            Guid id = Guid.NewGuid();
            var dto = new TrackDTO
                {
                    Id = id.ToString(),
                    Album = track.Album,
                    Artist = track.Artist,
                    ISRC = track.ISRC,
                    ReleaseYear = track.ReleaseYear,
                    Title = track.Title,
                    TrackLengthSec = track.Length
                };

            solrForTracksCore.Add(dto);
            solrForTracksCore.Commit();
            var trackReference = new SolrModelReference(id.ToString());
            track.TrackReference = trackReference;
            return trackReference;
        }

        public TrackData ReadTrack(IModelReference trackReference)
        {
            var trackId = SolrModelReference.GetId(trackReference);
            var query = new SolrQuery($"trackId:{trackId}");

            var results = solrForTracksCore.Query(query);
            return FirstFromResultSet(results);
        }

        public List<TrackData> ReadTracks(IEnumerable<IModelReference> ids)
        {
            string sids = string.Join(",", ids.Select(SolrModelReference.GetId));
            var query = new SolrQuery($"trackId:({sids})");
            var results = solrForTracksCore.Query(query);
            return AllFromResultSet(results).ToList();
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            int subIds = DeleteSubFingerprintsForTrack(trackReference);
            solrForTracksCore.Delete(SolrModelReference.GetId(trackReference));
            solrForTracksCore.Commit();
            return 1 + subIds;
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            var query = solrQueryBuilder.BuildReadQueryForTitleAndArtist(title, artist);
            var results = solrForTracksCore.Query(query);
            return AllFromResultSet(results);
        }
        
        public TrackData ReadTrackByISRC(string isrc)
        {
            var query = new SolrQuery(string.Format("isrc:{0}", isrc));
            var results = solrForTracksCore.Query(query);
            return FirstFromResultSet(results);
        }

        public IList<TrackData> ReadAll()
        {
            var query = new SolrQuery("*:*");
            var results = solrForTracksCore.Query(query);
            return AllFromResultSet(results);
        }

        private int DeleteSubFingerprintsForTrack(IModelReference trackReference)
        {
            string trackId = SolrModelReference.GetId(trackReference);
            string readAll = $"trackId:{trackId}";
            var results = solrForSubfingerprintsCore.Query(new SolrQuery(readAll));
            var ids = results.Select(dto => dto.SubFingerprintId).ToList();
            solrForSubfingerprintsCore.Delete(ids);
            solrForSubfingerprintsCore.Commit();
            return ids.Count;
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
