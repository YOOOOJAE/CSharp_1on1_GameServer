using System;
using System.Collections.Generic;
using System.Text;

namespace RPGCommon
{
    public class MapManager
    {
        //게임과 서버에서 움직임을 동기화하기 위해 맵 장애물을 읽는코드
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool[,] _collision;
        
        public void LoadMap(string file)
        {
            string[] lines = file.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            string[] size = lines[0].Split(',');
            Width = int.Parse(size[0]);
            Height = int.Parse(size[1]);
            _collision = new bool[Width, Height];

            for (int y = 0; y < Height; y++)
            {
                string line = lines[y + 1];
                for (int x = 0; x < Width; x++)
                {
                    _collision[x, (Height - 1) - y] = (line[x] == '1');
                }
            }
        }

        public bool CanMove(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
            return !_collision[x, y];
        }
    }
}
