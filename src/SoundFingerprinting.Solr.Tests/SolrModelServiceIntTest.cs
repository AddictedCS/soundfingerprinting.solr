namespace SoundFingerprinting.Solr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Strides;

    [TestClass]
    public class SolrModelServiceIntTest : IntegrationTestWithSampleFiles
    {
        private readonly SolrModelService modelService = new SolrModelService();
        private readonly FingerprintCommandBuilder fcb = new FingerprintCommandBuilder();
        private readonly NAudioService audioService = new NAudioService();
        private readonly TrackDao trackDao = new TrackDao();

        [TestCleanup]
        public void TearDown()
        {
            var allTracks = trackDao.ReadAll();
            foreach (var track in allTracks)
            {
                trackDao.DeleteTrack(track.TrackReference);
            }
        }

        [TestMethod]
        public void ShouldInsertAllSubfingerprintsForTrack()
        {
            var hashedFingerprints = fcb.BuildFingerprintCommand()
                                         .From(PathToMp3)
                                         .UsingServices(this.audioService)
                                         .Hash()
                                         .Result;

            var track = new TrackData("isrc", "artist", "title", "album", 1986, 4d);
            var trackReference = modelService.InsertTrack(track);

            modelService.InsertHashDataForTrack(hashedFingerprints, trackReference);
        }

        [TestMethod]
        public void ShouldReadSubFingerprintsByHashBucketsHavingThreshold()
        {
            TrackData firstTrack = new TrackData("isrc1", "artist", "title", "album", 1986, 200);
            var firstTrackReference = modelService.InsertTrack(firstTrack);
            TrackData secondTrack = new TrackData("isrc2", "artist", "title", "album", 1986, 200);
            var secondTrackReference = modelService.InsertTrack(secondTrack);
            long[] firstTrackBuckets = new long[]
                {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 
                };
            long[] secondTrackBuckets = new long[]
                {
                    2, 2, 4, 5, 6, 7, 7, 9, 10, 11, 12, 13, 14, 14, 16, 17, 18, 19, 20, 20, 22, 23, 24, 25, 26 
                };
            var firstHashData = new HashedFingerprint(GenericSignature, firstTrackBuckets, 1, 0.928);
            var secondHashData = new HashedFingerprint(GenericSignature, secondTrackBuckets, 1, 0.928);

            modelService.InsertHashDataForTrack(new[] { firstHashData }, firstTrackReference);
            modelService.InsertHashDataForTrack(new[] { secondHashData }, secondTrackReference);

            // query buckets are similar with 5 elements from first track and 4 elements from second track
            long[] queryBuckets = new long[]
                {
                    3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 
                };

            var subFingerprints = modelService.ReadSubFingerprints(queryBuckets, new DefaultQueryConfiguration());

            Assert.IsTrue(subFingerprints.Count == 1);
            Assert.AreEqual(firstTrackReference, subFingerprints[0].TrackReference);
        }

        [TestMethod]
        public void ShouldDeleteSubfingerprintsOnTrackDelete()
        {
            const int SecondsToProcess = 20;
            const int StartAtSecond = 30;

            var track = new TrackData("isrc", "artist", "title", "album", 1986, 3.3d);
            var trackReference = trackDao.InsertTrack(track);
            var hashData = fcb.BuildFingerprintCommand()
                   .From(PathToMp3, SecondsToProcess, StartAtSecond)
                   .WithFingerprintConfig(config =>
                       {
                           config.SpectrogramConfig.Stride = new StaticStride(0);
                       })
                   .UsingServices(this.audioService)
                   .Hash()
                   .Result;

            modelService.InsertHashDataForTrack(hashData, trackReference);

            int modifiedRows = trackDao.DeleteTrack(trackReference);

            Assert.AreEqual(1 + (int)(20 / 1.48), modifiedRows);
        }
    }
}
