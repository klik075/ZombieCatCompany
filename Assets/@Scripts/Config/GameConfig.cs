using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Config/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Game Settings")]
    
    [Min(500)]
    [SerializeField]
    private int initialGold = 1000;

    [Range(1, 20)]
    [SerializeField]
    private int initialLevel = 1;

    public int InitialGold => initialGold;
    public int InitialLevel => initialLevel;
}
