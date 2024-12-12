using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BitMinistry
{
    public static class _binarySerializers
    {

        public static TType BinaryDeserialize<TType>(this string fileName)
        {
            var binFormat = new BinaryFormatter();
            try
            {
                using (Stream fStream = File.OpenRead(fileName))
                    return (TType)binFormat.Deserialize(fStream);
            }
            catch {
                if (File.Exists(fileName)) File.Delete(fileName);
                return default(TType);
            }
        }

        public static void BinarySerialize(this object objGraph, string fileName)
        {
            var binFormat = new BinaryFormatter();
            using (Stream fStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                binFormat.Serialize(fStream, objGraph);
        }
    }
}
