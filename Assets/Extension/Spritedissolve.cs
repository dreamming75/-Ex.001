using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Spritedissolve : MonoBehaviour
{
    [Range(0f, 10f)]
    public float colorPower = 1.0f;

    private Material mat;
    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
        mat = Instantiate(image.material); // 복제본 만들기!
        image.material = mat;

        if (!mat.HasProperty("_ColorPower"))
        {
            Debug.LogError("Material에 '_ColorPower' 속성이 없습니다.");
        }
    }

    void Update()
    {
        if (mat != null)
        {
            mat.SetFloat("_ColorPower", colorPower); // Inspector 슬라이더 값 반영
        }
    }
}
