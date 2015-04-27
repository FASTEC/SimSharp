using SimSharp.Core.Resources.Events;
using SimSharp.Core.Resources.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimSharp.Core.Resources
{
    public class ResourceGroup : Resource
    {
        public List<Resource> Resources { get; private set; }

        public ResourceGroup(Environment env, double capacity = 1) : base(env, capacity)
        {
            this.Resources = new List<Resource>();
        }

        public ResourceGroupRequest RequestGroup(IEnumerable<ResourceQuantity> requestedResources, double quantity = 1)
        {
            if (this.Resources.Count == 0)
                throw new InvalidOperationException("No resources in ResourceGroup.");
            if (this.Resources.Count != requestedResources.Count())
                throw new InvalidOperationException("Not all resources requested.");
            ResourceGroupRequest request = new ResourceGroupRequest(Environment, TriggerRelease, base.DisposeCallback, requestedResources, quantity);
            RequestQueue.Enqueue(request);
            TriggerRequest();
            return request;
        }

        protected override void TriggerRequest(Event @event = null)
        {
            while (RequestQueue.Count > 0)
            {
                var request = RequestQueue.Peek();
                DoRequest(request as ResourceGroupRequest);
                if (request.IsTriggered)
                {
                    RequestQueue.Dequeue();
                }
                else break;
            }
        }

        protected void DoRequest(ResourceGroupRequest request)
        {
            var list = new List<Request>();
            bool available = true;
            RequestedResourceLoop((rr, resource) =>
            {
                if (resource.Remaining < rr.Quantity)
                    available = false;
            }, request);

            if (available)
            {
                RequestedResourceLoop((rr, resource) =>
                {
                    list.Add(resource.Request(rr.Quantity));
                }, request);
            }

            request.Requests = list;
            if (available)
                base.DoRequest(request);
        }

        private void RequestedResourceLoop(Action<ResourceQuantity, Resource> action, ResourceGroupRequest request)
        {
            foreach (var rr in request.RequestedResources)
            {
                var resource = this.Resources.FirstOrDefault(x => x.Id == rr.ResourceId);
                if (resource != null)
                {
                    action(rr, resource);
                }
            }
        }

        protected override void TriggerRelease(Event @event = null)
        {
            while (ReleaseQueue.Count > 0)
            {
                var release = ReleaseQueue.Peek();
                this.DoRelease(release);
                if (release.Request.IsTriggered)
                {
                    ReleaseQueue.Dequeue();
                }
                else break;
            }
        }

        protected void DoRelease(Release release)
        {
            ResourceGroupRequest req = release.Request as ResourceGroupRequest;
            foreach (var request in req.Requests)
            {
                request.Dispose();
            }
            base.DoRelease(release);
        }
    }
}
