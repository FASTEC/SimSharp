using SimSharp.Core.Resources.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimSharp.Core.Resources.Events
{
    public class ResourceGroupRequest : Request
    {
        public IEnumerable<ResourceQuantity> RequestedResources { get; set; }
        public IEnumerable<Request> Requests { get; set; }

        public ResourceGroupRequest(Environment environment, Action<Event> callback, Action<Event> disposeCallback, IEnumerable<ResourceQuantity> requestedResources, double quantity = 1)
      : base(environment, callback, disposeCallback, quantity)
        {
            this.RequestedResources = requestedResources;
        }
    }
}
