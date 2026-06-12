using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NeoFantasyOnline.Core.GlobalNPCs
{
    public class NfoBuffNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int Dazed = 0;
        public int Slowed = 0;

        public override void PostAI(NPC npc)
        {
            if (Dazed > 0)
            {
                Dazed--;
                npc.velocity.X = 0f;
                npc.velocity.Y = 0f;
            }

            if (Slowed > 0)
            {
                Slowed--;
                npc.velocity.X *= 0.5f;
                npc.velocity.Y *= 0.5f;
            }
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Dazed);
            binaryWriter.Write(Slowed);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            Dazed = binaryReader.ReadInt32();
            Slowed = binaryReader.ReadInt32();
        }
    }
}
