using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NpcHealth : MonoBehaviour
{
    [SerializeField] private GameObject HealthBarGO;
    private Slider Slider;
    private NpcBase Npc;
    private SkinnedMeshRenderer MeshRenderer;

    private int MaxHealth = 100;
    private float CurrentHealth;
    public Action Death;
    public Action<PlayerBase> HitWhileIdle;

    private bool _showHealthbar => CurrentHealth < MaxHealth && CurrentHealth > 0;
    public bool IsDead => CurrentHealth <= 0;

    void Awake()
    {
        Npc = GetComponent<NpcBase>();
        Slider = GetComponentInChildren<Slider>();
        CurrentHealth = MaxHealth;
        SetHealth(MaxHealth);
    }

    private void Start()
    {
        this.MeshRenderer = Npc.SkinSelected.GetComponent<SkinnedMeshRenderer>();

    }

    public void Assassinated(float dmgAmount)
    {
        var newHealth = (CurrentHealth - dmgAmount);
        SetHealth(newHealth);
        if (CurrentHealth <= 0 && Npc.CurrentState != Npc.StateManager.DeadState) Death.Invoke();
    }


    public void TakeDamage(float dmgAmount, PlayerBase player)
    {
        var newHealth = (CurrentHealth - dmgAmount);

        if (Npc.HostileTarget == null)
        {
            Npc.HostileTarget = player;
            if (Npc.CurrentState == Npc.StateManager.IdleState)
            {
                HitWhileIdle.Invoke(player);
                Npc.StateManager.SwitchState(Npc.StateManager.ChasingState, Npc);
            }
        }

        player.NpcSetHealthServerRpc(Npc.NpcSceneId, newHealth);
        SetHealth(newHealth);
    }

    public void SetHealth(float newHealth)
    {
        var isDead = Npc.CurrentState == Npc.StateManager.DeadState;
        if (!isDead && CurrentHealth > newHealth) StartCoroutine(HitFlash());

        CurrentHealth = newHealth;
        Slider.value = newHealth;
        ShowHideHealthBar();
        if (CurrentHealth <= 0 && !isDead) Death.Invoke();
    }

    IEnumerator HitFlash()
    {
        this.MeshRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        this.MeshRenderer.material.color = Color.white;
    }

    public void SetMaxHealth(float health)
    {
        Slider.maxValue = health;
        SetHealth(health);
    }

    private void ShowHideHealthBar()
    {
        HealthBarGO.SetActive(_showHealthbar);
        if (_showHealthbar) StartCoroutine(ShowHealthToCamera());
        else StopCoroutine(ShowHealthToCamera());
    }

    protected IEnumerator ShowHealthToCamera()
    {
        float delay = 0.02f;
        WaitForSeconds wait = new WaitForSeconds(delay);
        while (!IsDead)
        {
            yield return wait;
            HelperMethods.SpriteToFaceCamera(HealthBarGO.transform);
        }
    }
}
