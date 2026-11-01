using System.Collections;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class CameraAccessTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string[] permissions = { Permission.Camera, "horizonos.permission.HEADSET_CAMERA" };
        Permission.RequestUserPermissions(permissions);

        yield return new WaitForSeconds(8f);
        foreach (var perm in permissions)
            Debug.Log($"[CamAccTest] Permission {perm}: {Permission.HasUserAuthorizedPermission(perm)}");
#endif
        var devices = WebCamTexture.devices;
        Debug.Log($"[CamAccTest] Number of Cameras: {devices.Length}");

        foreach (var d in devices)
            Debug.Log($"[CamAccTest] Found Camera: {d.name}");

        yield break;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
