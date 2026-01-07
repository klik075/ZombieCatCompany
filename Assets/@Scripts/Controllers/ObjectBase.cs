using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    public bool Pooling { get; set; } = false;

    public virtual void Awake()
    {
        Init();
    }

    public virtual void Init()
    {

    }
}
