namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;

    internal interface ISolrQueryBuilder
    {
        string BuildReadQueryForHashes(int[] hashBins);

        string BuildReadQueryForHashesAndThreshold(IEnumerable<int[]> allHashes, int thresholdVotes);

        string BuildReadQueryForTitleAndArtist(string title, string artist);

        string BuildQueryForClusters(IEnumerable<string> clusters);
    }
}