using UnityEngine;

/* =============================================================================
   Project:        $PROJECT_NAME$
   File:           $NAME$.cs
   Author:         $USER$
   Studio:         SundayMood Studios (Indie Home Studio)
   IDE:            JetBrains Rider
   Engine:         Unity
   Created:        $DATE$

   Description:
   ---------------------------------------------------------------------------
   [Brief description of what this script does.]

   Notes:
   ---------------------------------------------------------------------------
   - Part of the $PROJECT_NAME$ project by SundayMood Studios.
   ========================================================================== */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Central gameplay state manager.
/// Modern Unity equivalent of legacy GManager.
/// </summary>
public sealed class GManager : MonoBehaviour
{
    public static GManager Instance { get; private set; }

    // ======================================================
    // Runtime instances
    // ======================================================

    private readonly Dictionary<int, RuntimeInstance> _runtimeInstances =
        new Dictionary<int, RuntimeInstance>(1024);

    // ======================================================
    // Gameplay State
    // ======================================================

    public bool IsInGameplay { get; private set; }
    public bool IsWarping { get; private set; }
    public bool StartFreeRoamPursuit { get; private set; }

    // ======================================================
    // Timers
    // ======================================================

    private readonly Dictionary<string, float> _timers =
        new Dictionary<string, float>();

    // ======================================================
    // SMS System
    // ======================================================

    private readonly Queue<int> _pendingSMS = new Queue<int>();
    private float _lastSMSTime;

    // ======================================================
    // Milestones / Speed Traps
    // ======================================================

    [SerializeField] private Milestone[] milestones;
    [SerializeField] private SpeedTrap[] speedTraps;

    // ======================================================
    // Unity Lifecycle
    // ======================================================

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PreBeginGameplay();
    }

    private void Update()
    {
        UpdateTimers(Time.deltaTime);
        UpdatePendingSMS();
    }

    // ======================================================
    // Gameplay Flow
    // ======================================================

    public void PreBeginGameplay()
    {
        IsInGameplay = false;
    }

    public void BeginGameplay()
    {
        IsInGameplay = true;
        StartWorldActivities();
    }

    public void EndGameplay()
    {
        SuspendAllActivities();
        IsInGameplay = false;
    }

    // ======================================================
    // Runtime Instance Management
    // ======================================================

    public void RegisterInstance(RuntimeInstance instance)
    {
        if (instance == null) return;
        _runtimeInstances[instance.Key] = instance;
    }

    public void UnregisterInstance(RuntimeInstance instance)
    {
        if (instance == null) return;
        _runtimeInstances.Remove(instance.Key);
    }

    public RuntimeInstance FindInstance(int key)
    {
        _runtimeInstances.TryGetValue(key, out var instance);
        return instance;
    }

    // ======================================================
    // Activities
    // ======================================================

    private void StartWorldActivities(bool freeRoamOnly = false)
    {
        foreach (var milestone in milestones)
        {
            if (milestone != null)
                milestone.Activate();
        }
    }

    private void SuspendAllActivities()
    {
        foreach (var milestone in milestones)
        {
            if (milestone != null)
                milestone.Deactivate();
        }
    }

    // ======================================================
    // Timers
    // ======================================================

    public bool SetTimer(string name, float duration)
    {
        _timers[name] = duration;
        return true;
    }

    public void KillTimer(string name)
    {
        _timers.Remove(name);
    }

    private void UpdateTimers(float deltaTime)
    {
        if (_timers.Count == 0) return;

        var keys = ListPool<string>.Get();
        keys.AddRange(_timers.Keys);

        foreach (var key in keys)
        {
            _timers[key] -= deltaTime;
            if (_timers[key] <= 0f)
            {
                OnTimerExpired(key);
                _timers.Remove(key);
            }
        }

        ListPool<string>.Release(keys);
    }

    private void OnTimerExpired(string timerName)
    {
        Debug.Log($"Timer expired: {timerName}");
    }

    // ======================================================
    // SMS System
    // ======================================================

    public void AddSMS(int smsID)
    {
        _pendingSMS.Enqueue(smsID);
    }

    private void UpdatePendingSMS()
    {
        if (_pendingSMS.Count == 0) return;
        if (Time.time - _lastSMSTime < 2f) return;

        DispatchSMSMessage(_pendingSMS.Dequeue());
        _lastSMSTime = Time.time;
    }

    private void DispatchSMSMessage(int smsID)
    {
        Debug.Log($"Dispatching SMS ID: {smsID}");
    }
}

public class SpeedTrap : MonoBehaviour
{
    public bool IsActive { get; private set; }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

public class Milestone : MonoBehaviour
{
    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}

public abstract class RuntimeInstance : MonoBehaviour
{
    [field: SerializeField] public int Key { get; private set; }

    protected virtual void OnEnable()
    {
        GManager.Instance.RegisterInstance(this);
    }

    protected virtual void OnDisable()
    {
        GManager.Instance.UnregisterInstance(this);
    }
}

