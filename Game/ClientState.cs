﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace CTC
{
    /// <summary>
    /// Holds all game data information for a single connection to a server
    /// or possibly a playing movie, either way this is usually passed to
    /// GameFrame.AddClient in order to create a new window displaying it.
    /// </summary>
    public class ClientState
    {
        public readonly ClientViewport Viewport;
        public readonly TibiaGameData GameData;
        public readonly TibiaGameProtocol Protocol;
        private readonly PacketStream InStream;
        UInt32 PlayerId = 1;
        public ClientState(PacketStream InStream)
        {
            this.InStream = InStream;
            FileStream datFile = new FileStream("./Tibia.dat", FileMode.Open);
            FileStream sprFile = new FileStream("./Tibia.spr", FileMode.Open);
            GameData = new TibiaGameData(datFile, sprFile);
            Protocol = new TibiaGameProtocol(GameData);
            Viewport = new ClientViewport(GameData, Protocol);

        }
        public void UpdateWithPlayerData(ClientPlayer playerData)
        {
            // Update ClientState properties based on playerData
            // This might involve updating Viewport, GameData, Protocol, etc.
            Viewport.Player = playerData;
            //HostName = "localhost";
        }
        public String HostName
        {
            get {
                return InStream.Name;
            }
            set
            {
                
            }
        }

        private void ReadPackets(GameTime Time)
        {
            while (InStream.Poll(Time))
            {
                try
                {
                    NetworkMessage nmsg = InStream.Read(Time);
                    if (nmsg == null)
                        return;
                    Protocol.parsePacket(nmsg);
                }
                catch (Exception ex)
                {
                    Log.Error("Protocol Error: " + ex.Message);
                }
            }
        }

        public void ForwardTo(TimeSpan Span)
        {
            if (!(InStream is TibiaMovieStream))
                throw new NotSupportedException("Can't fast-forward non-movie streams.");

            TibiaMovieStream Movie = (TibiaMovieStream)InStream;

            while (Movie.Elapsed.TotalSeconds < Span.TotalSeconds)
                Protocol.parsePacket(Movie.Read(null));
        }

        public void Update(GameTime Time)
        {
            ReadPackets(Time);
        }
    }
}
