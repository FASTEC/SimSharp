using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimSharp {
    public class RealTimeEnvironment : Environment {

        DateTime env_start, real_start;
        double factor;
        bool strict;

        public RealTimeEnvironment( DateTime initialTime, double factor = 1d, bool strict = true ) : base( initialTime ) {
            this.factor = factor;
            this.strict = strict;
            env_start = initialTime;
            real_start = DateTime.Now;
        }

        public void Sync() {
            real_start = DateTime.Now;
        }

        public override void Step() {
            var evt_time = Peek();
            var real_time = real_start.Add( evt_time.Subtract( env_start ) );
            var delta = real_time - DateTime.Now;

            if ( strict && ( DateTime.Now - real_time ).TotalSeconds > factor ) {
                throw new InvalidOperationException( string.Format( "Simulation too slow for real time ({0})", DateTime.Now - real_time ) );
            }

            if ( delta.TotalMilliseconds > 0 )
                System.Threading.Thread.Sleep( delta );
            base.Step();
        }
    }
}
