namespace SoundFingerprinting.Solr.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO.Data;

    [TestClass]
    public class TrackDaoIntTest
    {
        [TestMethod]
        public void ShouldInsertTrack()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 1986, 3.6);
            var trackDao = new TrackDao();
            var reference = trackDao.InsertTrack(track);
            Assert.IsNotNull(reference);
        }
    }
}
