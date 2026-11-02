using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSnapshotManager : MonoBehaviour
{
    [Serializable]
    public class SnapshotSetting
    {
        public string id;
        public AudioMixerSnapshot snapshot;
        public int priority;
        [Min(0f)] public float transitionDuration = 0.3f;
    }

    [SerializeField] private AudioMixerSnapshot normalSnapshot;
    [SerializeField, Min(0f)] private float normalTransitionDuration = 0.3f;
    [SerializeField] private List<SnapshotSetting> snapshots = new List<SnapshotSetting>();

    private class Request
    {
        public UnityEngine.Object owner;
        public SnapshotSetting setting;
        public long order;
    }

    private readonly List<Request> requests = new List<Request>();
    private long nextOrder;
    private AudioMixerSnapshot current;
    private bool started;

    private void Start()
    {
        started = true;
        Refresh();
    }

    public bool RequestSnapshot(UnityEngine.Object owner, string id)
    {
        if (owner == null) return false;
        SnapshotSetting setting = snapshots.Find(s => s != null && s.id == id);
        if (setting == null || setting.snapshot == null)
        {
            Debug.LogWarning("Snapshot '{id}' is not configured.", this);
            return false;
        }
        Request existing = requests.Find(r => r.owner == owner);
        if (existing != null && existing.setting == setting) return true;
        requests.RemoveAll(r => r.owner == owner);
        requests.Add(new Request { owner = owner, setting = setting, order = ++nextOrder });
        Refresh();
        return true;
    }

    public void ReleaseSnapshot(UnityEngine.Object owner)
    {
        requests.RemoveAll(r => r.owner == owner);
        Refresh();
    }

    private void LateUpdate()
    {
        if (requests.RemoveAll(r => r.owner == null) > 0) Refresh();
    }

    private void OnEnable()
    {
        current = null;
        if (started) Refresh();
    }

    private void OnDisable()
    {
        requests.Clear();
        current = null;
        if (started && normalSnapshot != null)
            normalSnapshot.TransitionTo(Mathf.Max(0f, normalTransitionDuration));
    }

    private void Refresh()
    {
        if (!started || !isActiveAndEnabled) return;
        Request best = null;
        foreach (Request request in requests)
        {
            if (request.owner == null) continue;
            if (best == null || request.setting.priority > best.setting.priority ||
                (request.setting.priority == best.setting.priority && request.order > best.order))
                best = request;
        }
        AudioMixerSnapshot target = best == null ? normalSnapshot : best.setting.snapshot;
        if (target == null || target == current) return;
        float duration = best == null ? normalTransitionDuration : best.setting.transitionDuration;
        current = target;
        Debug.Log("Transition to: " + target.name);
        target.TransitionTo(Mathf.Max(0f, duration));
    }
}
