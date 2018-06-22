using System.Collections.Generic;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Gdk.Core
{
    public abstract class MessagesReceived<T> : Component
    {
        public List<T> Buffer = new List<T>();
    }

    public class ComponentsUpdated<T> : MessagesReceived<T>
    {
    }

    public class CommandRequests<T> : MessagesReceived<T> where T : IIncomingCommandRequest
    {
    }

    public class CommandResponses<T> : MessagesReceived<T> where T : IIncomingCommandResponse
    {
    }

    public class EventsReceived<T> : MessagesReceived<T> where T : struct, ISpatialEvent
    {
    }

    public class AuthoritiesChanged<T> : MessagesReceived<Authority>
    {
    }
}