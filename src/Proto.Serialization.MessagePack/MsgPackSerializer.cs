using System;
using System.Collections.Generic;
using Google.Protobuf;
using MessagePack;
using MessagePack.Resolvers;
using Proto.Remote;

namespace Proto.Serialization.MessagePack
{
    public class MsgPackSerializer : ISerializer
    {
        private readonly Dictionary<string, Func<byte[], object>> _deserializers
            = new Dictionary<string, Func<byte[], object>>();

        private readonly MessagePackSerializerOptions _options;


        public MsgPackSerializer()
        {
            var resolver = CompositeResolver.Create(
                // resolver custom types first
                BuiltinResolver.Instance,
                MessagePackFormatter.Instance,
                AttributeFormatterResolver.Instance,
                DynamicEnumAsStringResolver.Instance,
                ContractlessStandardResolver.Instance
            );
            _options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
        }

        public ByteString Serialize(object obj)
        {
            return ByteString.CopyFrom(MessagePackSerializer.Serialize(obj, _options));
        }

        public object Deserialize(ByteString bytes, string typeName)
        {
            if (_deserializers.TryGetValue(typeName, out var deserialize)) return deserialize(bytes.ToByteArray());

            return null;
        }

        public string GetTypeName(object message)
        {
            return message.GetType().ToString();
        }

        public bool CanSerialize(object obj)
        {
            var key = obj.GetType().ToString();
            var can = _deserializers.ContainsKey(key);
            Console.WriteLine($"Can: {key} => {can} ");
            return can;
        }

        public void RegisterType<T>() where T : class
        {
            _deserializers.Add(typeof(T).ToString(), x => MessagePackSerializer.Deserialize<T>(x, _options));
        }
    }
}