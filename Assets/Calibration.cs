using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.XR;

public class Calibration : MonoBehaviour
{
    [SerializeField] OVRCameraRig rig;
    [SerializeField] Transform board;                         // quad, child of LeftEyeAnchor
    [SerializeField] bool runCalibration = true;              // untick for study sessions
    [SerializeField] float startDelay = 10f;                  // time to set the headset down and start the capture
    [SerializeField] float holdTime = 5f;                     // board visible per position
    [SerializeField] float gapTime = 1f;                      // board hidden between positions (segments the recording)
    [SerializeField] Vector2 shiftStep = new(0.05f, 0.05f);   // half a square, metres

    int pos;
    Vector3 basePos;

    [System.Serializable]
    class CalibrationPose
    {
        public int index;
        public float[] projLeft = new float[16];  // left-eye projection matrix, row-major
        public int eyeWidth, eyeHeight;           // eye buffer resolution
        public Vector3 boardPos;                  // board pose in left-eye space
        public Quaternion boardRot;
        public Vector3 boardScale;
        public float ipd;                         // metres, distance between eye anchors
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        board.gameObject.SetActive(false);
        if (!runCalibration) yield break;

        Vector3 basePos = board.localPosition;

        // Full resolution across the whole eye buffer while calibrating (sharper corners at the periphery)
        OVRManager.foveatedRenderingLevel = OVRManager.FoveatedRenderingLevel.Off;
        OVRManager.useDynamicFoveatedRendering = false;
        OVRManager.eyeTrackedFoveatedRenderingEnabled = false;

        yield return new WaitForSeconds(startDelay);

        for (int pos = 0; pos < 9; pos++)
        {
            // 3x3 grid of half-square shifts so corners densely cover the field of view
            board.localPosition = basePos + new Vector3((pos % 3) * shiftStep.x, (pos / 3) * shiftStep.y, 0f);
            WritePose(pos);

            board.gameObject.SetActive(true);
            yield return new WaitForSeconds(holdTime);
            board.gameObject.SetActive(false);
            yield return new WaitForSeconds(gapTime);
        }
        board.localPosition = basePos;
        Debug.Log("[Calib] done, board hidden");
    }

    void WritePose(int pos)
    {
        var cam = rig.centerEyeAnchor.GetComponent<Camera>();
        Matrix4x4 p = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
        var c = new CalibrationPose
        {
            index = pos,
            eyeWidth = XRSettings.eyeTextureWidth,
            eyeHeight = XRSettings.eyeTextureHeight,
            boardPos = board.localPosition,
            boardRot = board.localRotation,
            boardScale = board.localScale,
            ipd = Vector3.Distance(rig.leftEyeAnchor.position, rig.rightEyeAnchor.position),
        };
        for (int r = 0; r < 4; r++)
            for (int col = 0; col < 4; col++)
                c.projLeft[r * 4 + col] = p[r, col];

        string path = Path.Combine(Application.persistentDataPath, $"calib_pose_{pos}.json");
        File.WriteAllText(path, JsonUtility.ToJson(c, true));
    }
}
