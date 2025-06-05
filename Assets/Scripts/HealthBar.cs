using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class HealthBar : MonoBehaviour
{
    [Header("Style")]
    public Color healthColor;
    public Sprite healthSprite;
    public float margin = 0;

    [Header("Value")]
    public float value;
    public float maxValue;

    [Header("Debug")]
    public float relativeToMax;

    private RectTransform rTransform;

    private void Start()
    {
        rTransform = this.GetComponent<RectTransform>();
    }

    public void setMaxValue(float newVal) {
        if (newVal <= 0)
            throw new System.ArgumentOutOfRangeException($"La valeur maximale doit etre strictement plus grande que 0");

        maxValue = value;

        RenderHealthBar();
    }

    public void setHealthValue(float newValue) {
        if (newValue > maxValue)
            throw new System.ArgumentOutOfRangeException($"la valeur {newValue.ToString("F1")} doit etre plus petite que {maxValue.ToString("F1")}");

        value = newValue;

        RenderHealthBar();
    }

    private void RenderHealthBar()
    {
        // Remove all children first
        deleteAllChildren();

        relativeToMax = value / maxValue;

        float xTotalLength = rTransform.rect.width;
        float yTotalLength = rTransform.rect.height;

        float xLength = xTotalLength - (margin * 2);
        float yLength = yTotalLength - (margin * 2);

        GameObject healthBar = new GameObject("HealthBar");
        healthBar.transform.SetParent(this.transform);

        // -- Adding the image -- //
        Image image = healthBar.AddComponent<Image>();
        image.color = healthColor;
        image.sprite = healthSprite;
        image.rectTransform.localScale = Vector3.one;

        float xSize = xLength * relativeToMax;
        float ySize = yLength;
        image.rectTransform.sizeDelta = new Vector2(xSize, ySize);

        // -- Setting the position of the HealthBar -- //
        float xPos = (xTotalLength * 0.5f) - margin - (xSize * 0.5f);
        healthBar.transform.localPosition = new Vector3(-xPos, 0, 0);
    }

    private void deleteAllChildren() {
        foreach (Transform transform in this.transform) {
            Destroy(transform.gameObject);
        }
    }
}
