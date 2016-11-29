namespace SoundFingerprinting.Solr.Tests
{
    using System;
    using System.IO;

    using NUnit.Framework;

    public abstract class IntegrationTestWithSampleFiles
    {
        private readonly byte[] genericSignatureArray = new[]
            {
                (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10, (byte)11,
                (byte)12, (byte)13, (byte)14, (byte)15, (byte)16, (byte)17, (byte)18, (byte)19, (byte)20, (byte)21,
                (byte)22, (byte)23, (byte)24, (byte)25
            };

        protected string PathToMp3 = Path.Combine(TestContext.CurrentContext.TestDirectory, "Chopin.mp3");

        protected byte[] GenericSignature()
        {
            byte[] copy = new byte[this.genericSignatureArray.Length];
            Array.Copy(this.genericSignatureArray, copy, copy.Length);
            return copy;
        }
    }
}
