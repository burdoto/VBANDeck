using System;
using System.Collections.Generic;
using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;

namespace VBANDeck.Model {
    public class DeviceManager {
        private readonly StreamDeckConnection       _connection;
        private readonly Dictionary<string, Device> _instances = new Dictionary<string, Device>();

        protected internal DeviceManager(StreamDeckConnection connection) {
            _connection = connection;
        }

        public Device GetOrCreate(string opaqueValue) {
            Device device;
            if (!_instances.ContainsKey(opaqueValue)) {
                device = new Device(_connection, opaqueValue);
                _instances.Add(opaqueValue, device);
            }

            _instances.TryGetValue(opaqueValue, out device);
            return device;
        }

        public bool Destroy(string opaqueValue) {
            return _instances.Remove(opaqueValue);
        }
    }

    public class Device : StreamDeckEntity {
        private const string Uninitialized = "ActionButton is not initialized!";

        private  DeviceModifier _modifier;
        internal DeviceSize     SizeF;
        internal DeviceType     TypeF;

        internal Device(StreamDeckConnection conn, string context) : base(conn, context) { }

        public DeviceModifier Modifier => _modifier ?? (_modifier = new DeviceModifier(this));
        public DeviceSize     Size     => SizeF     ?? throw new NullReferenceException(Uninitialized);
        public DeviceType     Type     => TypeF;
    }

    public class DeviceModifier {
        private readonly Device _device;

        internal DeviceModifier(Device device) {
            _device = device;
        }

        public DeviceModifier DefineSize(DeviceSize deviceSize) {
            _device.SizeF = deviceSize;
            return this;
        }

        public DeviceModifier DefineType(DeviceType deviceType) {
            _device.TypeF = deviceType;
            return this;
        }
    }
}