namespace SoundFingerprinting.Solr.Tests
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    [DeploymentItem(@"TestEnvironment\floatsamples.bin")]
    [DeploymentItem(@"TestEnvironment\Kryptonite.mp3")]
    [DeploymentItem(@"x86", @"x86")]
    [DeploymentItem(@"x64", @"x64")]
    [TestClass]
    public class SolrModelServiceIntTest
    {
        private readonly SolrModelService modelService = new SolrModelService();
        private readonly FingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();
        private readonly NAudioService nAudioService = new NAudioService();

        [TestMethod]
        public void ShouldInsertAllSubfingerprintsForTrack()
        {
            List<HashedFingerprint> hashedFingerprints = fingerprintCommandBuilder.BuildFingerprintCommand()
                .From("Kryptonite.mp3")
                .UsingServices(nAudioService)
                .Hash()
                .Result;

            var track = new TrackData("isrc", "artist", "title", "album", 1986, 4d);
            var trackReference = modelService.InsertTrack(track);

            modelService.InsertHashDataForTrack(hashedFingerprints, trackReference);
        }
    }
}
