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
            if (_networkStream == null)
                return false;
            // Implement polling logic for network data
            return _networkStream.DataAvailable;
        }

        public NetworkMessage Read(GameTime time)
        {
            if (_networkStream.DataAvailable)
            {
                byte[] sizeBuffer = new byte[2]; // Buffer for the size (2 bytes)
                int totalBytesRead = 0;

                // Read the size of the message
                while (totalBytesRead < sizeBuffer.Length)
                {
                    int bytesRead = _networkStream.Read(sizeBuffer, totalBytesRead, sizeBuffer.Length - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        throw new Exception("Connection closed while reading message size.");
                    }
                    totalBytesRead += bytesRead;
                }

                // Convert the size to an integer
                ushort dataSize = BitConverter.ToUInt16(sizeBuffer, 0);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(sizeBuffer); // Convert from big-endian to little-endian if needed

                byte[] dataBuffer = new byte[dataSize];
                totalBytesRead = 0;

                // Read the message data
                while (totalBytesRead < dataSize)
                {
                    int bytesRead = _networkStream.Read(dataBuffer, totalBytesRead, dataSize - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        if (_networkStream.Socket.Poll(1, SelectMode.SelectRead) && _networkStream.Socket.Available == 0)
                        {
                            // Connection has been closed gracefully
                            Debug.WriteLine("Connection closed gracefully by the server.");
                            break;
                        }
                        else
                        {
                            // Connection has been closed unexpectedly
                            throw new Exception("Connection closed unexpectedly while reading message data.");
                        }
                    }

                    totalBytesRead += bytesRead;
                    // Print the total bytes read
                    Debug.WriteLine($"Total bytes read: {totalBytesRead}");
                }

                // Print the total bytes read
                Debug.WriteLine($"Total bytes read: {totalBytesRead}");

                NetworkMessage message = new NetworkMessage();
                message.SetData(dataBuffer, totalBytesRead);
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
