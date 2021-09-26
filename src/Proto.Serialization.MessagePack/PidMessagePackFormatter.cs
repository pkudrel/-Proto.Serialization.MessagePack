using System;
using Google.Protobuf;
using MessagePack;
using MessagePack.Formatters;

namespace Proto.Serialization.MessagePack
{
    public sealed class PidMessagePackFormatter : IMessagePackFormatter<PID>
    {
        public static readonly PidMessagePackFormatter Instance = new PidMessagePackFormatter();

        public PID Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil()) return null;
            var sequence = reader.ReadBytes();
            return sequence.HasValue ? PID.Parser.ParseFrom(sequence.Value) : null;
        }

        public void Serialize(ref MessagePackWriter writer, PID value, MessagePackSerializerOptions options)
        {
            writer.Write(new ReadOnlySpan<byte>(value.ToByteArray()));
        }
    }
}