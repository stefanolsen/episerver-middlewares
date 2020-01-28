using System;
using System.IO;
using Microsoft.Owin.Security.DataHandler.Serializer;

namespace StefanOlsen.Episerver.Owin.AnonymousId
{
    internal class AnonymousIdSerializer : IDataSerializer<AnonymousId>
    {
        public byte[] Serialize(AnonymousId model)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    Write(binaryWriter, model);
                    binaryWriter.Flush();

                    return memoryStream.ToArray();
                }
            }
        }

        public AnonymousId Deserialize(byte[] data)
        {
            using (MemoryStream input = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(input))
                {
                    return Read(reader);
                }
            }
        }

        public static void Write(BinaryWriter writer, AnonymousId data)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            writer.Write(1);
            writer.Write(data.Id);
            writer.Write(data.ExpireDate.Ticks);
        }

        public static AnonymousId Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.ReadInt32() != 1)
            {
                return null;
            }

            string id = reader.ReadString();
            DateTime expireDate = new DateTime(reader.ReadInt64());

            var anonymousId = new AnonymousId(id, expireDate);

            return anonymousId;
        }
    }
}