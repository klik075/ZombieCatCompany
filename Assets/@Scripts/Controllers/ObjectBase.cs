using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    public bool Pooling { get; set; } = false;

    protected virtual void Awake()
    {
        Init();
    }

    public virtual void Init()
    {

    }
}
