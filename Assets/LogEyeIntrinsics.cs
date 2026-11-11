using UnityEngine;
using UnityEngine.XR;

// Logs eye projection after XR is fully initialized, plus OVR's per-eye FOV for cross-checking.
public class LogEyeIntrinsics : MonoBehaviour
{
    int frame;
    bool done;

    void Update()
    {
        if (done || ++frame < 120) return;
        done = true;

        var cam = GetComponent<Camera>();
        Matrix4x4 P = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
        int W = XRSettings.eyeTextureWidth;
        int H = XRSettings.eyeTextureHeight;

        float fx = P[0, 0] * W / 2f;
        float fy = P[1, 1] * H / 2f;
        float cx = W / 2f * (1f - P[0, 2]);
        float cy = H / 2f * (1f + P[1, 2]);
        Debug.Log($"[EyeK] W={W} H={H} fx={fx} fy={fy} cx={cx} cy={cy} P02={P[0, 2]} P12={P[1, 2]}");

        // Per-eye FOV in degrees as reported by the Oculus runtime
        var fov = OVRManager.display.GetEyeRenderDesc(XRNode.LeftEye).fullFov;
        Debug.Log($"[EyeK] OVR fov up={fov.UpFov} down={fov.DownFov} left={fov.LeftFov} right={fov.RightFov}");
    }
}