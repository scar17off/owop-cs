using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OWOP_cs.OWOP.Instance
{
    internal class CPlayer
    {
        internal byte[] rgb;

        public int x { get; set; }
        public int y { get; set; }
        public int worldX { get; set; }
        public int worldY { get; set; }
        public int tool { get; set; }
        public Rank rank { get; set; }
        public int id { get; set; }
        public int[] color { get; set; } = new int[3];

        public Bucket bucket { get; private set; }

        public CPlayer()
        {
            color[0] = 0;
            color[1] = 0;
            color[2] = 0;
            rank = Rank.NONE;
            bucket = new Bucket(rate: 50, time: 2, infinite: false);
        }
    }
}