namespace SoundFingerprinting.Solr.Tests.Integration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    [TestFixture]
    [Category("IntegrationTest")]
    public class TrackDaoTest
    {
        private readonly TrackDao trackDao = new TrackDao();

        [TearDown]
        public void TearDown()
        {
            var allTracks = trackDao.ReadAll();
            TearDownTracks(allTracks.Select(t => t.TrackReference));
        }

        [Test]
        public void ShouldInsertTrack()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 1986, 3.6);
            var reference = trackDao.InsertTrack(track);

            Assert.IsNotNull(reference);
        }

        [Test]
        public void ShouldReadTrack()
        {
            var expected = new TrackData("isrc", "artist", "title", "album", 1994, 4.0);
            var reference = trackDao.InsertTrack(expected);

            var actual = trackDao.ReadTrack(reference);

            AssertTracksAreEqual(expected, actual);
        }

        [Test]
        public void ShouldReadMultipleTracks()
        {
            var refs = new List<IModelReference>();
            var l = 10;
            for (int i = 0; i < l; ++i)
            {
                var track = new TrackData($"isrc_{i}", "artist", "title", "album", 1986, 100);
                var reference = trackDao.InsertTrack(track);
                refs.Add(reference);
            }

            var actual = trackDao.ReadTracks(refs);
            Assert.AreEqual(10, actual.Count);
        }

        [Test]
        public void ShouldReadNonExistingTrack()
        {
            var actual = trackDao.ReadTrack(new SolrModelReference("256-256"));

            Assert.IsNull(actual);
        }

        [Test]
        public void ShouldDeleteTrack()
        {
            var expected = new TrackData("isrc", "artist", "title", "album", 1994, 4.0);
            var reference = trackDao.InsertTrack(expected);
            var actual = trackDao.ReadTrack(reference);
            AssertTracksAreEqual(expected, actual);

            trackDao.DeleteTrack(reference);

            var tracks = trackDao.ReadAll();
            Assert.AreEqual(0, tracks.Count);
        }

        [Test]
        public void ShouldInsertMultipleTracksConcurrently()
        {
            const int NumberOfTracks = 100;
            var modelReferences = new ConcurrentBag<IModelReference>();
            for (int i = 0; i < NumberOfTracks; i++)
            {
                var modelReference =
                    trackDao.InsertTrack(
                        new TrackData("isrc", "artist", "title", "album", 2012, 200));

                Assert.IsFalse(modelReferences.Contains(modelReference));
                modelReferences.Add(modelReference);
            }

            Assert.AreEqual(NumberOfTracks, trackDao.ReadAll().Count);
        }

        [Test]
        public void ShouldReadAllTracksFromTheCore()
        {
            const int TrackCount = 5;
            var expectedTracks = InsertRandomTracks(TrackCount);

            var tracks = trackDao.ReadAll();

            Assert.AreEqual(TrackCount, tracks.Count);
            foreach (var expectedTrack in expectedTracks)
            {
                Assert.IsTrue(tracks.Any(track => track.ISRC == expectedTrack.ISRC));
            }
        }

        [Test]
        public void ShouldInsertMultipleTracksViaOneCall()
        {
            const int TrackCount = 100;
            var tracks = InsertRandomTracks(TrackCount);

            var actualTracks = trackDao.ReadAll();

            Assert.AreEqual(tracks.Count, actualTracks.Count);
            for (int i = 0; i < actualTracks.Count; i++)
            {
                AssertTracksAreEqual(
                    tracks[i], actualTracks.First(track => track.TrackReference.Equals(tracks[i].TrackReference)));
            }
        }

        [Test]
        public void ReadTrackByArtistAndTitleTest()
        {
            var track = GetRandomTrack();
            track.Artist = "This is a long artist name";
            track.Title = "This is a long title name";
            trackDao.InsertTrack(track);

            var tracks = trackDao.ReadTrackByArtistAndTitleName(track.Artist, track.Title);

            Assert.IsNotNull(tracks);
            Assert.IsTrue(tracks.Count == 1);
            AssertTracksAreEqual(track, tracks[0]);
        }

        [Test]
        public void ReadByNonExistentArtistAndTitleTest()
        {
            var tracks = trackDao.ReadTrackByArtistAndTitleName("artist", "title");

            Assert.IsTrue(tracks.Count == 0);
        }

        [Test]
        public void ReadTrackByISRCTest()
        {
            var expectedTrack = GetRandomTrack();
            trackDao.InsertTrack(expectedTrack);

            var actualTrack = trackDao.ReadTrackByISRC(expectedTrack.ISRC);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [Test]
        public void DeleteCollectionOfTracksTest()
        {
            const int NumberOfTracks = 10;
            InsertRandomTracks(NumberOfTracks);

            var allTracks = trackDao.ReadAll();

            Assert.IsTrue(allTracks.Count == NumberOfTracks);
            foreach (var track in allTracks)
            {
                trackDao.DeleteTrack(track.TrackReference);
            }

            Assert.IsTrue(trackDao.ReadAll().Count == 0);
        }

        [Test]
        public void InserTrackShouldAcceptEmptyEntriesCodes()
        {
            TrackData track = new TrackData(string.Empty, string.Empty, string.Empty, string.Empty, 1986, 200);
            var trackReference = trackDao.InsertTrack(track);

            var actualTrack = trackDao.ReadTrack(trackReference);

            AssertTracksAreEqual(track, actualTrack);
        }

        private void AssertTracksAreEqual(TrackData expectedTrack, TrackData actualTrack)
        {
            Assert.AreEqual(expectedTrack.Album, actualTrack.Album);
            Assert.AreEqual(expectedTrack.Artist, actualTrack.Artist);
            Assert.AreEqual(expectedTrack.Title, actualTrack.Title);
            Assert.AreEqual(expectedTrack.Length, actualTrack.Length);
            Assert.AreEqual(expectedTrack.ISRC, actualTrack.ISRC);
        }

        private List<TrackData> InsertRandomTracks(int trackCount)
        {
            var tracks = new List<TrackData>();
            for (int i = 0; i < trackCount; i++)
            {
                var track = this.GetRandomTrack();
                tracks.Add(track);
                var reference = trackDao.InsertTrack(track);
                track.TrackReference = reference;
            }

            return tracks;
        }

        private TrackData GetRandomTrack()
        {
            return new TrackData(Guid.NewGuid().ToString(), "artist", "title", "album", 1986, 360);
        }

        private void TearDownTracks(IEnumerable<IModelReference> modelReferences)
        {
            foreach (var modelReference in modelReferences)
            {
                trackDao.DeleteTrack(modelReference);
            }
        }
    }
}
