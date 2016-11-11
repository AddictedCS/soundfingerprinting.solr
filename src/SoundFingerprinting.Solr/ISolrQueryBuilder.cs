namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;

    internal interface ISolrQueryBuilder
    {
        string BuildReadQueryForHashesAndThreshold(long[] hashBins, int thresholdVotes);

        string BuildReadQueryForHashesAndThreshold(IEnumerable<long[]> allHashes, int thresholdVotes);

        string BuildReadQueryForTitleAndArtist(string title, string artist);
    }
}