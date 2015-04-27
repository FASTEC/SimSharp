#region License Information
/* SimSharp - A .NET port of SimPy, discrete event simulation framework
Copyright (C) 2014  Heuristic and Evolutionary Algorithms Laboratory (HEAL)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace SimSharp
{
    public class Resource
    {
        public Guid Id { get; private set; }
        public double Capacity { get; protected set; }

        public double InUse { get; private set; }

        public double Remaining { get { return Capacity - InUse; } }

        protected Environment Environment { get; private set; }

        protected Queue<Request> RequestQueue { get; private set; }
        protected Queue<Release> ReleaseQueue { get; private set; }
        protected HashSet<Request> Users { get; private set; }

        public Resource(Environment environment, double capacity = 1)
        {
            Id = Guid.NewGuid();
            if (capacity <= 0) throw new ArgumentException("Capacity must > 0.", "capacity");
            Environment = environment;
            Capacity = capacity;
            RequestQueue = new Queue<Request>();
            ReleaseQueue = new Queue<Release>();
            Users = new HashSet<Request>();
        }

        public virtual Request Request(double quantity = 1)
        {
            var request = new Request(Environment, TriggerRelease, DisposeCallback, quantity);
            RequestQueue.Enqueue(request);
            TriggerRequest();
            return request;
        }

        public virtual Release Release(Request request)
        {
            var release = new Release(Environment, request, TriggerRequest);
            ReleaseQueue.Enqueue(release);
            TriggerRelease();
            return release;
        }

        protected virtual void DisposeCallback(Event @event)
        {
            var request = @event as Request;
            if (request != null) Release(request);
        }

        protected virtual void DoRequest(Request request)
        {
            if (request.Quantity <= Remaining)
            {
                InUse += request.Quantity;
                Users.Add(request);
                request.Succeed();
            }
        }

        protected virtual void DoRelease(Release release)
        {
            Users.Remove(release.Request);
            InUse -= release.Request.Quantity;
            release.Succeed();
        }

        protected virtual void TriggerRequest(Event @event = null)
        {
            while (RequestQueue.Count > 0)
            {
                var request = RequestQueue.Peek();
                DoRequest(request);
                if (request.IsTriggered)
                {
                    RequestQueue.Dequeue();
                }
                else break;
            }
        }

        protected virtual void TriggerRelease(Event @event = null)
        {
            while (ReleaseQueue.Count > 0)
            {
                var release = ReleaseQueue.Peek();
                DoRelease(release);
                if (release.IsTriggered)
                {
                    ReleaseQueue.Dequeue();
                }
                else break;
            }
        }
    }
}
