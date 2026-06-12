using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using NeoFantasyOnline.Core.GlobalNPCs;

namespace NeoFantasyOnline.Core.Networking
{
    public enum NfoMessageType : byte
    {
        NpcDazed,
        NpcSlowed,
        ProjectileTimeLeft
    }

    public static class NfoNetHelper
    {
        public static void SendNpcDazed(int npcIndex, int duration)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                if (Main.npc[npcIndex].active)
                {
                    var global = Main.npc[npcIndex].GetGlobalNPC<NfoBuffNPC>();
                    if (global.Dazed < duration)
                        global.Dazed = duration;
                }
                return;
            }

            ModPacket packet = NeoFantasyOnline.Instance.GetPacket();
            packet.Write((byte)NfoMessageType.NpcDazed);
            packet.Write(npcIndex);
            packet.Write(duration);
            packet.Send();
        }

        public static void SendNpcSlowed(int npcIndex, int duration)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                if (Main.npc[npcIndex].active)
                {
                    var global = Main.npc[npcIndex].GetGlobalNPC<NfoBuffNPC>();
                    if (global.Slowed < duration)
                        global.Slowed = duration;
                }
                return;
            }

            ModPacket packet = NeoFantasyOnline.Instance.GetPacket();
            packet.Write((byte)NfoMessageType.NpcSlowed);
            packet.Write(npcIndex);
            packet.Write(duration);
            packet.Send();
        }

        public static void SendProjectileTimeLeft(int projIndex, int timeLeft)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                if (Main.projectile[projIndex].active)
                    Main.projectile[projIndex].timeLeft = timeLeft;
                return;
            }

            if (Main.projectile[projIndex].active)
                Main.projectile[projIndex].timeLeft = timeLeft;

            ModPacket packet = NeoFantasyOnline.Instance.GetPacket();
            packet.Write((byte)NfoMessageType.ProjectileTimeLeft);
            packet.Write(projIndex);
            packet.Write(timeLeft);
            packet.Send();
        }

        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            NfoMessageType msgType = (NfoMessageType)reader.ReadByte();

            switch (msgType)
            {
                case NfoMessageType.NpcDazed:
                    {
                        int npcIndex = reader.ReadInt32();
                        int dazed = reader.ReadInt32();
                        if (Main.netMode == NetmodeID.Server && Main.npc[npcIndex].active)
                        {
                            var global = Main.npc[npcIndex].GetGlobalNPC<NfoBuffNPC>();
                            if (global.Dazed < dazed)
                            {
                                global.Dazed = dazed;
                                Main.npc[npcIndex].netUpdate = true;
                            }
                        }
                        break;
                    }
                case NfoMessageType.NpcSlowed:
                    {
                        int npcIndex = reader.ReadInt32();
                        int slowed = reader.ReadInt32();
                        if (Main.netMode == NetmodeID.Server && Main.npc[npcIndex].active)
                        {
                            var global = Main.npc[npcIndex].GetGlobalNPC<NfoBuffNPC>();
                            if (global.Slowed < slowed)
                            {
                                global.Slowed = slowed;
                                Main.npc[npcIndex].netUpdate = true;
                            }
                        }
                        break;
                    }
                case NfoMessageType.ProjectileTimeLeft:
                    {
                        int projIndex = reader.ReadInt32();
                        int timeLeft = reader.ReadInt32();
                        if (Main.netMode == NetmodeID.Server && Main.projectile[projIndex].active)
                        {
                            Main.projectile[projIndex].timeLeft = timeLeft;
                            Main.projectile[projIndex].netUpdate = true;
                        }
                        break;
                    }
            }
        }
    }
}
