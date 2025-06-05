using TMPro;
using UnityEngine;

public class DebugInterface : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private int currentFps;

    private void Update()
    {
        currentFps = (int)(1 / Time.unscaledDeltaTime);
        fpsText.text = $"fps: {currentFps.ToString()}";
    }
}
