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
        public static bool characterlist { get; set; }
        PacketStream packetStream;
        DebugManager debugManager = new DebugManager();
        public ClientState clientState;
        public TibiaGamePacketParserFactory tibiaGamePacketParserFactory;
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

            byte[] buffer = new byte[1024]; // Adjust buffer size based on expected message size
            StringBuilder jsonBuffer = new StringBuilder();

            try
            {
                while (IsConnected)
                {
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        // Append to JSON buffer
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Received Data: {receivedData}");
                        jsonBuffer.Append(receivedData);

                        // Process complete JSON messages
                        ProcessJsonBuffer(jsonBuffer, gameTime);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in data receiving loop: {ex.Message}");
                IsConnected = false;
                OnConnectionStatusChanged(false);
            }
        }

        private void ProcessJsonBuffer(StringBuilder jsonBuffer, GameTime gameTime)
        {
            string bufferString = jsonBuffer.ToString();
            int jsonStartPosition = FindJsonStartPosition(bufferString);

            if (jsonStartPosition != -1)
            {
                string json = bufferString.Substring(jsonStartPosition);
                jsonBuffer.Clear().Append(json); // Reset buffer with cleaned JSON
            }

            int jsonEndPosition = FindJsonEndPosition(jsonBuffer.ToString());

            if (jsonEndPosition != -1)
            {
                string completeJson = jsonBuffer.ToString().Substring(0, jsonEndPosition + 1);
                jsonBuffer.Remove(0, jsonEndPosition + 1); // Remove processed JSON

                try
                {
                    ProcessJson(completeJson, gameTime); // Deserialize and process JSON
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"JSON Deserialization error: {ex.Message}");
                    // Handle incomplete or malformed JSON object
                }
            }
        }
        private int FindJsonStartPosition(string buffer)
        {
            // Assuming JSON string starts after the first occurrence of '{'
            return buffer.IndexOf('{');
        }

        private int FindJsonEndPosition(string buffer)
        {
            int depth = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                char c = buffer[i];
                if (c == '{') depth++;
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0) return i; // End of JSON object
                }
            }
            return -1; // No complete JSON object found
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
        private void ReceiveDataFromServer(byte[] data, int size, GameTime gameTime)
        {
            string receivedPart = Encoding.UTF8.GetString(data, 0, size);
            jsonBuffer.Append(receivedPart);

            if (IsCompleteJson(jsonBuffer.ToString()))
            {
                string completeJson = jsonBuffer.ToString();
                jsonBuffer.Clear(); // Clear the buffer for the next message

                ProcessJson(completeJson, gameTime); // Process the complete JSON
            }
        }
        private bool IsCompleteJson(string json)
        {
            // Implement logic to check if the JSON string is complete
            // This could be as simple as checking for a closing brace '}' for simple cases,
            // or more complex JSON structure validation for advanced scenarios.
            return json.EndsWith("}");
        }


        private void ProcessJson(string json, GameTime gameTime)
        {
            try
            {
                Debug.WriteLine("Received JSON: " + json); // Log the complete JSON

                if (!IsValidJson(json))
                {
                    throw new InvalidOperationException("Invalid JSON data.");
                }

                dynamic baseObject = JsonConvert.DeserializeObject<dynamic>(json);
                Console.WriteLine(baseObject);
                if (baseObject["player"] != null)
                {
                    JObject playerObject = baseObject["player"] as JObject;
                    HandlePlayerLogin(playerObject);
                }
                else if ((string)baseObject["Type"] != null)
                {
                    string messageType = (string)baseObject["Type"];
                    switch (messageType)
                    {
                        case "MapDescription":
                            // Antag att du vill hantera kartbeskrivningen här
                            HandleMapDescription(baseObject, gameTime);
                            break;
                        case "Heartbeat":
                            HandleHeartbeat(baseObject);
                            break;
                        default:
                            Debug.WriteLine("Unknown data type received.");
                            break;
                    }
                }
                else
                {
                    Debug.WriteLine("No recognizable type found in JSON.");
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON Deserialization error: {ex.Message}");
            }
        }

        private void HandlePlayerLogin(JObject playerObject)
        {
            // Antag att ClientPlayer har en struktur som matchar 'player' delen av JSON.
            ClientPlayer player = playerObject.ToObject<ClientPlayer>();

            // Fortsätt med att hantera 'player' objektet som tidigare
            string filepath = "C:\\Users\\dennis\\source\\repos\\tibiamonoopengl\\json\\MapDescription.json";
            


            clientState = new ClientState(packetStream, filepath);
            clientState.UpdateWithPlayerData(player);
            HandleSuccessfulLogin();

            characterlist = true;
        }

        private void HandleMapDescription(dynamic baseObject, GameTime gameTime)
        {
            try
            {
                Task.Run(() => StartReceivingDataAsync(gameTime));
                var mapDescription = JsonConvert.DeserializeObject<ClientMap>(baseObject);
                clientState.Update(gameTime);

                string filepath = "C:\\Users\\dennis\\source\\repos\\tibiamonoopengl\\json\\MapDescription.json";
                ////MapDescription.LoadMapDescription(filepath);
                tibiaGamePacketParserFactory.LoadMapDescription(filepath);
                Console.WriteLine("sdfsdf");
                // var mapDescription = JsonConvert.DeserializeObject<ClientMap>(json);


                //// ProcessMapDescription(mapDescription);
                // clientState.Viewport.Map = mapDescription;

                //UpdateClientMap(mapDescription);



            }
            catch (JsonException jsonEx)
            {
                // Log the exception message and stack trace to see what went wrong
                Console.WriteLine("JsonException: " + jsonEx.Message);
                Console.WriteLine("StackTrace: " + jsonEx.StackTrace);
            }
            catch (Exception ex)
            {
                // Log any other exceptions that may occur during deserialization
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
            }
        }

        private void HandleHeartbeat(dynamic baseObject)
        {
            ReceiveHeartbeatFromServer(networkStream);
        }

        public void ReceiveHeartbeatFromServer(NetworkStream networkStream)
        {
            try
            {
                byte[] buffer = new byte[1024]; // Adjust the buffer size as needed
                int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                string jsonPacket = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var heartbeatPacket = JsonConvert.DeserializeObject<HeartbeatPacket>(jsonPacket);

                // Process the received heartbeat data here
                // You can access heartbeatPacket.Type and heartbeatPacket.Timestamp
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        //private void ProcessMapDescription(MapDescription mapDescription)
        //{
        //    foreach (var tile in mapDescription.Tiles)
        //    {
        //        var clientTile = new ClientTile(new MapPosition(tile.Location.X, tile.Location.Y, tile.Location.Z));


        //        foreach (var item in tile.Items)
        //        {
        //            //var itemType = GetItemTypeById(item.ID);
        //            //var clientItem = new ClientItem(itemType, item.Subtype);
        //            clientTile.Add(clientItem);
        //        }

        //        // Add the populated clientTile to your map
        //        // This might involve updating a dictionary or list that represents the game map
        //    }
        //}


        //public ItemType GetItemTypeById(int id)
        //{
        //    ItemType itemType;
        //    if (Items.TryGetValue(id, out itemType))
        //    {
        //        return itemType;
        //    }
        //    return null; // or ItemType.NullType if you want to return a default value instead of null
        //}


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

        private NetworkMessage ConvertMapDataToNetworkMessage(string mapDataJson)
        {
            debugManager.LogMessage(Log.Level.Debug, mapDataJson);

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
