namespace SoundFingerprinting.Solr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SubFingerprintDaoIntTest
    {
        private SubFingerprintDao dao = new SubFingerprintDao();

        [TestMethod]
        public void ShouldInsertSubFingerprintToSolrWithoutTheHash()
        {
            var subReference = this.dao.InsertSubFingerprint(new byte[0], 1, 0d, new SolrModelReference("track-id"));

            Assert.IsNotNull(subReference.Id);
        }
    }
}
