namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    internal class TrackDao : ITrackDao
    {

        public IModelReference InsertTrack(TrackData track)
        {
            return null;
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
