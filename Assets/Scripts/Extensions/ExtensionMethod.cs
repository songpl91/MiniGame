using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;
using UnityEngine;

public static class ExtensionMethod
{
    public static void SafeSetActive(this GameObject gameObject, bool active)
    {
        if (gameObject.activeSelf != active)
        {
            gameObject.SetActive(active);
        }
    }

    public static Coroutine DelayInvoke(this MonoBehaviour behaviour, Action action, float delayTime)
    {
        if (delayTime <= 0f)
        {
            action?.Invoke();
            return null;
        }

        return behaviour.StartCoroutine(DelayInvoke(action, delayTime));
    }

    private static IEnumerator DelayInvoke(Action action, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        action?.Invoke();
    }

    public static bool IsSameColor(this Color color, Color otherColor)
    {
        return Vector4.Distance(color, otherColor) < 0.0039f;
    }


    public static void ClearTransformChild(this Transform transform, bool checkActive = false)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            if (checkActive)
            {
                if (obj.activeSelf)
                {
                    UnityEngine.Object.Destroy(obj);
                }
            }
            else
            {
                UnityEngine.Object.Destroy(obj);
            }
        }
    }

    public static bool IsInRange(this Vector2Int range, Vector2Int vector2Int)
    {
        return vector2Int.x >= 0 && vector2Int.y >= 0 && vector2Int.x <= range.x && vector2Int.y <= range.y;
    }

    public static void SetContentSize(this Transform tran, Vector2 size)
    {
        RectTransform rectTran = tran as RectTransform;

        if (rectTran != null)
        {
            rectTran.sizeDelta = size;
        }
    }
}


public static class ExtensionResources
{
    public static ResourcesRequestAwaiter GetAwaiter(this ResourceRequest request) => new ResourcesRequestAwaiter(request);

    public static async Task<T> LoadResourcesAsync<T>(string path) where T : UnityEngine.Object
    {
        var gres = Resources.LoadAsync(path);
        await gres;
        return gres.asset as T;
    }
}

public class ResourcesRequestAwaiter : INotifyCompletion
{
    public Action Continuation;
    public ResourceRequest ResourceRequest;
    public bool IsCompleted => ResourceRequest.isDone;
    public ResourcesRequestAwaiter(ResourceRequest resourceRequest)
    {
        this.ResourceRequest = resourceRequest;
        this.ResourceRequest.completed += Accomplish;
    }

    public void OnCompleted(Action continuation) => this.Continuation = continuation;

    public void Accomplish(AsyncOperation async) => Continuation?.Invoke();

    public void GetResult()
    {

    }
} 
