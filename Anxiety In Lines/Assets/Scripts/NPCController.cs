using UnityEngine;

/// <summary>
/// NPC 排队行为：随机等待时间 (RWT) 后自动前移，到达柜台后接受服务并消失
/// </summary>
public class NPCController : MonoBehaviour
{
    public int currentSlot;

    GameManager gm;
    float waitTimer;
    bool isWaiting;
    bool isAtCounter;
    float serviceTimer;
    Color originalColor;
    SpriteRenderer sr;

    public void Initialize(int slot, GameManager gm)
    {
        this.currentSlot = slot;
        this.gm = gm;
        isWaiting = false;
        isAtCounter = false;
        serviceTimer = 0;
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    public void Tick()
    {
        if (isAtCounter)
        {
            // Visual: pulse while being served
            float alpha = 0.35f + 0.25f * Mathf.PingPong(Time.time * 3f, 1f);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            serviceTimer -= Time.deltaTime;
            if (serviceTimer <= 0)
                gm.OnNPCServed(this);
            return;
        }

        // Check if the slot ahead is free
        int slotAhead = currentSlot - 1;
        bool aheadFree = !gm.IsSlotOccupied(slotAhead);

        if (aheadFree)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = Random.Range(gm.minRWT, gm.maxRWT);
            }

            waitTimer -= Time.deltaTime;

            // Visual: subtle shrink while waiting to move
            float t = 1f - (waitTimer / Mathf.Max(0.01f, gm.maxRWT)) * 0.1f;
            transform.localScale = new Vector3(0.6f * t, 0.6f * t, 1f);

            if (waitTimer <= 0)
            {
                currentSlot = slotAhead;
                transform.position = gm.GetSlotPosition(currentSlot);
                isWaiting = false;
                transform.localScale = new Vector3(0.6f, 0.6f, 1f);

                if (currentSlot == 0)
                {
                    isAtCounter = true;
                    serviceTimer = gm.serviceTime;
                }
            }
        }
        else
        {
            isWaiting = false;
            transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        }
    }

    public void ForceMoveToSlot(int slot)
    {
        currentSlot = slot;
        transform.position = gm.GetSlotPosition(slot);
        isAtCounter = (slot == 0);
        isWaiting = false;
        transform.localScale = new Vector3(0.6f, 0.6f, 1f);

        if (isAtCounter)
            serviceTimer = gm.serviceTime;
        else
            sr.color = originalColor;
    }
}
