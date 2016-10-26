namespace SoundFingerprinting.Solr
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SolrNet;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Solr.DAO;

    internal class TrackDao : ITrackDao
    {
        private readonly ISolrOperations<TrackDTO> solr;

        public TrackDao() : this(DependencyResolver.Current.Get<ISolrOperations<TrackDTO>>())
        {
        }

        protected TrackDao(ISolrOperations<TrackDTO> solr)
        {
            this.solr = solr;
        }

        public IModelReference InsertTrack(TrackData track)
        {
            Guid id = Guid.NewGuid();
            var dto = new TrackDTO
                {
                    Id = id.ToString(),
                    Album = track.Album,
                    Artist = track.Artist,
                    GroupId = track.GroupId,
                    ISRC = track.ISRC,
                    ReleaseYear = track.ReleaseYear,
                    Title = track.Title,
                    TrackLengthSec = track.TrackLengthSec
                };

            solr.Add(dto);
            solr.Commit();
            return new SolrModelReference(id.ToString());
        }

        public TrackData ReadTrack(IModelReference trackReference)
        {
            var trackId = SolrModelReference.GetId(trackReference);
            var query = new SolrQuery(string.Format("trackId:{0}", trackId));

            var results = solr.Query(query);
            return FirstFromResultSet(results);
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            solr.Delete(SolrModelReference.GetId(trackReference));
            solr.Commit();
            return 1;
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            var query = new SolrQuery(string.Format("title:{0} AND artist:{1}", title, artist));
            var results = solr.Query(query);
            return AllFromResultSet(results);
        }
        
        public TrackData ReadTrackByISRC(string isrc)
        {
            var query = new SolrQuery(string.Format("isrc:{0}", isrc));
            var results = solr.Query(query);

            return FirstFromResultSet(results);
        }

        public IList<TrackData> ReadAll()
        {
            var query = new SolrQuery("*:*");
            var results = solr.Query(query);
            return AllFromResultSet(results);
        }

        private static TrackData Convert(TrackDTO dto)
        {
            var track = new TrackData(
                dto.ISRC,
                dto.Artist,
                dto.Title,
                dto.Album,
                dto.ReleaseYear,
                dto.TrackLengthSec,
                new SolrModelReference(dto.Id));
            track.GroupId = dto.GroupId;
            return track;
        }

        private static TrackData FirstFromResultSet(ICollection<TrackDTO> results)
        {
            if (results.Count > 0)
            {
                var dto = results.First();
                return Convert(dto);
            }

            return default(TrackData);
        }

        private static IList<TrackData> AllFromResultSet(ICollection<TrackDTO> results)
        {
            if (results.Count == 0)
            {
                return new List<TrackData>();
            }

            return results.Select(Convert).ToList();
        }
    }
}
