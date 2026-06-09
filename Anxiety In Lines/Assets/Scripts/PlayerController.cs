using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 玩家控制：↑ 前进、Y/N 协商插队
/// </summary>
public class PlayerController : MonoBehaviour
{
    public int currentSlot;

    GameManager gm;
    float cooldownTimer;
    const float MoveCooldown = 0.25f;
    SpriteRenderer sr;
    Color baseColor;

    public void Initialize(int slot, GameManager gm)
    {
        this.currentSlot = slot;
        this.gm = gm;
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
    }

    public void HandleInput()
    {
        if (gm.gameState != GameManager.GameState.Playing) return;

        cooldownTimer -= Time.deltaTime;

        var kb = Keyboard.current;
        if (kb == null) return;

        // --- Negotiation input ---
        if (gm.negotiationActive)
        {
            if (kb.yKey.wasPressedThisFrame)
                gm.ResolveNegotiation(true);
            else if (kb.nKey.wasPressedThisFrame)
                gm.ResolveNegotiation(false);

            // Pulse player while negotiating
            sr.color = Color.Lerp(baseColor, Color.white, 0.3f * Mathf.PingPong(Time.time * 4f, 1f));
            return;
        }

        sr.color = baseColor;

        // --- Movement input ---
        if (kb.upArrowKey.wasPressedThisFrame && cooldownTimer <= 0)
        {
            cooldownTimer = MoveCooldown;
            int slotAhead = currentSlot - 1;

            if (slotAhead < 0) return; // Already at front

            if (!gm.IsSlotOccupied(slotAhead))
            {
                // Slot is free: move forward
                currentSlot = slotAhead;
                transform.position = gm.GetSlotPosition(currentSlot);
                gm.OnPlayerMoved();
            }
            else
            {
                // NPC ahead: start negotiation
                gm.StartNegotiation();
            }
        }
    }

    public void ForceMoveToSlot(int slot)
    {
        currentSlot = slot;
        transform.position = gm.GetSlotPosition(slot);
    }
}
