using UnityEngine;
using UnityEngine.Rendering;
public class PlayerMovement : MovmementBase
{
    [SerializeField] private Camera _cam;  

    public override void HandleMove(Vector3 moveVec)
    {
        base.HandleMove(moveVec);
    }

    public Vector3 GetInputVector(Vector2 input)
    {
        //1: Flatten camera forward/right to the XZ plane
        Transform camT = _cam.transform;
        Vector3 camForward = camT.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = camT.right;
        camRight.y = 0;
        camRight.Normalize();
        //2: Build world-space move vector
        return camForward * input.y + camRight * input.x;
    }
}
