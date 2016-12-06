namespace SoundFingerprinting.Solr.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;

    public abstract class IntegrationTestWithSampleFiles
    {
        private readonly byte[] genericSignatureArray = new[]
            {
                (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10, (byte)11,
                (byte)12, (byte)13, (byte)14, (byte)15, (byte)16, (byte)17, (byte)18, (byte)19, (byte)20, (byte)21,
                (byte)22, (byte)23, (byte)24, (byte)25
            };

        protected string PathToSamples= Path.Combine(TestContext.CurrentContext.TestDirectory, "chopinsamples.bin");

        protected void AssertHashDatasAreTheSame(IList<HashedFingerprint> firstHashDatas, IList<HashedFingerprint> secondHashDatas)
        {
            Assert.AreEqual(firstHashDatas.Count, secondHashDatas.Count);

            // hashes are not ordered as parallel computation is involved
            firstHashDatas = SortHashesByFirstValueOfHashBin(firstHashDatas);
            secondHashDatas = SortHashesByFirstValueOfHashBin(secondHashDatas);

            for (int i = 0; i < firstHashDatas.Count; i++)
            {
                CollectionAssert.AreEqual(firstHashDatas[i].SubFingerprint, secondHashDatas[i].SubFingerprint);
                CollectionAssert.AreEqual(firstHashDatas[i].HashBins, secondHashDatas[i].HashBins);
                Assert.AreEqual(firstHashDatas[i].SequenceNumber, secondHashDatas[i].SequenceNumber);
                Assert.AreEqual(firstHashDatas[i].StartsAt, secondHashDatas[i].StartsAt, 0.001);
            }
        }

        protected byte[] GenericSignature()
        {
            byte[] copy = new byte[genericSignatureArray.Length];
            Array.Copy(genericSignatureArray, copy, copy.Length);
            return copy;
        }

        protected AudioSamples GetAudioSamples()
        {
            var serializer = new BinaryFormatter();

            using (Stream stream = new FileStream(PathToSamples, FileMode.Open, FileAccess.Read))
            {
                return (AudioSamples)serializer.Deserialize(stream);
            }
        }

        protected float[] GetQuerySamples(AudioSamples audioSamples, int startAtSecond, int secondsToProcess)
        {
            int sampleRate = audioSamples.SampleRate;
            float[] querySamples = new float[sampleRate * secondsToProcess];
            int startAt = startAtSecond * sampleRate;
            Array.Copy(audioSamples.Samples, startAt, querySamples, 0, querySamples.Length);
            return querySamples;
        }

        private List<HashedFingerprint> SortHashesByFirstValueOfHashBin(IEnumerable<HashedFingerprint> hashDatasFromFile)
        {
            return hashDatasFromFile.OrderBy(hashData => hashData.SequenceNumber).ToList();
        }
    }
}
