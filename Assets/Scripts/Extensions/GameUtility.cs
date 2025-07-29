using UnityEngine;

public static class GameUtility
{
    public static Vector3 ExpDecay(Vector3 a, Vector3 b, float decay, float deltaTime)
    {
        return b + (a - b) * Mathf.Exp(-decay * deltaTime);
    }

    // public static Vector3 ConvertUItoGamePos(Vector3 position)
    // {
    //     var pos = UIManager.Instance.UICamera.WorldToScreenPoint(position);
    //     return Camera.main.ScreenToWorldPoint(pos);
    // }
    //
    // public static Vector3 ConvertGameToUIPos(Vector3 position)
    // {
    //     var pos = Camera.main.WorldToScreenPoint(position);
    //     return UIManager.Instance.UICamera.ScreenToWorldPoint(pos);
    // }
}