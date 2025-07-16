using UnityEngine;

public class SelfDestroyOnDisable : MonoBehaviour
{
    private void OnDisable()
    {
        DestroyImmediate(gameObject);
    }
}
