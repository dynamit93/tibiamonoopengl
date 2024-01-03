using CTC;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                byte[] sizeBuffer = new byte[2]; // Buffer for the size (2 bytes)
                int bytesRead = _networkStream.Read(sizeBuffer, 0, sizeBuffer.Length);
                if (bytesRead != sizeBuffer.Length)
                    throw new Exception("Failed to read the size of the message.");

                // Convert the size to an integer
                ushort dataSize = BitConverter.ToUInt16(sizeBuffer, 0);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(sizeBuffer); // Convert from big-endian to little-endian if needed

                byte[] dataBuffer = new byte[dataSize];
                bytesRead = _networkStream.Read(dataBuffer, 0, dataSize);
                if (bytesRead != dataSize)
                    throw new Exception("Failed to read the full message.");

                NetworkMessage message = new NetworkMessage();
                message.SetData(dataBuffer, bytesRead);
                return message;
            }
            return null; // Return null if no data is available
        }




        public void Write(NetworkMessage nmsg)
        {
            byte[] data = nmsg.GetData(); // Get byte data from the message
            byte[] dataSize = BitConverter.GetBytes((ushort)data.Length); // Get the size of the data

            if (BitConverter.IsLittleEndian)
                Array.Reverse(dataSize); // Convert to big-endian if needed

            byte[] buffer = new byte[dataSize.Length + data.Length];
            Buffer.BlockCopy(dataSize, 0, buffer, 0, dataSize.Length); // Copy size to buffer
            Buffer.BlockCopy(data, 0, buffer, dataSize.Length, data.Length); // Copy data to buffer

            _networkStream.Write(buffer, 0, buffer.Length);
        }


        public string Name
        {
            get { return "TibiaServer"; } // Or any appropriate name
        }
    }

}
