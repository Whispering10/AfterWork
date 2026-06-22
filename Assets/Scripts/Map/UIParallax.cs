using UnityEngine;
using UnityEngine.UI;

public class UIParallax : MonoBehaviour
{
    [Header("Ссылка на камеру (или игрока)")]
    public Transform cameraTransform;

    [Header("Настройки слоя")]
    public float parallaxSpeed = 0.2f;   // Скорость слоя
    public bool horizontal = true;       // Движение по X или Y

    private RawImage rawImage;
    private Rect uvRect;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        // Убедитесь, что у вашей текстуры в настройках Wrap Mode = Repeat
        uvRect = rawImage.uvRect;
    }

    void Update()
    {
        // Берем позицию камеры
        float pos = horizontal ? cameraTransform.position.x : cameraTransform.position.y;
        // Вычисляем смещение (берем дробную часть для бесконечного повтора)
        float offset = Mathf.Repeat(pos * parallaxSpeed, 1f);

        // Сдвигаем UV-прямоугольник
        if (horizontal)
            uvRect.x = offset;
        else
            uvRect.y = offset;

        rawImage.uvRect = uvRect;
    }
}