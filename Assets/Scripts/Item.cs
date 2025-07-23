using UnityEngine;
using Interact;
using Unity.VisualScripting;
public class Item : MonoBehaviour, IPushable
{
    private void OnCollisionStay(Collision other) {
        if (other.gameObject.CompareTag("Player"))
        {
            //Todo Calculate the vector the player is pushing from
            Rigidbody rb = this.GetComponent<Rigidbody>();
            if (rb == null) return;

            //Todo Null Check
            Vector3 pushVector = this.transform.position - other.collider.bounds.center;
            pushVector.y = 0f;
            //  Debug.Log(pushVector);
            Debug.DrawRay(other.collider.bounds.center, pushVector.normalized * 2f, Color.green);
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.AddForce(pushVector.normalized * 2f, ForceMode.Impulse);
            Debug.Log($"Actor: {other.gameObject.name} push me: {this.name}");
        }
    }
    void FixedUpdate()
    {
        
    }

}

namespace Interact
{
    interface IPushable
    {

    }
}
