namespace SoundFingerprinting.Solr.Converters
{
    internal interface IHashConverter
    {
        byte[] ToBytes(long[] array, int count);

        long[] ToLongs(byte[] array, int count);
    }
}