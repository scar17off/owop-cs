using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OWOP_cs.OWOP.Instance
{
    internal class ChunkSystem
    {
        /*
        private readonly Dictionary<int, Dictionary<int, byte[]>> chunks = new Dictionary<int, Dictionary<int, byte[]>>();
        private readonly Dictionary<int, Dictionary<int, bool>> chunkProtected = new Dictionary<int, Dictionary<int, bool>>();

        internal byte[] SetChunk(int x, int y, byte[] data)
        {
            if (data == null || !IsValidCoordinate(x, y)) return null;
            if (!chunks.ContainsKey(x)) chunks[x] = new Dictionary<int, byte[]>();
            chunks[x][y] = data;
            return data;
        }

        internal byte[] GetChunk(int x, int y, bool raw)
        {
            if (!raw)
            {
                x = x / 16;
                y = y / 16;
            }
            if (!chunks.ContainsKey(x)) return null;
            return chunks[x].GetValueOrDefault(y);
        }

        internal bool RemoveChunk(int x, int y)
        {
            if (!chunks.ContainsKey(x)) return false;
            return chunks[x].Remove(y);
        }

        internal bool SetPixel(int x, int y, int[] rgb)
        {
            if (rgb == null || !IsValidCoordinate(x, y)) return false;
            int chunkX = x / 16;
            int chunkY = y / 16;

            if (!chunks.ContainsKey(chunkX)) return false;

            byte[] chunk = chunks[chunkX].GetValueOrDefault(chunkY);
            if (chunk == null) return false;
            int i = GetIndexByXY(x & (16 - 1), y & (16 - 1), 16);

            chunk[i] = (byte)rgb[0];
            chunk[i + 1] = (byte)rgb[1];
            chunk[i + 2] = (byte)rgb[2];
            return true;
        }

        internal int[] GetPixel(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return null;
            int chunkX = x / 16;
            int chunkY = y / 16;

            if (!chunks.ContainsKey(chunkX)) return null;
            byte[] chunk = chunks[chunkX].GetValueOrDefault(chunkY);
            if (chunk == null) return null;
            int i = GetIndexByXY(x & (16 - 1), y & (16 - 1), 16);
            return new[] { chunk[i], chunk[i + 1], chunk[i + 2] };
        }

        internal bool ProtectChunk(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return false;
            if (!chunkProtected.ContainsKey(x)) chunkProtected[x] = new Dictionary<int, bool>();
            chunkProtected[x][y] = true;
            return true;
        }

        internal bool UnProtectChunk(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return false;
            if (!chunkProtected.ContainsKey(x)) return false;
            chunkProtected[x][y] = false;
            return true;
        }

        internal bool IsProtected(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return false;
            if (!chunkProtected.ContainsKey(x)) return false;
            return chunkProtected[x].GetValueOrDefault(y);
        }

        private bool IsValidCoordinate(int x, int y) => x >= 0 && y >= 0;

        private int GetIndexByXY(int x, int y, int w) => (y * w + x) * 3;
        */
    }
}
