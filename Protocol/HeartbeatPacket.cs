using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tibiamonoopengl.Protocol
{
    public class HeartbeatPacket
    {
        public string Type { get; set; }
        public string Timestamp { get; set; }
    }

}
