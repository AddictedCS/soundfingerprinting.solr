namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;
    using System.Linq;

    internal class SolrQueryBuilder : ISolrQueryBuilder
    {
        public string BuildReadQueryForHashesAndThreshold(long[] hashBins, int thresholdVotes)
        {
            List<string> termFreqs = hashBins.Select((hash, index) => string.Format("termfreq(hashTable_{0},'{1}')", index, hash)).ToList();
            string joined = string.Join(",", termFreqs);
            string queryString = string.Format("{{!frange l={0} u={1}}}sum({2})", thresholdVotes, hashBins.Length, joined);
            return queryString;
        }

        public string BuildReadQueryForHashesAndThresholdEDismax(long[] hashBins, int thresholdVotes)
        {
            List<string> terms = hashBins.Select((hash, index) => string.Format("hashTable_{0}:{1}", index, hash)).ToList();
            return string.Join(" ", terms);
        }

        public string BuildReadQueryForTitleAndArtist(string title, string artist)
        {
            string query = string.Format(
                "title:\"{0}\" AND artist:\"{1}\"", EscapeSolrQueryInput(title), EscapeSolrQueryInput(artist));
            return query;
        }

        private static string EscapeSolrQueryInput(string title)
        {
            return title.Replace("\"", "\\\"");
        }
    }
}
