using UnityEngine;
using static Define;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Config/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Game Settings")]
    
    [Min(500)]
    [SerializeField]
    private int initialGold = 5000;

    [Min(1)]
    [SerializeField]
    private int initialYear = 1;

    [Min(0)]
    [SerializeField]
    private int initialFood = 100;

    [SerializeField]
    private EGameMode initialGameMode = EGameMode.Purchase;

    public int InitialGold => initialGold;
    public int InitialYear => initialYear;
    public int InitialFood => initialFood;
    public EGameMode InitialGameMode => initialGameMode;
}
