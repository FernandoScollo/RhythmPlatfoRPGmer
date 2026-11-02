using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class AudioSnapshotZone : MonoBehaviour
{
    [SerializeField] private AudioSnapshotManager manager;
    [SerializeField] private Transform player;
    [SerializeField] private string snapshotId = "PlayerOnWater";
    private readonly HashSet<Collider2D> occupants = new HashSet<Collider2D>();
    private BoxCollider2D zoneCollider;

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void Awake()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;
        if (manager == null) manager = FindObjectOfType<AudioSnapshotManager>();
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) { Register(other); }
    private void OnTriggerStay2D(Collider2D other) { Register(other); }

    private void Register(Collider2D other)
    {
        if (!isActiveAndEnabled || player == null || manager == null) return;
        if (other.transform != player && !other.transform.IsChildOf(player)) return;
        occupants.Add(other);
        manager.RequestSnapshot(this, snapshotId);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        occupants.Remove(other);
        if (occupants.Count == 0 && manager != null) manager.ReleaseSnapshot(this);
    }

    private void FixedUpdate()
    {
        // Disabled/destroyed colliders may not deliver an exit callback.
        if (!zoneCollider.enabled)
            occupants.Clear();
        else
            occupants.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy ||
                !zoneCollider.bounds.Intersects(c.bounds));
        if (occupants.Count == 0 && manager != null) manager.ReleaseSnapshot(this);
    }

    private void OnDisable()
    {
        occupants.Clear();
        if (manager != null) manager.ReleaseSnapshot(this);
    }
}
