using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Anxiety In Lines — 核心游戏控制器
/// 管理游戏流程：排队机制、计时、胜负判定、协商（插队）
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int totalNPCs = 5;
    public float gameTime = 45f;
    public float slotSpacing = 1.3f;
    public float minRWT = 3f;
    public float maxRWT = 7f;
    public float serviceTime = 3f;
    [Range(0f, 1f)] public float cutSuccessChance = 0.6f;
    public float cutFailTimePenalty = 3f;

    [Header("Runtime State")]
    public int noP;
    public float remainingTime;
    public GameState gameState;
    public bool negotiationActive;

    public enum GameState { Playing, Won, Lost }

    readonly List<NPCController> npcs = new();
    PlayerController player;
    GameUI gameUI;
    Sprite whiteSprite;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CreateWhiteSprite();
    }

    void Start()
    {
        SetupCamera();
        SetupGame();
    }

    void CreateWhiteSprite()
    {
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.transform.position = new Vector3(0.5f, -0.3f, -10);
        cam.orthographicSize = 5.5f;
        cam.backgroundColor = new Color(0.08f, 0.09f, 0.13f);
    }

    void SetupGame()
    {
        remainingTime = gameTime;
        gameState = GameState.Playing;
        negotiationActive = false;
        noP = totalNPCs;

        CreateFloor();
        CreateCounter();
        CreateSlotMarkers();

        Color[] npcColors = {
            new(0.85f, 0.35f, 0.19f),  // coral
            new(0.94f, 0.62f, 0.15f),  // amber
            new(0.83f, 0.33f, 0.49f),  // pink
            new(0.78f, 0.45f, 0.22f),  // brown-orange
            new(0.70f, 0.28f, 0.28f),  // dark red
        };

        for (int i = 0; i < totalNPCs; i++)
        {
            int slot = i + 1;
            npcs.Add(CreateNPC(slot, npcColors[i % npcColors.Length], i + 1));
        }

        player = CreatePlayer(totalNPCs + 1);

        var uiObj = new GameObject("GameUI");
        gameUI = uiObj.AddComponent<GameUI>();
        gameUI.Initialize(this);
    }

    // --- Visual Object Creation ---

    GameObject MakeSprite(string name, Vector3 pos, Vector3 scale, Color color, int order = 0)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = whiteSprite;
        sr.color = color;
        sr.sortingOrder = order;
        go.transform.localScale = scale;
        return go;
    }

    void CreateFloor()
    {
        float height = (totalNPCs + 3) * slotSpacing;
        MakeSprite("Floor",
            new Vector3(0, 2.5f - height / 2f + slotSpacing, 0),
            new Vector3(2.2f, height, 1),
            new Color(0.13f, 0.14f, 0.18f), -2);
    }

    void CreateCounter()
    {
        MakeSprite("Counter",
            new Vector3(0, GetSlotY(0) + 0.85f, 0),
            new Vector3(2.8f, 0.35f, 1),
            new Color(0.25f, 0.62f, 0.42f), 1);

        MakeSprite("CounterLabel",
            new Vector3(0, GetSlotY(0) + 1.2f, 0),
            new Vector3(0.08f, 0.08f, 1),
            new Color(0.25f, 0.62f, 0.42f), 1);
    }

    void CreateSlotMarkers()
    {
        for (int i = 0; i <= totalNPCs + 1; i++)
        {
            MakeSprite($"Slot_{i}",
                new Vector3(0, GetSlotY(i), 0),
                new Vector3(1.8f, 0.03f, 1),
                new Color(0.20f, 0.21f, 0.26f), -1);
        }
    }

    NPCController CreateNPC(int slot, Color color, int id)
    {
        var go = MakeSprite($"NPC_{id}", GetSlotPosition(slot),
            new Vector3(0.6f, 0.6f, 1), color, 2);

        // Small number indicator above NPC
        var numObj = MakeSprite($"NPC_{id}_Num",
            GetSlotPosition(slot) + new Vector3(0, 0.45f, 0),
            new Vector3(0.2f, 0.2f, 1),
            new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f), 2);
        numObj.transform.parent = go.transform;

        var npc = go.AddComponent<NPCController>();
        npc.Initialize(slot, this);
        return npc;
    }

    PlayerController CreatePlayer(int slot)
    {
        var go = MakeSprite("Player", GetSlotPosition(slot),
            new Vector3(0.6f, 0.6f, 1),
            new Color(0.22f, 0.54f, 0.87f), 3);

        // Arrow indicator above player
        var arrow = MakeSprite("PlayerArrow",
            GetSlotPosition(slot) + new Vector3(0, 0.48f, 0),
            new Vector3(0.25f, 0.15f, 1),
            new Color(0.40f, 0.70f, 1.0f), 3);
        arrow.transform.parent = go.transform;

        var pc = go.AddComponent<PlayerController>();
        pc.Initialize(slot, this);
        return pc;
    }

    // --- Slot Position ---

    public float GetSlotY(int slot) => 3f - slot * slotSpacing;

    public Vector3 GetSlotPosition(int slot) => new(0, GetSlotY(slot), 0);

    // --- Game Loop ---

    void Update()
    {
        if (gameState != GameState.Playing) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            gameState = GameState.Lost;
            gameUI.ShowEndScreen(false);
            return;
        }

        // Tick all NPCs
        foreach (var npc in npcs.ToArray())
            npc.Tick();

        // Handle player input
        player.HandleInput();

        // Update NoP
        noP = CalculateNoP();
    }

    int CalculateNoP()
    {
        int count = 0;
        foreach (var npc in npcs)
            if (npc.currentSlot < player.currentSlot) count++;
        return count;
    }

    // --- Slot Queries ---

    public bool IsSlotOccupied(int slot)
    {
        if (player != null && player.currentSlot == slot) return true;
        foreach (var npc in npcs)
            if (npc.currentSlot == slot) return true;
        return false;
    }

    public NPCController GetNPCAtSlot(int slot)
    {
        foreach (var npc in npcs)
            if (npc.currentSlot == slot) return npc;
        return null;
    }

    // --- Game Events ---

    public void OnNPCServed(NPCController npc)
    {
        npcs.Remove(npc);
        Destroy(npc.gameObject);
        noP = CalculateNoP();
        CheckWin();
    }

    public void OnPlayerMoved()
    {
        noP = CalculateNoP();
        CheckWin();
    }

    void CheckWin()
    {
        if (noP <= 0 && remainingTime > 0 && gameState == GameState.Playing)
        {
            gameState = GameState.Won;
            gameUI.ShowEndScreen(true);
        }
    }

    // --- Negotiation (Cut-in) ---

    public void StartNegotiation()
    {
        negotiationActive = true;
        gameUI.ShowNegotiationPrompt(true);
    }

    public void ResolveNegotiation(bool tryCut)
    {
        negotiationActive = false;
        gameUI.ShowNegotiationPrompt(false);

        if (!tryCut) return;

        if (Random.value <= cutSuccessChance)
        {
            // Success: swap player and NPC
            int playerSlot = player.currentSlot;
            int npcSlot = playerSlot - 1;
            var npc = GetNPCAtSlot(npcSlot);
            player.ForceMoveToSlot(npcSlot);
            if (npc != null) npc.ForceMoveToSlot(playerSlot);
            noP = CalculateNoP();
            gameUI.ShowCutResult(true, 0);
            CheckWin();
        }
        else
        {
            // Fail: time penalty
            remainingTime = Mathf.Max(0, remainingTime - cutFailTimePenalty);
            gameUI.ShowCutResult(false, cutFailTimePenalty);
            if (remainingTime <= 0)
            {
                gameState = GameState.Lost;
                gameUI.ShowEndScreen(false);
            }
        }
    }

    // --- Restart ---

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
