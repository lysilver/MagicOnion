﻿// <auto-generated />
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace MagicOnion.Formatters
{
    using System;
    using MessagePack;

    public sealed class TaskCreationOptionsFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::System.Threading.Tasks.TaskCreationOptions>
    {
        public void Serialize(ref MessagePackWriter writer, global::System.Threading.Tasks.TaskCreationOptions value, MessagePackSerializerOptions options)
        {
            writer.Write((Int32)value);
        }
        
        public global::System.Threading.Tasks.TaskCreationOptions Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return (global::System.Threading.Tasks.TaskCreationOptions)reader.ReadInt32();
        }
    }

}

#pragma warning restore 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 612
#pragma warning restore 618

