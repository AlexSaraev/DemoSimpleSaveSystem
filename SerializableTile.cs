using UnityEngine;
using Tiles;

namespace Saving
{
    [System.Serializable]
    public class SerializableTile
    {
        float x, y;

        public SerializableTile(Tile tile)
        {
            x = tile.transform.position.x;
            y = tile.transform.position.y;   
        }

        public Tile ToTile()
        {
            var coordinates = new Vector2(x, y);
            return Tile.GetTile(coordinates);
        }
    }
}