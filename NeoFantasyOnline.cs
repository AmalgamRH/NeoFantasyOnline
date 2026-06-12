using System.IO;
using Terraria.ModLoader;
using NeoFantasyOnline.Core.Networking;

namespace NeoFantasyOnline
{
	public class NeoFantasyOnline : Mod
	{
		public static NeoFantasyOnline Instance => ModContent.GetInstance<NeoFantasyOnline>();

		/// <summary>获取音乐资源，直接输入文件名不需要带后缀，也不需要带路径</summary>
		public static int GetMusic(string name)
		{
			return MusicLoader.GetMusicSlot($"NeoFantasyOnline/Assets/Music/{name}");
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			NfoNetHelper.HandlePacket(reader, whoAmI);
		}
	}
}
