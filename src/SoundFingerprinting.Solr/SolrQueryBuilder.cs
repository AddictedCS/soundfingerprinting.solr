namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Data;

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

        public string BuildQueryForHashes(IEnumerable<HashedFingerprint> allHashes, int thresholdVotes)
        {
            List<string> allSubQueries = new List<string>();
            foreach (var hash in allHashes)
            {
                List<string> terms = hash.HashBins.Select((h, index) => string.Format("hashTable_{0}:{1}", index, h)).ToList();
                string oneQuery = string.Format("_query_:\"{{!edismax mm={0}}}{1}\"", thresholdVotes, string.Join(" ", terms));
                allSubQueries.Add(oneQuery);
            }

            return string.Join(" OR ", allSubQueries);
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
