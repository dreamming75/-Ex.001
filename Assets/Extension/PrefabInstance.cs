using UnityEngine;

public class MyClass : MonoBehaviour
{
    public GameObject prefab;
    public Transform parent;

    void Start()
    {
        // Instantiate the prefab as a child of the parent transform
        GameObject myInstance = Instantiate(prefab, parent);

        // Set the position relative to the parent (or use parent.position directly)
        myInstance.transform.position = parent.position + new Vector3(0, 1.0f, 0);

        // Access the component and initialize it
        MyChild childComponent = myInstance.GetComponent<MyChild>();
        if (childComponent != null)
        {
            childComponent.Init();
        }

        // Make the object active in the world
        myInstance.SetActive(true);
    }
}

public class MyChild : MonoBehaviour
{
    //string name;

    // new 키워드를 사용하여 부모의 'name' 속성을 숨긴다고 명시
    public new string name;

    public void Init()
    {
        // Do something. Initialize or change data
        name = "Init";
    }
    
    public string GetName() // Method to access the private name field
    {
        return name;
    }

}