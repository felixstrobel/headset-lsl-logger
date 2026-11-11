using UnityEngine;

public class FrameMarker : MonoBehaviour
{
    public Transform anchor;          // CenterEyeAnchor
    public Material onMat, offMat;    // white / black, unlit
    public int bits = 12;             // frame number modulo 4096
    public float yawDeg = -42f;       // horizontal angle of the column (negative = left)
    public float cellDeg = 2.5f;      // angular size per cell
    public float distance = 1f;       // meters in front of the eye

    Renderer[] cells;

    void Start()
    {
        cells = new Renderer[bits];
        for (int i = 0; i < bits; i++)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(q.GetComponent<Collider>());
            q.transform.SetParent(anchor, false);

            // Bit 0 at the bottom, highest bit at the top
            float pitch = (i - (bits - 1) / 2f) * cellDeg;
            Vector3 dir = Quaternion.Euler(-pitch, yawDeg, 0f) * Vector3.forward;
            q.transform.localPosition = dir * distance;
            q.transform.localRotation = Quaternion.LookRotation(dir);
            float s = 2f * distance * Mathf.Tan(0.45f * cellDeg * Mathf.Deg2Rad); // small gap between cells
            q.transform.localScale = new Vector3(s, s, 1f);

            cells[i] = q.GetComponent<Renderer>();
        }
    }

    void LateUpdate()
    {
        int f = Time.frameCount;
        for (int i = 0; i < bits; i++)
            cells[i].sharedMaterial = ((f >> i) & 1) == 1 ? onMat : offMat;
    }
}
