using UnityEngine;

namespace HideAndSkull.Character
{
    [RequireComponent(typeof(BoxCollider))]
    public class Sword : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[{nameof(Sword)}] TriggerEnter :: {other.name}");
            if (other.TryGetComponent(out Skull skull))
            {
                skull.Die();
            }
        }
    }
}