namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;

    internal interface ISolrQueryBuilder
    {
        string BuildReadQueryForHashes(long[] hashBins);

        string BuildReadQueryForHashesAndThreshold(IEnumerable<long[]> allHashes, int thresholdVotes);

        string BuildReadQueryForTitleAndArtist(string title, string artist);

        string BuildQueryForClusters(IEnumerable<string> clusters);
    }
}