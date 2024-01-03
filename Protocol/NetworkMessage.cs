using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

namespace CTC
{



    public class NetworkMessage
    {
        private int Index = 0, Size = 0;
        private byte[] Data = new byte[0x10000];
        private bool Raw = false;
        public int GetSize()
        {
            return Size;
        }

        public void SendMessage(Socket socket, string messageContent)
        {
            NetworkMessage message = new NetworkMessage();
            message.AddString(messageContent); // Add the string to the message

            try
            {
                message.WriteTo(socket); // Write the message to the socket
            }
            catch (SocketException ex)
            {
                Debug.WriteLine($"Socket exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
            }
        }

        public void ReceiveMessage(Socket socket)
        {
            NetworkMessage message = new NetworkMessage();

            try
            {
                if (message.ReadFrom(socket))
                {
                    // Process the message
                    string receivedContent = message.ReadString();
                    Debug.WriteLine($"Received: {receivedContent}");
                }
            }
            catch (SocketException ex)
            {
                Debug.WriteLine($"Socket exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
            }
        }


        public NetworkMessage(bool raw = false)
        {
            this.Raw = raw; // Now Raw is recognized in this context
        }

        public string Text {
            get { return ASCIIEncoding.ASCII.GetString(Data, 0, Size); }
        }

        public Boolean ReadFrom(Socket socket, Boolean block = true)
        {
            byte[] header = new byte[2];
            if (!block)
                if (socket.Available < 2)
                    return false;

            socket.Receive(header);
            Size = header[1] << 8 | header[0];

            Index = 0;
            socket.Receive(Data, Size, SocketFlags.None);

            return true;
        }

        public void WriteTo(Socket socket)
        {
            if (!Raw)
            {
                byte[] header = new byte[2];
                header[0] = (byte)((0x00FF & Index) >> 0);
                header[1] = (byte)((0xFF00 & Index) >> 8);
                socket.Send(header);
            }
            socket.Send(Data, Index, SocketFlags.None);
        }

        public Boolean ReadAllData()
        {
            return Index >= Size;
        }


        public byte[] GetData()
        {
            // Return a copy or a portion of the Data array
            byte[] data = new byte[Size];
            Array.Copy(Data, data, Size);
            return data;
        }

        public void SetData(byte[] data, int length)
        {
            // Ensure the data array is large enough to hold the incoming data
            if (length > Data.Length)
            {
                throw new InvalidOperationException("Buffer size exceeded.");
            }
            Debug.WriteLine("SetData (length)",length);
            // Copy the data into the NetworkMessage's buffer
            Array.Copy(data, 0, Data, 0, length);
            Size = length; // Update the size of the data
        }

        public byte ReadByte()
        {
            return Data[Index++];
        }

        public UInt16 ReadU16()
        {
            uint u = 0;
            u |= (uint)Data[Index++] << 0;
            u |= (uint)Data[Index++] << 8;
            return (UInt16)u;
        }

        public UInt32 ReadU32()
        {
            uint u = 0;
            u |= (uint)Data[Index++] << 0;
            u |= (uint)Data[Index++] << 8;
            u |= (uint)Data[Index++] << 16;
            u |= (uint)Data[Index++] << 24;
            return (UInt32)u;
        }

        public byte PeekByte()
        {
            return Data[Index];
        }

        public UInt16 PeekU16()
        {
            uint u = 0;
            u |= (uint)Data[Index  ] << 0;
            u |= (uint)Data[Index+1] << 8;
            return (UInt16)u;
        }

        public UInt32 PeekU32()
        {
            uint u = 0;
            u |= (uint)Data[Index  ] << 0;
            u |= (uint)Data[Index+1] << 8;
            u |= (uint)Data[Index+2] << 16;
            u |= (uint)Data[Index+3] << 24;
            return (UInt32)u;
        }

        public string ReadString()
        {
            int sz = ReadU16();
            String s = "" + Encoding.ASCII.GetString(Data, Index, sz);
            Index += sz;
            return s;
        }

        public void Skip(int n)
        {
            Index += n;
        }

        public void AddByte(int u8)
        {
            Data[Size++] = (byte)u8;
        }

        public void AddU16(UInt16 u16)
        {
            Data[Size++] = (byte)((u16 & 0x00FF) >> 0);
            Data[Size++] = (byte)((u16 & 0xFF00) >> 8);
        }

        public void AddU32(UInt32 u32)
        {
            Data[Size++] = (byte)((u32 & 0x000000FF) >> 0);
            Data[Size++] = (byte)((u32 & 0x0000FF00) >> 8);
            Data[Size++] = (byte)((u32 & 0x00FF0000) >> 16);
            Data[Size++] = (byte)((u32 & 0xFF000000) >> 24);
        }

        public void AddString(string s)
        {
            AddU16((UInt16)s.Length);
            foreach (byte c in s)
                AddByte(c);
        }


    }
}
