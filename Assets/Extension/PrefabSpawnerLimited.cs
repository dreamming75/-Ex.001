using UnityEngine;
using System.Collections.Generic;

public class LimitedPrefabSpawner : MonoBehaviour
{
    public GameObject prefab;
    public Transform parentObject;
    public int maxCount = 5;

    private Queue<GameObject> spawnedObjects = new Queue<GameObject>();

    void Awake()
    {
        if (parentObject != null)
        {
            var watcher = parentObject.GetComponent<ObjectEnableDisableWatcher>();
            if (watcher == null)
                watcher = parentObject.gameObject.AddComponent<ObjectEnableDisableWatcher>();
            watcher.OnDisabled += CleanUpAllChildren;
            watcher.OnEnabled += CleanUpAllChildren; // 부모가 켜질 때도 삭제!
        }
    }

    // 버튼 등에서 호출
    public void SpawnPrefab()
    {
        GameObject obj = Instantiate(prefab, parentObject);
        spawnedObjects.Enqueue(obj);

        if (spawnedObjects.Count > maxCount)
        {
            GameObject oldObj = spawnedObjects.Dequeue();
            if (oldObj != null)
                Destroy(oldObj);
        }
    }

    // parentObject 밑 자식 전체 삭제
    public void CleanUpAllChildren()
    {
        if (parentObject == null) return;

        for (int i = parentObject.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parentObject.GetChild(i).gameObject);
        }

        spawnedObjects.Clear();
    }

    // 내부에서 사용할 감시자 클래스
    public class ObjectEnableDisableWatcher : MonoBehaviour
    {
        public System.Action OnEnabled;
        public System.Action OnDisabled;

        private void OnEnable()
        {
            if (OnEnabled != null)
                OnEnabled.Invoke();
        }

        private void OnDisable()
        {
            if (OnDisabled != null)
                OnDisabled.Invoke();
        }

        private void OnDestroy()
        {
            if (OnDisabled != null)
                OnDisabled.Invoke();
        }
    }
}
