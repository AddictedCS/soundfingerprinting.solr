namespace SoundFingerprinting.Solr.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Solr.DAO;

    [TestClass]
    public class SubFingerprintDaoIntTest
    {
        private SubFingerprintDao dao = new SubFingerprintDao();

        [TestMethod]
        public void ShouldInsertSubFingerprintToSolrWithoutTheHash()
        {
            var subReference = dao.InsertSubFingerprint(new byte[0], 1, 0d, new SolrModelReference("track-id"));

            Assert.IsNotNull(subReference.Id);
        }
    }
}
