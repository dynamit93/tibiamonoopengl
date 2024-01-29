using System;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using tibiamonoopengl.Protocol;
using tibiamonoopengl.Rsa;
using Microsoft.Xna.Framework;
using CTC;
using tibiamonoopengl.UI.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using System.Numerics;
using Newtonsoft.Json.Linq;
namespace tibiamonoopengl.Protocol
{




    public class NetworkManager
    {
        private GameDesktop gameDesktop;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        //private RsaDecryptor rsaDecryptor;
        public bool IsConnected { get; set; }
        public event EventHandler<bool> ConnectionStatusChanged;
        private LoginWindow loginWindow;
        private ClientViewport clientViewport;
        public static bool characterlist {get; set;}
        PacketStream packetStream;
        DebugManager debugManager = new DebugManager();
        public ClientState clientState;
        public NetworkManager(ClientViewport viewport, LoginWindow loginWindow, GameDesktop gameDesktop)
        {
            this.clientViewport = viewport;
            this.loginWindow = loginWindow;
            this.gameDesktop = gameDesktop;
            
            // gameDesktop = new GameDesktop();
             characterlist = false;
            //rsaDecryptor = new RsaDecryptor("path/to/key.pem");


        }



        private bool isConnecting = false;
        

        private void HandleSuccessfulLogin()
        {
            // Use the ClientPlayer data to initialize or update ClientState
            // For example:
            //clientState = new ClientState(packetStream); // Initialize with necessary data
           // clientState = new ClientState(packetStream);
        //clientState.UpdateWithPlayerData(player); // Update ClientState with player data

            // Add client to GameDesktop
            gameDesktop.AddClient(clientState);
        }
        public async Task ConnectToServerAsync(string serverAddress, int port, GameTime gameTime)
        {
            if (isConnecting) return;
       
            isConnecting = true;
            tcpClient = new TcpClient();

            try
            {
                // First, connect to the server
                await tcpClient.ConnectAsync(serverAddress, port);

                // After successfully connecting, get the network stream
                networkStream = tcpClient.GetStream();

                // Now that networkStream is not null, initialize TibiaNetworkStream
                packetStream = new TibiaNetworkStream(networkStream);

                IsConnected = true;
                OnConnectionStatusChanged(true);

                // Send the initial authentication token or any other setup data
                string authToken = "ExpectedAuthToken";
                byte[] authBytes = Encoding.UTF8.GetBytes(authToken);
                await networkStream.WriteAsync(authBytes, 0, authBytes.Length);

                // Start receiving data in a separate task
                Task.Run(() => StartReceivingDataAsync(gameTime));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error connecting to server: {ex.Message}");
                IsConnected = false;
                OnConnectionStatusChanged(false);
            }
            finally
            {
                OnConnectionStatusChanged(false);
                isConnecting = false;
            }
        }


        public async Task StartReceivingDataAsync(GameTime gameTime)
        {

            if (!IsConnected)
            {
                Debug.WriteLine("Cannot start receiving data: not connected to the server.");
                return;
            }

            // Use TibiaNetworkStream for handling network operations
            TibiaNetworkStream tibiaNetworkStream = new TibiaNetworkStream(networkStream);

            while (IsConnected)
            {
                try
                {
                    // Poll for new data
                    if (tibiaNetworkStream.Poll(gameTime))
                    {
                        // Read the incoming message
                        NetworkMessage message = tibiaNetworkStream.Read(gameTime);
                       
                        if (message != null && message.Text != "")
                        {
                            // Process the received data
                            ReceiveDataFromServer(message.GetData(), message.GetSize());
                        }
                    }
                    else
                    {
                        Debug.WriteLine("No data available to read.");
                    }
                    await Task.Delay(100); // Delay to prevent tight loop; adjust as needed
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception in data receiving loop: {ex.Message}");
                    if (ex.Message.Contains("Connection closed"))
                    {
                        Debug.WriteLine("Connection to server lost.");
                        IsConnected = false;
                        OnConnectionStatusChanged(false);
                        break;
                    }
                }
            }

        }




        //public void UpdateClientStateWithPlayerData(ClientPlayer playerData, ClientState clientState)
        //{
            
        //    // Assuming you have access to an instance of ClientState
        //  //  clientState.UpdateWithPlayerData(playerData);
        //    gameDesktop.AddClient(clientState);
        //}




        //private void ProcessReceivedData(byte[] data, int bytesRead)
        //{
        //    string receivedString = Encoding.UTF8.GetString(data, 0, bytesRead);
        //    Debug.WriteLine($"Received data: {receivedString}");

        //    try
        //    {
        //        // Assuming the JSON structure is {"player": { ... }}
        //        var container = JsonConvert.DeserializeObject<Dictionary<string, ClientPlayer>>(receivedString);
        //        ClientPlayer player = container["player"];
        //        // Process the player data
        //    }
        //    catch (JsonException ex)
        //    {
        //        Debug.WriteLine($"JSON Deserialization error: {ex.Message}");
        //    }
        //}
        private StringBuilder jsonBuffer = new StringBuilder();
        private void ReceiveDataFromServer(byte[] data, int size)
        {
            string receivedPart = Encoding.UTF8.GetString(data, 0, size);
            jsonBuffer.Append(receivedPart);

            if (IsCompleteJson(jsonBuffer.ToString()))
            {
                string completeJson = jsonBuffer.ToString();
                jsonBuffer.Clear(); // Clear the buffer for the next message

                ProcessJson(completeJson); // Process the complete JSON
            }
        }
        private bool IsCompleteJson(string json)
        {
            // Implement logic to check if the JSON string is complete
            // This could be as simple as checking for a closing brace '}' for simple cases,
            // or more complex JSON structure validation for advanced scenarios.
            return json.EndsWith("}");
        }


        private void ProcessJson(string json)
        {
            try
            {
                Debug.WriteLine("Received JSON: " + json); // Log the complete JSON

                if (!IsValidJson(json))
                {
                    throw new InvalidOperationException("Invalid JSON data.");
                }

                dynamic baseObject = JsonConvert.DeserializeObject<dynamic>(json);

                if (baseObject.Property("player") != null)
                {
                    var playerData = JsonConvert.DeserializeObject<Dictionary<string, ClientPlayer>>(json);
                    ClientPlayer player = playerData["player"];
                     clientState = new ClientState(packetStream);
                    clientState.UpdateWithPlayerData(player);
                    HandleSuccessfulLogin();
                    //UpdateClientStateWithPlayerData(player, clientState);

                    characterlist = true;
                }
                else if (baseObject.Property("MapData") != null)
                {
                    var mapData = JsonConvert.DeserializeObject<Dictionary<string, ClientMap>>(json);
                    var mapDataJson = mapData["MapData"].ToString();
                    // Convert the map data into a NetworkMessage format
                    NetworkMessage mapMessage = ConvertMapDataToNetworkMessage(mapDataJson);
                }
                else
                {
                    Debug.WriteLine("Unknown data type received.");
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON Deserialization error: {ex.Message}");
            }
        }
        private bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || // For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) // For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    Debug.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //// Example client-side method to receive data
        //private void ReceiveDataFromServera(byte[] data, int size)
        //{
        //    // Convert the received byte array to a string
        //    string receivedJson = Encoding.UTF8.GetString(data, 0, size);
        //    receivedJson = receivedJson.TrimStart('\0');
        //    debugManager.LogMessage(Log.Level.Debug, receivedJson);
        //    Debug.WriteLine($"Received JSON: {receivedJson}");
            
        //    try
        //    {
        //        clientState = new ClientState(packetStream);
        //        //var playerData = JsonConvert.DeserializeObject<ClientPlayer>(receivedJson);
        //        // Deserialize the JSON string to the appropriate object
        //        // Deserialize to a base type or a dynamic object to inspect the "Type" property
        //        var baseObject = JsonConvert.DeserializeObject<dynamic>(receivedJson);

        //        if (baseObject.Type == "PlayerData")
        //        {
        //            var playerData = JsonConvert.DeserializeObject<Dictionary<string, ClientPlayer>>(receivedJson);
        //            if (playerData != null && playerData.ContainsKey("player"))
        //            {
        //                ClientPlayer player = playerData["player"];
        //                HandleSuccessfulLogin();
        //                // Pass the deserialized data to ClientState for further processing
        //                UpdateClientStateWithPlayerData(player, clientState);

        //                characterlist = true;
        //                // Process the player data
        //                // Here you can update the UI or game state based on the received player data
        //                // ...
        //                //UpdatePlayerState(player);
        //                // HandleSuccessfulLogin();

        //            }
        //            else
        //            {

        //                Debug.WriteLine("Deserialization returned null or missing 'player' key.");
        //            }
        //        }
        //        else if (baseObject.Type == "MapData")
        //        {
        //            var mapData = JsonConvert.DeserializeObject<Dictionary<string, ClientMap>>(receivedJson);

        //            if (mapData != null && mapData.ContainsKey("MapData"))
        //            {
        //                // Extract the map data from the JSON
        //                var mapDataJson = mapData["MapData"].ToString();
        //                // Convert the map data into a NetworkMessage format
        //                NetworkMessage mapMessage = ConvertMapDataToNetworkMessage(mapDataJson);

        //                // Get the MapDescription packet parser
        //                //var mapDescriptionParser = Protocol.Factory.CreatePacketHandler("MapDescription");
        //                // Parse the map data
        //                //Packet mapPacket = mapDescriptionParser.parser(mapMessage);
        //            }
        //        }
        //        else
        //        {
        //            Debug.WriteLine("Unknown data type received.");
        //        }




        //        }
        //    catch (JsonException ex)
        //    {
        //        Debug.WriteLine($"JSON Deserialization error: {ex.Message}");
                
        //        Console.WriteLine($"JSON Deserialization error: {ex.Message}");

        //        debugManager.LogMessage(Log.Level.Error, ex.Message);
        //    }
        //}

        // Method to convert JSON map data to NetworkMessage format
        private NetworkMessage ConvertMapDataToNetworkMessage(string mapDataJson)
        {
            debugManager.LogMessage(Log.Level.Debug, mapDataJson);
            // Implement the logic to convert the JSON map data into the format expected by NetworkMessage
            // This might involve parsing the JSON to extract tile information, creatures, items, etc.
            // and then encoding this information in a byte array that NetworkMessage can understand.
            // ...

            // Assuming you have access to an instance of ClientState
            bool encodedMapData = true; 

                //MapPosition = MapData;


            return new NetworkMessage(encodedMapData);
        }

        private void UpdatePlayerState(ClientPlayer player)
        {
            // Assuming 'clientViewport' is an instance of ClientViewport
            // and it's accessible in this context

            // Update the player's state in the viewport
           clientViewport.Player = player;

            //// Update other game states or UI elements as needed
            //RefreshUI();
            //UpdateMap();
            //// ... other update methods ...

            //// Trigger any events or notifications necessary
            //OnPlayerStateUpdated(); // This is a hypothetical event handler
        }


        public void SetClientViewport(ClientViewport viewport)
        {
            this.clientViewport = viewport;
        }
        private bool loginRequestSent = false;
        public async Task SendLoginRequestAsync(string serverAddress, int port, string username, string password, GameTime gameTime)
        {
            if (!IsConnected)
            {
                await ConnectToServerAsync(serverAddress, port, gameTime);
            }

            if (IsConnected && !loginRequestSent)
            {
                string loginMessage = $"LOGIN {username} {password}";
                byte[] loginBytes = Encoding.UTF8.GetBytes(loginMessage);
                await networkStream.WriteAsync(loginBytes, 0, loginBytes.Length);
                loginRequestSent = true;
            }
        }



        public void Cleanup()
        {
            if (networkStream != null)
                networkStream.Close();
            if (tcpClient != null)
                tcpClient.Close();
        }

        protected virtual void OnConnectionStatusChanged(bool isConnected)
        {
            ConnectionStatusChanged?.Invoke(this, isConnected);
        }
        // Other methods like ProcessReceivedData, etc.
    }
}
