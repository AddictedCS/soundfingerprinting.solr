namespace SoundFingerprinting.Solr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SubFingerprintDaoIntTest
    {
        private readonly SubFingerprintDao subFingerprintDao = new SubFingerprintDao();

        [TestMethod]
        public void ShouldInsertSubFingerprintToSolrWithoutTheHash()
        {
            //
        }
    }
}
