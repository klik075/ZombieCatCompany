using UnityEngine;

[CreateAssetMenu(fileName = "GameDevQualityRuleConfig", menuName = "Config/GameDevQualityRuleConfig")]
public class GameDevQualityRuleConfig : ScriptableObject
{
    [Header("Pick Chance")]
    [Range(0f, 1f)]
    [SerializeField] private float mainQualityPickChance = 0.7f;

    [Header("Score Weights (sum should be 1.0)")]
    [Min(0f)]
    [SerializeField] private float funWeight = 0.2f;

    [Min(0f)]
    [SerializeField] private float mainWeight = 0.7f;

    [Min(0f)]
    [SerializeField] private float subWeight1 = 0.05f;

    [Min(0f)]
    [SerializeField] private float subWeight2 = 0.05f;

    [Header("Try Count")]
    [Min(1)]
    [SerializeField] private int minTryBase = 1;

    [Min(1)]
    [SerializeField] private int minTryDiv = 5;

    [Min(1)]
    [SerializeField] private int offsetDiv = 20;

    public float MainQualityPickChance => mainQualityPickChance;

    public float FunWeight => funWeight;
    public float MainWeight => mainWeight;
    public float SubWeight1 => subWeight1;
    public float SubWeight2 => subWeight2;

    public int MinTryBase => minTryBase;
    public int MinTryDiv => minTryDiv;
    public int OffsetDiv => offsetDiv;
}
