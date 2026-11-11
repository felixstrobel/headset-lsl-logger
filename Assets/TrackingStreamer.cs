using UnityEngine;
using LSL;

public class QuestTrackingStreamer : MonoBehaviour
{
    [SerializeField] OVRCameraRig rig;

    static readonly string[] Labels = {
        // Pose in tracking space (Unity: left-handed, y top, z front)
        "head_px","head_py","head_pz","head_qx","head_qy","head_qz","head_qw",
        "leye_px","leye_py","leye_pz","leye_qx","leye_qy","leye_qz","leye_qw",
        "reye_px","reye_py","reye_pz","reye_qx","reye_qy","reye_qz","reye_qw",
        // Gaze per eye: pose (view direction = rotation * Vector3.forward), confidence, valid
        "lgaze_px","lgaze_py","lgaze_pz","lgaze_qx","lgaze_qy","lgaze_qz","lgaze_qw","lgaze_conf","lgaze_valid",
        "rgaze_px","rgaze_py","rgaze_pz","rgaze_qx","rgaze_qy","rgaze_qz","rgaze_qw","rgaze_conf","rgaze_valid",
        "gaze_time_ovr","frame"
    };

    StreamOutlet trackingOutlet;
    readonly double[] sample = new double[Labels.Length];
    OVRPlugin.EyeGazesState gazes;
    float nextStartAttempt;

    void Start()
    {
        if (rig == null) rig = FindFirstObjectByType<OVRCameraRig>();

        var info = new StreamInfo("QuestTracking", "Tracking", Labels.Length,
            LSL.LSL.IRREGULAR_RATE, channel_format_t.cf_double64, "questpro_tracking");
        var chans = info.desc().append_child("channels");
        foreach (var l in Labels) chans.append_child("channel").append_child_value("label", l);
        info.desc().append_child_value("coordinates", "unity_lefthanded_tracking_space");

        trackingOutlet = new StreamOutlet(info, 1);
    }

    void Update()
    {
        // Start eye tracking as soon as permissions granted
        if (!OVRPlugin.eyeTrackingEnabled && Time.time > nextStartAttempt)
        {
            OVRPlugin.StartEyeTracking();
            nextStartAttempt = Time.time + 1f;
        }

        int i = 0;

        WritePose(ref i, rig.centerEyeAnchor.localPosition, rig.centerEyeAnchor.localRotation);
        WritePose(ref i, rig.leftEyeAnchor.localPosition, rig.leftEyeAnchor.localRotation);
        WritePose(ref i, rig.rightEyeAnchor.localPosition, rig.rightEyeAnchor.localRotation);

        bool ok = OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref gazes)
                  && gazes.EyeGazes != null;
        for (int e = 0; e < 2; e++)
        {
            if (ok)
            {
                var g = gazes.EyeGazes[e];
                var p = g.Pose.ToOVRPose();
                WritePose(ref i, p.position, p.orientation);
                sample[i++] = g.Confidence;
                sample[i++] = g.IsValid ? 1 : 0;
            }
            else
            {
                for (int k = 0; k < 9; k++) sample[i++] = double.NaN;
            }
        }
        sample[i++] = ok ? gazes.Time : double.NaN;
        sample[i++] = Time.frameCount;

        trackingOutlet.push_sample(sample, LSL.LSL.local_clock());
    }

    void WritePose(ref int i, Vector3 p, Quaternion q)
    {
        sample[i++] = p.x; sample[i++] = p.y; sample[i++] = p.z;
        sample[i++] = q.x; sample[i++] = q.y; sample[i++] = q.z; sample[i++] = q.w;
    }
}