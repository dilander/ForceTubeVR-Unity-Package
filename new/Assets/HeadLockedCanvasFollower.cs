using UnityEngine;

public class HeadLockedCanvasFollower : MonoBehaviour
{
    [Header("Target (HMD Camera)")]
    public Transform head;  // XR Origin の Main Camera を入れる

    [Header("Placement")]
    public float distance = 1.2f;
    public float heightOffset = -0.05f;

    [Header("Follow Behavior")]
    public float followAngle = 25f;     // 視界中心からこれ以上ズレたら追従開始
    public float positionLerp = 12f;    // 追従の滑らかさ
    public float rotationLerp = 12f;

    void Reset()
    {
        // 自動でMainCameraを探す（あれば）
        var cam = Camera.main;
        if (cam) head = cam.transform;
    }

    void LateUpdate()
    {
        if (!head) return;

        // 目標位置（頭の前方）
        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.001f) forward = head.forward;

        Vector3 targetPos = head.position + forward * distance + Vector3.up * heightOffset;

        // いまUIがどれだけ視界中心からズレてるか
        Vector3 toUI = (transform.position - head.position).normalized;
        float angle = Vector3.Angle(forward, Vector3.ProjectOnPlane(toUI, Vector3.up).normalized);

        // ズレが大きい時だけ追従させる（酔いにくい）
        if (angle > followAngle)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, 1f - Mathf.Exp(-positionLerp * Time.deltaTime));

            // UIは常に頭の方を向く（水平のみ）
            Vector3 lookDir = (head.position - transform.position);
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(-lookDir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-rotationLerp * Time.deltaTime));
            }
        }
    }
}
