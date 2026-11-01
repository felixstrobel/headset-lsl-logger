using UnityEngine;

public class CalibrationBoard : MonoBehaviour
{
    [SerializeField] OVRCameraRig rig;
    [SerializeField] Material boardMaterial;       // URP/Unlit
    [SerializeField] int innerCornersX = 9;        // OpenCV pattern size (inner corners)
    [SerializeField] int innerCornersY = 6;
    [SerializeField] float squareSize = 0.08f;     // metres
    [SerializeField] float distance = 1.0f;
    [SerializeField] float secondsPerPosition = 8f;   // time to capture each position
    float nextSwitch;

    // Board centre as (azimuth, elevation) in degrees; together they cover the field of view
    static readonly Vector2[] Offsets = {
        new(0, 0), new(-20, 0), new(20, 0), new(0, 15), new(0, -15),
        new(-20, 15), new(20, 15), new(-20, -15), new(20, -15)
    };

    Transform board;
    int offsetIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rig == null) rig = FindFirstObjectByType<OVRCameraRig>();

        // Checkerboard texture with a one-square white border (needed by OpenCV's detector)
        int sx = innerCornersX + 1, sy = innerCornersY + 1;
        int w = sx + 2, h = sy + 2;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false) { filterMode = FilterMode.Point };
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool inside = x >= 1 && x < w - 1 && y >= 1 && y < h - 1;
                bool black = inside && ((x + y) % 2 == 0);
                tex.SetPixel(x, y, black ? Color.black : Color.white);
            }
        tex.Apply();

        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(quad.GetComponent<Collider>());
        var mat = new Material(boardMaterial) { mainTexture = tex };
        quad.GetComponent<Renderer>().sharedMaterial = mat;
        quad.transform.localScale = new Vector3(w * squareSize, h * squareSize, 1f);

        board = quad.transform;
        board.SetParent(rig.leftEyeAnchor, false);   // head-locked to the left eye camera
        PlaceBoard();
        nextSwitch = Time.time + secondsPerPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextSwitch)
        {
            offsetIndex = (offsetIndex + 1) % Offsets.Length;
            PlaceBoard();
            nextSwitch = Time.time + secondsPerPosition;
        }
    }

    void PlaceBoard()
    {
        var o = Offsets[offsetIndex];
        var rot = Quaternion.Euler(-o.y, o.x, 0f);              // yaw = azimuth, pitch = -elevation
        board.localRotation = rot;                                // board faces the eye
        board.localPosition = rot * Vector3.forward * distance;
    }
}
