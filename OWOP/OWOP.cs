using OWOP_cs.OWOP.Instance;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace OWOP_cs.OWOP
{
    public enum Rank
    {
        NONE = 0,
        USER = 1,
        MODERATOR = 2,
        ADMIN = 3
    }

    public class Opcodes
    {
        public int SetId { get; } = 0;
        public int WorldUpdate { get; } = 1;
        public int ChunkLoad { get; } = 2;
        public int Teleport { get; } = 3;
        public int SetRank { get; } = 4;
        public int Captcha { get; } = 5;
        public int SetPQuota { get; } = 6;
        public int ChunkProtected { get; } = 7;
    }

    internal class Client
    {
        public ClientWebSocket ws { get; private set; } = new ClientWebSocket();

        public CPlayer Player = new CPlayer();

        private Dictionary<string, Action> eventHandlers = new Dictionary<string, Action>();
        public List<CPlayer> players = new List<CPlayer>();
        public ChunkSystem Chunks { get; private set; } = new ChunkSystem();

        public Opcodes Opcode { get; } = new Opcodes();
        public string World { get; private set; }

        private bool WorldNameSent = false;

        public Client(object options)
        {
            World = "main";

            if (options != null && options is IDictionary<string, object> dictionary)
            {
                if (dictionary.TryGetValue("world", out object world))
                {
                    World = world.ToString();
                }
            }
        }

        public async Task Connect(string serverUri)
        {
            try
            {
                await ws.ConnectAsync(new Uri(serverUri), CancellationToken.None);

                if (ws.State == WebSocketState.Open)
                {
                    await ListenForEvents();

                    while (ws.State == WebSocketState.Open)
                    {
                        await Task.Delay(1000);
                    }
                }
                else
                {
                    Console.WriteLine("WebSocket failed to connect");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket exception: {ex.Message}");
            }
        }

        public async Task<bool> MoveAsync(int x = 0, int y = 0)
        {
            if (ws.State != WebSocketState.Open)
            {
                return false;
            }

            Player.x = x;
            Player.y = y;

            x *= 16;
            y *= 16;

            byte[] buffer;
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                Player.worldX = x;
                Player.worldY = y;

                writer.Write(x);
                writer.Write(y);
                writer.Write((byte)Player.color[0]);
                writer.Write((byte)Player.color[1]);
                writer.Write((byte)Player.color[2]);
                writer.Write((byte)Player.tool);

                buffer = stream.ToArray();
            }

            await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);

            return true;
        }

        public async Task<bool> SetPixelAsync(int x = 0, int y = 0, int[] color = null, bool sneaky = false)
        {
            if (!Player.bucket.CanSpend(1))
            {
                return false;
            }

            int lX = Player.x, lY = Player.y;
            Player.x = x;
            Player.y = y;

            byte[] buffer;
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(x);
                writer.Write(y);
                writer.Write((byte)color[0]);
                writer.Write((byte)color[1]);
                writer.Write((byte)color[2]);
                Player.color = color;

                buffer = stream.ToArray();
            }

            if (ws.State == WebSocketState.Open && Player.rank != Rank.NONE)
            {
                await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);

                if (sneaky)
                {
                    await MoveAsync(lX, lY);
                }
                else
                    await MoveAsync(x, y);
            }

            return true;
        }

        public async Task Disconnect()
        {
            if (ws.State == WebSocketState.Open)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnect requested", CancellationToken.None);
            }
        }

        public void On(string eventName, Action handler)
        {
            eventHandlers[eventName] = handler;
        }

        private async Task ListenForEvents()
        {
            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    if(!WorldNameSent)
                    {
                        WorldNameSent = true;
                        World = World.ToLower();

                        byte[] ints = new byte[Math.Min(World.Length, 24)];
                        int index = 0;

                        for (int i = 0; i < ints.Length; i++)
                        {
                            int charCode = (int)World[i];
                            if ((charCode < 123 && charCode > 96) || (charCode < 58 && charCode > 47) || charCode == 95 || charCode == 46)
                            {
                                ints[index++] = (byte)charCode;
                            }
                        }

                        byte[] array = new byte[index + 2];
                        Buffer.BlockCopy(ints, 0, array, 0, index);
                        BitConverter.GetBytes((ushort)0x0).CopyTo(array, index);

                        await ws.SendAsync(new ArraySegment<byte>(array), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }

                    byte[] buffer = new byte[1024];
                    WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        switch (buffer[0])
                        {
                            case 0: // SetId
                                Player.id = (int)BitConverter.ToUInt32(buffer, 1);
                                break;
                            case 1: // worldUpdate
                                // Players
                                bool updated = false;
                                Dictionary<int, dynamic> updates = new Dictionary<int, dynamic>();
                                int offset = 2;
                                for (int i = buffer[1]; i > 0; i--)
                                {
                                    updated = true;
                                    int pid = (int)BitConverter.ToUInt32(buffer, offset);
                                    if (pid == Player.id) continue;
                                    int pmx = (int)BitConverter.ToUInt32(buffer, offset + 4);
                                    int pmy = (int)BitConverter.ToUInt32(buffer, offset + 8);
                                    Console.WriteLine(pid + " " + pmx + " " + pmy);
                                    int pr = buffer[offset + 12];
                                    int pg = buffer[offset + 13];
                                    int pb = buffer[offset + 14];
                                    int ptool = buffer[offset + 15];
                                    updates[pid] = new
                                    {
                                        x = pmx >> 4,
                                        y = pmy >> 4,
                                        rgb = new[] { pr, pg, pb },
                                        tool = ptool
                                    };
                                    offset += 16;
                                }
                                if (updated)
                                {
                                    foreach (var update in updates)
                                    {
                                        players[update.Key] = new CPlayer
                                        {
                                            id = update.Key,
                                            x = update.Value.x,
                                            y = update.Value.y,
                                            rgb = update.Value.rgb,
                                            tool = update.Value.tool
                                        };
                                    }
                                }
                                // Pixels
                                offset += buffer[offset] * 16 + 2;
                                int pixelCount = BitConverter.ToUInt16(buffer, offset);
                                offset += 2;
                                for (int j = 0; j < pixelCount; j++)
                                {
                                    int x = BitConverter.ToInt32(buffer, offset + j * 15 + 4);
                                    int y = BitConverter.ToInt32(buffer, offset + j * 15 + 8);
                                    int r = buffer[offset + j * 15 + 12];
                                    int g = buffer[offset + j * 15 + 13];
                                    int b = buffer[offset + j * 15 + 14];
                                    //Chunks.SetPixel(x, y, new[] { r, g, b });
                                }

                                // Disconnects
                                offset += pixelCount * 15 + 2;
                                int disconnectCount = buffer[offset];
                                offset += 1;
                                for (int k = 0; k < disconnectCount; k++)
                                {
                                    int dpid = (int)BitConverter.ToUInt32(buffer, offset + k * 4);
                                    if (players.Any(p => p.id == dpid))
                                    {
                                        players = players.Where(p => p.id != dpid).ToDictionary(p => p.id, p => p).Values.ToList();
                                    }
                                }
                                break;
                            case 4: // SetRank
                                Player.rank = (Rank)buffer[1];
                                break;
                            case 6:
                                Player.bucket.rate = buffer[1];
                                Player.bucket.time = buffer[3];
                                Player.bucket.CanSpend(0);
                                break;
                        };

                        if(buffer.Count() == 0) TriggerEvent("join");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket event listening exception: {ex.Message}");
            }
        }

        private void TriggerEvent(string eventName)
        {
            if (eventHandlers.TryGetValue(eventName, out Action handler))
            {
                handler.Invoke();
            }
        }
    }
}