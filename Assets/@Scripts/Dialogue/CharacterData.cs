using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VN/Character Data")]
public class CharacterData : ScriptableObject
{
    public CharacterId characterId;
    public string characterName;

    [System.Serializable]
    public class ExpressionSprite
    {
        public CharacterState characterState;
        public Sprite sprite;
    }

    public ExpressionSprite[] expressions = CreateDefaultExpressions();

    private static ExpressionSprite[] CreateDefaultExpressions()
    {
        var states = (CharacterState[])System.Enum.GetValues(typeof(CharacterState));
        var arr = new ExpressionSprite[states.Length];
        for (int i = 0; i < states.Length; i++)
        {
            arr[i] = new ExpressionSprite();
            arr[i].characterState = states[i];
        }
        return arr;
    }
}
