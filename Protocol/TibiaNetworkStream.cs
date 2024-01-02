using CTC;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace tibiamonoopengl.Protocol
{
    public class TibiaNetworkStream : PacketStream
    {
        private NetworkStream _networkStream;

        public TibiaNetworkStream(NetworkStream networkStream)
        {
            _networkStream = networkStream;
        }

        public bool Poll(GameTime time)
        {
            // Implement polling logic for network data
            return _networkStream.DataAvailable;
        }

        public NetworkMessage Read(GameTime time)
        {
            if (_networkStream.DataAvailable)
            {
                byte[] buffer = new byte[1024]; // Adjust buffer size as needed
                int bytesRead = _networkStream.Read(buffer, 0, buffer.Length);

                NetworkMessage message = new NetworkMessage();
                // Assuming NetworkMessage has methods to set its data
                message.SetData(buffer, bytesRead); // Set the data for the message
                return message;
            }
            return null; // Return null if no data is available
        }



        public void Write(NetworkMessage nmsg)
        {
            // Basic implementation: Write data to the network stream
            // This is a placeholder implementation and should be replaced with actual logic
            byte[] data = nmsg.GetData(); // Assuming NetworkMessage has a method to get byte data
            _networkStream.Write(data, 0, data.Length);
        }

        public string Name
        {
            get { return "TibiaServer"; } // Or any appropriate name
        }
    }

}
