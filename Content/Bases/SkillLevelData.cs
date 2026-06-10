namespace NeoFantasyOnline.Content.Bases
{
    public struct SkillLevelData
    {
        /// <summary>使用间隔（帧）</summary>
        public int FireCD;
        /// <summary>弹幕数量</summary>
        public int Count;
        /// <summary>基础伤害</summary>
        public int Damage;
        /// <summary>速度倍率</summary>
        public float Speed;
        /// <summary>大小倍率</summary>
        public float Size;
        /// <summary>穿透次数（-1 = 无限）</summary>
        public int HitTimes;

        public SkillLevelData(int fireCD, int count, int damage, float speed, float size, int hitTimes)
        {
            FireCD = fireCD;
            Count = count;
            Damage = damage;
            Speed = speed;
            Size = size;
            HitTimes = hitTimes;
        }
    }
}
