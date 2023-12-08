using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OWOP_cs.OWOP.Instance
{
    internal class PlayerUpdate
    {
        public int x { get; set; }
        public int y { get; set; }
        public byte[] rgb { get; set; }
        public byte tool { get; set; }
    }
}