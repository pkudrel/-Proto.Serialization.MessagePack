using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using Proto.Serialization.MessagePack.Systems;

namespace Proto.Serialization.MessagePack
{
    public sealed class MessagePackFormatter : IFormatterResolver
    {
        public static IFormatterResolver Instance = new MessagePackFormatter();

        private MessagePackFormatter()
        {
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> Formatter;

            static FormatterCache()
            {
                Formatter = (IMessagePackFormatter<T>)MyTypeResolverGetFormatterHelper.GetFormatter(typeof(T));
            }
        }
    }

    internal static class MyTypeResolverGetFormatterHelper
    {
        private static readonly Dictionary<Type, object> _formatterMap = new Dictionary<Type, object>
        {
            { typeof(PID), PidMessagePackFormatter.Instance }
        };

        internal static object GetFormatter(Type t)
        {
            if (_formatterMap.TryGetValue(t, out var formatter)) return formatter;
            return null;
        }
    }
}