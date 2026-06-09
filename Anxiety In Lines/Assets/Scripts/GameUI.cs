using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏 HUD：NoP (前方人数)、GT (剩余时间)、协商提示、胜负画面
/// </summary>
public class GameUI : MonoBehaviour
{
    GameManager gm;

    Text noPText;
    Text gtText;
    Text promptText;
    Text endText;
    Text restartText;
    Text controlsText;
    Text resultText;

    float resultDisplayTimer;

    public void Initialize(GameManager manager)
    {
        gm = manager;

        // Canvas setup
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // HUD — top left: NoP
        noPText = MakeText("NoPText",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(40, -40), new Vector2(350, 60),
            34, TextAnchor.UpperLeft);

        // HUD — top right: GT
        gtText = MakeText("GTText",
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-40, -40), new Vector2(350, 60),
            34, TextAnchor.UpperRight);

        // Controls hint — bottom left
        controlsText = MakeText("Controls",
            new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(40, 40), new Vector2(600, 40),
            18, TextAnchor.LowerLeft);
        controlsText.color = new Color(1, 1, 1, 0.35f);
        controlsText.text = "[Up] Forward  |  [Y] Cut in  |  [N] Wait  |  [R] Restart";

        // Negotiation prompt — center
        promptText = MakeText("Prompt",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -220), new Vector2(700, 60),
            26, TextAnchor.MiddleCenter);
        promptText.color = new Color(1f, 0.85f, 0.2f);
        promptText.gameObject.SetActive(false);

        // Cut result — center (brief flash)
        resultText = MakeText("Result",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -160), new Vector2(500, 50),
            22, TextAnchor.MiddleCenter);
        resultText.gameObject.SetActive(false);

        // End screen — center
        endText = MakeText("EndText",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(900, 90),
            54, TextAnchor.MiddleCenter);
        endText.gameObject.SetActive(false);

        restartText = MakeText("Restart",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -70), new Vector2(400, 50),
            22, TextAnchor.MiddleCenter);
        restartText.color = new Color(1, 1, 1, 0.5f);
        restartText.gameObject.SetActive(false);
    }

    Text MakeText(string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 size, int fontSize, TextAnchor alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var txt = go.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null)
            txt.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        txt.fontSize = fontSize;
        txt.alignment = alignment;
        txt.color = Color.white;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;

        return txt;
    }

    void Update()
    {
        if (gm.gameState == GameManager.GameState.Playing)
        {
            noPText.text = $"Ahead: {gm.noP}";
            gtText.text = $"Time: {gm.remainingTime:F1}s";
            gtText.color = gm.remainingTime < 10f
                ? new Color(1f, 0.25f, 0.25f)
                : Color.white;
        }

        // Cut result auto-hide
        if (resultText.gameObject.activeSelf)
        {
            resultDisplayTimer -= Time.deltaTime;
            if (resultDisplayTimer <= 0)
                resultText.gameObject.SetActive(false);
        }

        // Restart input
        if (gm.gameState != GameManager.GameState.Playing)
        {
            if (UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
                gm.RestartGame();
        }
    }

    public void ShowNegotiationPrompt(bool show)
    {
        promptText.gameObject.SetActive(show);
        if (show)
            promptText.text = "Someone's ahead! Cut in?  [Y] Yes  [N] No";
    }

    public void ShowCutResult(bool success, float penalty)
    {
        resultText.gameObject.SetActive(true);
        resultText.color = success
            ? new Color(0.3f, 0.9f, 0.4f)
            : new Color(1f, 0.3f, 0.3f);
        resultText.text = success
            ? "Cut succeeded!"
            : $"Cut failed! -{penalty:F0}s";
        resultDisplayTimer = 1.5f;
    }

    public void ShowEndScreen(bool won)
    {
        noPText.gameObject.SetActive(false);
        gtText.gameObject.SetActive(false);
        promptText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        controlsText.gameObject.SetActive(false);

        endText.gameObject.SetActive(true);
        endText.text = won ? "YOU MADE IT!" : "TIME'S UP!";
        endText.color = won
            ? new Color(0.3f, 0.9f, 0.4f)
            : new Color(1f, 0.3f, 0.3f);

        restartText.gameObject.SetActive(true);
        restartText.text = "Press [R] to Restart";
    }
}
