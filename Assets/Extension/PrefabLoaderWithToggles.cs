//스크롤뷰에서 Content에 불러올 프리팹을 인스펙터에 지정
//해당 프리팹의 특정 콤포넌트를 끄거나 켤 수 있게 설정



using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

[Serializable]
public class ComponentToggle
{
    [Tooltip("Full type name of the Behaviour component to toggle (e.g. 'UnityEngine.UI.Image')")]
    public string componentTypeName;
    [Tooltip("Enable or disable the component on the instantiated prefab")]
    public bool enable;
}

/// <summary>
/// PrefabLoaderWithToggles
/// Instantiates a specified prefab under a target parent and toggles components on the instance.
/// </summary>
public class PrefabLoaderWithToggles : MonoBehaviour
{
    [Header("Prefab to Use")]
    [Tooltip("Prefab to instantiate under the target parent")]
    public GameObject prefabToUse;

    [Header("Component Toggles")]
    [Tooltip("List of component types on the prefab to enable or disable on instantiate")]
    public List<ComponentToggle> componentToggles;

    [Header("Target Parent")]
    [Tooltip("Parent transform under which the prefab will be instantiated")]
    public Transform targetParent;

    private void Start()
    {
        LoadPrefab();
    }

    /// <summary>
    /// Instantiates the prefab under the target parent and applies component toggles.
    /// </summary>
    private void LoadPrefab()
    {
        if (prefabToUse == null || targetParent == null)
            return;

        var instance = Instantiate(prefabToUse, targetParent);
        ApplyComponentToggles(instance);
    }

    /// <summary>
    /// Enables or disables specified Behaviour components on the instantiated prefab.
    /// Improved type resolution: searches loaded assemblies if direct Type.GetType fails.
    /// </summary>
    /// <param name="instance">The instantiated prefab GameObject.</param>
    private void ApplyComponentToggles(GameObject instance)
    {
        foreach (var toggle in componentToggles)
        {
            if (string.IsNullOrEmpty(toggle.componentTypeName))
                continue;

            Type type = Type.GetType(toggle.componentTypeName);
            if (type == null)
            {
                // Search all loaded assemblies for the type
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(toggle.componentTypeName);
                    if (type != null)
                        break;
                }
            }

            // Ensure it's a Behaviour
            if (type == null || !typeof(Behaviour).IsAssignableFrom(type))
                continue;

            var comp = instance.GetComponent(type) as Behaviour;
            if (comp != null)
                comp.enabled = toggle.enable;
        }
    }
}
