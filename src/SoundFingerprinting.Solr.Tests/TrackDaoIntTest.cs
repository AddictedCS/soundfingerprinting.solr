namespace SoundFingerprinting.Solr.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    [TestClass]
    public class TrackDaoIntTest
    {
        private readonly TrackDao trackDao = new TrackDao();

        [TestCleanup]
        public void TearDown()
        {
            var allTracks = trackDao.ReadAll();
            TearDownTracks(allTracks.Select(t => t.TrackReference));
        }

        [TestMethod]
        public void ShouldInsertTrack()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 1986, 3.6);
            var reference = trackDao.InsertTrack(track);

            Assert.IsNotNull(reference);
        }

        [TestMethod]
        public void ShouldReadTrack()
        {
            var expected = new TrackData("isrc", "artist", "title", "album", 1994, 4.0);
            var reference = trackDao.InsertTrack(expected);

            var actual = trackDao.ReadTrack(reference);

            AssertTracksAreEqual(expected, actual);
        }

        [TestMethod]
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

        [TestMethod]
        public void ShouldInsertMultipleTracksConcurrently()
        {
            const int NumberOfTracks = 1000;
            var modelReferences = new ConcurrentBag<IModelReference>();
            for (int i = 0; i < NumberOfTracks; i++)
            {
                var modelReference =
                    trackDao.InsertTrack(
                        new TrackData("isrc", "artist", "title", "album", 2012, 200) { GroupId = "group-id" });

                Assert.IsFalse(modelReferences.Contains(modelReference));
                modelReferences.Add(modelReference);
            }

            Assert.AreEqual(NumberOfTracks, trackDao.ReadAll().Count);
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void ReadByNonExistentArtistAndTitleTest()
        {
            var tracks = trackDao.ReadTrackByArtistAndTitleName("artist", "title");

            Assert.IsTrue(tracks.Count == 0);
        }

        [TestMethod]
        public void ReadTrackByISRCTest()
        {
            var expectedTrack = GetRandomTrack();
            trackDao.InsertTrack(expectedTrack);

            var actualTrack = trackDao.ReadTrackByISRC(expectedTrack.ISRC);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
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

        [TestMethod]
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
            Assert.AreEqual(expectedTrack.TrackLengthSec, actualTrack.TrackLengthSec);
            Assert.AreEqual(expectedTrack.ISRC, actualTrack.ISRC);
            Assert.AreEqual(expectedTrack.GroupId, actualTrack.GroupId);
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
            return new TrackData(Guid.NewGuid().ToString(), "artist", "title", "album", 1986, 360)
            {
                GroupId = Guid.NewGuid().ToString().Substring(0, 20)
            };
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
