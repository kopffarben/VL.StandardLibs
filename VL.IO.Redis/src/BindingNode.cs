﻿#nullable enable
using System;
using System.Reactive.Disposables;
using VL.Core;
using VL.Core.Import;
using VL.Model;
using VL.Lib.Reactive;

namespace VL.IO.Redis
{
    [ProcessNode(Name = "Binding")]
    public class BindingNode : IDisposable
    {
        private readonly SerialDisposable _current = new();
        private readonly NodeContext _nodeContext;

        private (RedisClient? client, IChannel? channel, string? key, Initialization initialization, RedisBindingType bindingType, CollisionHandling collisionHandling, SerializationFormat? serializationFormat) _config;

        public BindingNode([Pin(Visibility = PinVisibility.Hidden)] NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        public void Update(
            RedisClient? client, 
            IChannel? channel, 
            string? key,
            Initialization initialization = Initialization.Redis,
            RedisBindingType bindingType = RedisBindingType.SendAndReceive,
            CollisionHandling collisionHandling = default,
            SerializationFormat? serializationFormat = default)
        {
            var config = (client, channel, key, initialization, bindingType, collisionHandling, serializationFormat);
            if (config == _config)
                return;

            _config = config;
            _current.Disposable = null;

            if (client is null || channel is null || string.IsNullOrWhiteSpace(key))
                return;

            var model = new BindingModel(key, initialization, bindingType, collisionHandling, serializationFormat);
            try
            {
                _current.Disposable = client.AddBinding(model, channel);
            }
            catch (Exception e)
            {
                // TODO: Use logger / add better API which also works in exported apps
                _current.Disposable = IVLRuntime.Current?.AddException(_nodeContext, e);
            }
        }

        public void Dispose()
        {
            _current.Dispose();
        }
    }
}