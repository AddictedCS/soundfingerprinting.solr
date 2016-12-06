namespace SoundFingerprinting.Solr.DAO
{
    using SolrNet.Attributes;

    internal class TrackDTO
    {
        [SolrUniqueKey("trackId")]
        public string Id { get; set; }

        [SolrField("artist")]
        public string Artist { get; set; }

        [SolrField("title")]
        public string Title { get; set; }

        [SolrField("isrc")]
        public string ISRC { get; set; }

        [SolrField("album")]
        public string Album { get; set; }

        [SolrField("releaseYear")]
        public int ReleaseYear { get; set; }

        [SolrField("trackLengthSec")]
        public double TrackLengthSec { get; set; }
    }
}
