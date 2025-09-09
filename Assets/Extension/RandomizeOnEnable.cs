using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class RandomizeOnEnable : MonoBehaviour
{
    [Header("🔹 랜덤 이미지 후보 (여러 개 등록)")]
    public Sprite[] sprites;

    [Header("🔹 이미지 적용 대상 (택1, 비워두면 자동 탐색 시도)")]
    public SpriteRenderer targetSpriteRenderer; // 2D Sprite
    public Image targetUIImage;                 // UI Image
    public bool uiSetNativeSize = false;        // UI일 때 SetNativeSize 할지

    [Header("🔹 위치 랜덤")]
    public bool randomizePosition = true;
    public bool useWorldSpaceForPosition = false; // true면 world 좌표, false면 local 좌표
    public Vector3 positionMin = new Vector3(-1f, -1f, 0f);
    public Vector3 positionMax = new Vector3( 1f,  1f, 0f);

    [Tooltip("아래 콜라이더/콜라이더2D가 있으면 그 바운즈 안에서 배치함(월드 기준).")]
    public BoxCollider areaCollider3D;     // 선택
    public BoxCollider2D areaCollider2D;   // 선택

    [Header("🔹 로테이션 랜덤 (오일러 각, 도 단위)")]
    public bool randomizeRotation = true;
    public bool useWorldSpaceForRotation = false; // true면 world 회전, false면 local 회전
    public Vector3 rotationMin = new Vector3(0f, 0f, 0f);
    public Vector3 rotationMax = new Vector3(0f, 0f, 360f);

    [Header("🔹 스케일 랜덤")]
    public bool randomizeScale = true;
    public bool uniformScale = true; // true면 XYZ 같은 크기
    [Min(0f)] public float uniformScaleMin = 0.8f;
    [Min(0f)] public float uniformScaleMax = 1.2f;
    public Vector3 scaleMin = Vector3.one * 0.8f; // non-uniform일 때 사용
    public Vector3 scaleMax = Vector3.one * 1.2f;

    void OnEnable()
    {
        ApplyRandomSprite();

        if (randomizePosition)
            ApplyRandomPosition();

        if (randomizeRotation)
            ApplyRandomRotation();

        if (randomizeScale)
            ApplyRandomScale();
    }

    // --- 이미지 랜덤 적용 ---
    void ApplyRandomSprite()
    {
        if (sprites != null && sprites.Length > 0)
        {
            Sprite chosen = sprites[Random.Range(0, sprites.Length)];

            // 대상 자동 탐색(비워두면)
            if (targetSpriteRenderer == null && targetUIImage == null)
            {
                targetSpriteRenderer = GetComponent<SpriteRenderer>();
                if (targetSpriteRenderer == null)
                    targetUIImage = GetComponent<Image>();
            }

            if (targetSpriteRenderer != null)
            {
                targetSpriteRenderer.sprite = chosen;
            }
            else if (targetUIImage != null)
            {
                targetUIImage.sprite = chosen;
                if (uiSetNativeSize) targetUIImage.SetNativeSize();
            }
        }
    }

    // --- 위치 랜덤 ---
    void ApplyRandomPosition()
    {
        Vector3 pos;

        // 콜라이더 우선 사용(월드 기준)
        if (areaCollider3D != null)
        {
            Bounds b = areaCollider3D.bounds;
            pos = new Vector3(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y, b.max.y),
                Random.Range(b.min.z, b.max.z)
            );
            transform.position = pos;
            return;
        }
        if (areaCollider2D != null)
        {
            Bounds b = areaCollider2D.bounds;
            pos = new Vector3(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y, b.max.y),
                transform.position.z
            );
            transform.position = pos;
            return;
        }

        // 범위(min~max)에서 샘플
        pos = new Vector3(
            Random.Range(positionMin.x, positionMax.x),
            Random.Range(positionMin.y, positionMax.y),
            Random.Range(positionMin.z, positionMax.z)
        );

        if (useWorldSpaceForPosition) transform.position = pos;
        else                          transform.localPosition = pos;
    }

    // --- 로테이션 랜덤 ---
    void ApplyRandomRotation()
    {
        Vector3 euler = new Vector3(
            Random.Range(rotationMin.x, rotationMax.x),
            Random.Range(rotationMin.y, rotationMax.y),
            Random.Range(rotationMin.z, rotationMax.z)
        );
        Quaternion q = Quaternion.Euler(euler);

        if (useWorldSpaceForRotation) transform.rotation = q;
        else                          transform.localRotation = q;
    }

    // --- 스케일 랜덤 ---
    void ApplyRandomScale()
    {
        if (uniformScale)
        {
            float s = Random.Range(uniformScaleMin, uniformScaleMax);
            transform.localScale = new Vector3(s, s, s);
        }
        else
        {
            Vector3 s = new Vector3(
                Random.Range(scaleMin.x, scaleMax.x),
                Random.Range(scaleMin.y, scaleMax.y),
                Random.Range(scaleMin.z, scaleMax.z)
            );
            transform.localScale = s;
        }
    }

    // 에디터에서 우클릭 메뉴로 한 번 테스트하기 좋음
    [ContextMenu("Run Once (Editor)")]
    void RunOnceInEditor()
    {
        OnEnable();
    }
}
