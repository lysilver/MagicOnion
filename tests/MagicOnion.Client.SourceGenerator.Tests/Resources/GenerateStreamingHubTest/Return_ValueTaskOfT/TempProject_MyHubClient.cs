﻿// <auto-generated />
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

// NOTE: Disable warnings for nullable reference types.
// `#nullable disable` causes compile error on old C# compilers (-7.3)
#pragma warning disable 8603 // Possible null reference return.
#pragma warning disable 8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
#pragma warning disable 8625 // Cannot convert null literal to non-nullable reference type.

namespace TempProject
{
    using global::System;
    using global::Grpc.Core;
    using global::MagicOnion;
    using global::MagicOnion.Client;
    using global::MessagePack;
    
    [global::MagicOnion.Ignore]
    public class MyHubClient : global::MagicOnion.Client.StreamingHubClientBase<global::TempProject.IMyHub, global::TempProject.IMyHubReceiver>, global::TempProject.IMyHub
    {
        protected override global::Grpc.Core.Method<global::System.Byte[], global::System.Byte[]> DuplexStreamingAsyncMethod { get; }
        
        public MyHubClient(global::Grpc.Core.CallInvoker callInvoker, global::System.String host, global::Grpc.Core.CallOptions options, global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider, global::MagicOnion.Client.IMagicOnionClientLogger logger)
            : base(callInvoker, host, options, serializerProvider, logger)
        {
            var marshaller = global::MagicOnion.MagicOnionMarshallers.ThroughMarshaller;
            DuplexStreamingAsyncMethod = new global::Grpc.Core.Method<global::System.Byte[], global::System.Byte[]>(global::Grpc.Core.MethodType.DuplexStreaming, "IMyHub", "Connect", marshaller, marshaller);
        }
        
        public global::System.Threading.Tasks.ValueTask<global::TempProject.MyObject> A(global::TempProject.MyObject a)
            => new global::System.Threading.Tasks.ValueTask<global::TempProject.MyObject>(base.WriteMessageWithResponseAsync<global::TempProject.MyObject, global::TempProject.MyObject>(-1005848884, a));
        
        public global::TempProject.IMyHub FireAndForget()
            => new FireAndForgetClient(this);
        
        [global::MagicOnion.Ignore]
        class FireAndForgetClient : global::TempProject.IMyHub
        {
            readonly MyHubClient parent;
        
            public FireAndForgetClient(MyHubClient parent)
                => this.parent = parent;
        
            public global::TempProject.IMyHub FireAndForget() => this;
            public global::System.Threading.Tasks.Task DisposeAsync() => throw new global::System.NotSupportedException();
            public global::System.Threading.Tasks.Task WaitForDisconnect() => throw new global::System.NotSupportedException();
        
            public global::System.Threading.Tasks.ValueTask<global::TempProject.MyObject> A(global::TempProject.MyObject a)
                => new global::System.Threading.Tasks.ValueTask<global::TempProject.MyObject>(parent.WriteMessageFireAndForgetAsync<global::TempProject.MyObject, global::TempProject.MyObject>(-1005848884, a));
            
        }
        
        protected override void OnBroadcastEvent(global::System.Int32 methodId, global::System.ArraySegment<global::System.Byte> data)
        {
            switch (methodId)
            {
            }
        }
        
        protected override void OnResponseEvent(global::System.Int32 methodId, global::System.Object taskCompletionSource, global::System.ArraySegment<global::System.Byte> data)
        {
            switch (methodId)
            {
                case -1005848884: // ValueTask<MyObject> A(global::TempProject.MyObject a)
                    base.SetResultForResponse<global::TempProject.MyObject>(taskCompletionSource, data);
                    break;
            }
        }
        
    }
}


