using UnityEngine;

[CreateAssetMenu(menuName = "YearEvent/EventRewardData")]
public class EventRewardData : ScriptableObject
{
    public string rewardName;
    public RewardType rewardType;
    public Sprite sprite;
}

public enum RewardType
{
    Gold,
    Food,
    Member
}

[System.Serializable]
public class EventReward
{
    public EventRewardData rewardData;
    public int amount;
}
