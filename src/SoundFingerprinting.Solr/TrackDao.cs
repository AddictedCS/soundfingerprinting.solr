namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;
    using System.Net.Http;

    using Newtonsoft.Json;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    public class TrackDao : ITrackDao
    {
        private const string SolrHttpAddress = "http://solr.server:8985/update";

        public IModelReference InsertTrack(TrackData track)
        {
            HttpClient client = new HttpClient();
            string json = JsonConvert.SerializeObject(track);
            var response = client.PostAsync(SolrHttpAddress, new StringContent(json)).Result;
            var strResponse = response.Content.ReadAsStringAsync().Result;
            return new SolrModelReference(strResponse);
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
