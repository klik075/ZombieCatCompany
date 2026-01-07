using UnityEngine;

public class BaseScene : MonoBehaviour
{
    public Define.EScene SceneType { get; protected set; } = Define.EScene.Unknown;

    protected virtual void Awake()
    {
        // TODO
    }
}
