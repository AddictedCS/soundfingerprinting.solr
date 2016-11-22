namespace SoundFingerprinting.Solr.Tests
{
    using System.IO;
    using System.Reflection;

    public abstract class IntegrationTestWithSampleFiles
    {
        protected string PathToMp3 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Kryptonite.mp3");

        protected readonly byte[] GenericSignature = new[]
            {
                (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10, (byte)11,
                (byte)12, (byte)13, (byte)14, (byte)15, (byte)16, (byte)17, (byte)18, (byte)19, (byte)20, (byte)21,
                (byte)22, (byte)23, (byte)24, (byte)25
            };
    }
}
