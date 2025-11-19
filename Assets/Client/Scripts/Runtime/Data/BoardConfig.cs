using UnityEngine;

namespace MiniIT
{
    [CreateAssetMenu(menuName = "Match3/BoardConfig", fileName = "BoardConfig")]
    public class BoardConfig : ScriptableObject
    {
        [Header("Grid")]
        [field: SerializeField] public int BoardWidth { get; set; }
        [field: SerializeField] public int BoardHeight { get; set; }
        [field: SerializeField] public float CellSize { get; set; }
        
        [Header("Tile")] 
        [field: SerializeField] public Sprite[] TileSprites { get; set; }
        [field: SerializeField] public int ColorsCount { get; set; }
        
        [Header("Gameplay")]
        [field: SerializeField] public float SwapDuration{ get; set; }

        [field: SerializeField] public float FallDuration { get; set; }
    }
}