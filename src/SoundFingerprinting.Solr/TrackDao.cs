namespace SoundFingerprinting.Solr
{
    using System;
    using System.Collections.Generic;

    using SolrNet;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Solr.DAO;

    internal class TrackDao : ITrackDao
    {
        private ISolrOperations<TrackDTO> solr;

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
            var dto = new TrackDTO()
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
            throw new System.NotImplementedException();
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            throw new System.NotImplementedException();
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            throw new System.NotImplementedException();
        }

        public IList<TrackData> ReadAll()
        {
            throw new System.NotImplementedException();
        }
    }
}
