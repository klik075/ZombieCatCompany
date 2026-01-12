using System.Collections;
using UnityEngine;

public class CoroutineManager : Singleton<CoroutineManager>
{
    public Coroutine Run(IEnumerator coroutine)
    {
        if (coroutine == null)
        {
            Debug.LogWarning("CoroutineManager: Trying to start null coroutine!");
            return null;
        }
        
        return StartCoroutine(coroutine);
    }
    
    public void Stop(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }
    
    public void StopAll()
    {
        StopAllCoroutines();
    }
}