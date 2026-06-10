using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace NeoFantasyOnline
{
	public class NeoFantasyOnline : Mod
	{
		/// <summary>获取音乐资源，直接输入文件名不需要带后缀，也不需要带路径</summary>
		public static int GetMusic(string name)
		{
			return MusicLoader.GetMusicSlot($"NeoFantasyOnline/Assets/Music/{name}");
        }
	}
}
