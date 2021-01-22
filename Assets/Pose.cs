using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SimpleJSON;
using System.IO;
using Unity.Mathematics;
//using System.Numerics;

[System.Serializable]
public class Pose : MonoBehaviour
{
    public Vector3 Hip;
    public Vector3 RLeg;
    public Vector3 RL_Hint;
    public Vector3 RL_Target;
    public Vector3 LLeg;
    public Vector3 LL_Hint;
    public Vector3 LL_Target;
    public Vector3 Spine;
    public Vector3 Thorax;
    //public Vector3 Neck;
    public Vector3 Head_IK;
    public Vector3 LHand;
    public Vector3 LH_Hint;
    public Vector3 LH_Target;
    public Vector3 RHand;
    public Vector3 RH_Hint;
    public Vector3 RH_Target;

    private int frameCount;
    public struct UnityHuman
    {
        internal static Vector3 RL_Target;
        internal static Vector3 RL_Hint;
        internal static Vector3 LL_Target;
        internal static Vector3 LL_Hint;
        internal static Vector3 RHip;
        internal static Vector3 LH_Target;
        internal static Vector3 LH_Hint;
        internal static Vector3 RH_Target;
        internal static Vector3 RH_Hint;
        internal static Vector3 RShoulder;
        internal static Vector3 Head_IK;
    }
    public struct scale
    {
        internal static float sc_RL_Target;
        internal static float sc_RL_Hint;
        internal static float sc_LL_Target;
        internal static float sc_LL_Hint;
        internal static float sc_RHip;
        internal static float sc_LH_Target;
        internal static float sc_LH_Hint;
        internal static float sc_RH_Target;
        internal static float sc_RH_Hint;
        internal static float sc_RShoulder;
        internal static float sc_Head_IK;
    }
    public float k; //scaling factor

   
    //public Pose(List<Vector3> humanJoints)
    //{
    //    //foreach(Vector3 joint in humanJoints)
    //    UnityHuman.RL_Target = humanJoints[1];
    //    UnityHuman.RL_Hint = humanJoints[2];
    //    UnityHuman.LL_Target = humanJoints[3];
    //    UnityHuman.LL_Hint = humanJoints[4];
    //    UnityHuman.RHip = humanJoints[5];
    //    UnityHuman.LH_Target = humanJoints[6];
    //    UnityHuman.LH_Hint = humanJoints[7];
    //    UnityHuman.RH_Target = humanJoints[8];
    //    UnityHuman.RH_Hint = humanJoints[9];
    //    UnityHuman.RShoulder = humanJoints[10];
    //    UnityHuman.Head_IK = humanJoints[11];
    //}

    public void Ensure2D()
    {
        //Fixing z components of all joints to a single value i.e zero

        Hip.z = 0;
        RLeg.z = 0;
        RL_Hint.z = 0;
        RL_Target.z = 0;
        LLeg.z = 0;
        LL_Hint.z = 0;
        LL_Target.z = 0;
        Spine.z = 0;
        Thorax.z = 0;
        Head_IK.z = 0;
        LHand.z = 0;
        LH_Hint.z = 0;
        LH_Target.z = 0;
        RHand.z = 0;
        RH_Hint.z = 0;
        RH_Target.z = 0;
    }

    
    public static Pose loadPose(string jsonData, string video)
    {
        jsonData = jsonData.Replace("\"", "");
        jsonData = jsonData.Replace('\'', '\"');
        var N = JSON.Parse(jsonData);
        Debug.Log(N);
        Pose pose1;

        pose1 = new Pose();
        pose1.Hip = N[video]["pelvis"]["t"].AsArray;
        pose1.RLeg = N[video]["pelvis"]["r_hip"]["t"].AsArray;
        pose1.RL_Hint = N[video]["pelvis"]["r_hip"]["r_knee"]["t"].AsArray;
        pose1.RL_Target = N[video]["pelvis"]["r_hip"]["r_knee"]["r_ankle"]["t"].AsArray;
        pose1.LLeg = N[video]["pelvis"]["l_hip"]["t"].AsArray;
        pose1.LL_Hint = N[video]["pelvis"]["l_hip"]["l_knee"]["t"].AsArray;
        pose1.LL_Target = N[video]["pelvis"]["l_hip"]["l_knee"]["l_ankle"]["t"].AsArray;
        pose1.Spine = N[video]["pelvis"]["spine"]["t"].AsArray;
        pose1.Thorax = N[video]["pelvis"]["spine"]["neck"]["t"].AsArray;
        //pose1.Hip Neck = N[video]["pelvis"]["t"].AsArray;Debug.Log(pose1.Hip);
        pose1.Head_IK = N[video]["pelvis"]["spine"]["neck"]["head"]["head_top"]["t"].AsArray;
        pose1.LHand = N[video]["pelvis"]["spine"]["neck"]["l_shoulder"]["t"].AsArray;
        pose1.LH_Hint = N[video]["pelvis"]["spine"]["neck"]["l_shoulder"]["l_elbow"]["t"].AsArray;
        pose1.LH_Target = N[video]["pelvis"]["spine"]["neck"]["l_shoulder"]["l_elbow"]["l_wrist"]["t"].AsArray;
        pose1.RHand = N[video]["pelvis"]["spine"]["neck"]["r_shoulder"]["t"].AsArray;
        pose1.RH_Hint = N[video]["pelvis"]["spine"]["neck"]["r_shoulder"]["r_elbow"]["t"].AsArray;
        pose1.RH_Target = N[video]["pelvis"]["spine"]["neck"]["r_shoulder"]["r_elbow"]["r_wrist"]["t"].AsArray;
        return pose1;
    }

    public static Pose Extrapolate(Pose oldPose1, Pose oldPose2, float t1, float t2)
    {
        Debug.Log(oldPose1.Hip + " " + oldPose2.Hip + " " + t1 + " " + t2);
        Pose newPose = new Pose();
        newPose.Hip = LinearExtrapolate(oldPose1.Hip, oldPose2.Hip, t1, t2);
        newPose.RLeg = LinearExtrapolate(oldPose1.RLeg, oldPose2.RLeg, t1, t2);
        newPose.RL_Hint = LinearExtrapolate(oldPose1.RL_Hint, oldPose2.RL_Hint, t1, t2);
        newPose.RL_Target = LinearExtrapolate(oldPose1.RL_Target, oldPose2.RL_Target, t1, t2);
        newPose.LLeg = LinearExtrapolate(oldPose1.LLeg, oldPose2.LLeg, t1, t2);
        newPose.LL_Hint = LinearExtrapolate(oldPose1.LL_Hint, oldPose2.LL_Hint, t1, t2);
        newPose.LL_Target = LinearExtrapolate(oldPose1.LL_Target, oldPose2.LL_Target, t1, t2);
        newPose.Spine = LinearExtrapolate(oldPose1.Spine, oldPose2.Spine, t1, t2);
        newPose.Thorax = LinearExtrapolate(oldPose1.Thorax, oldPose2.Thorax, t1, t2);
        //pose1.Hip Neck = N[video][video1]["pelvis"]["t"].AsArray;Debug.Log(pose1.Hip);
        newPose.Head_IK = LinearExtrapolate(oldPose1.Head_IK, oldPose2.Head_IK, t1, t2);
        newPose.LHand = LinearExtrapolate(oldPose1.LHand, oldPose2.LHand, t1, t2);
        newPose.LH_Hint = LinearExtrapolate(oldPose1.LH_Hint, oldPose2.LH_Hint, t1, t2);
        newPose.LH_Target = LinearExtrapolate(oldPose1.LH_Target, oldPose2.LH_Target, t1, t2);
        newPose.RHand = LinearExtrapolate(oldPose1.RHand, oldPose2.RHand, t1, t2);
        newPose.RH_Hint = LinearExtrapolate(oldPose1.RH_Hint, oldPose2.RH_Hint, t1, t2);
        newPose.RH_Target = LinearExtrapolate(oldPose1.RH_Target, oldPose2.RH_Target, t1, t2);
        return newPose;
    }

    private static Vector3 LinearExtrapolate(Vector3 oldV1, Vector3 oldV2, float t1, float t2)
    {
        if((t2-t1) == 0)
        {
            return oldV2;
        }
        Vector3 ps = oldV1 + (t2 + Time.deltaTime - t1) / (t2 - t1) * (oldV2 - oldV1);
        return ps;

    }

    public static Pose[] LoadPoseFromFile(string dataFileName, string video, List<Vector3> humanJoints) //
    {
        string tempPath = Path.Combine(Application.dataPath, "");
        tempPath = Path.Combine(tempPath, dataFileName);

        byte[] jsonByte = null;
        try
        {
            jsonByte = File.ReadAllBytes(tempPath);
            Debug.Log("Loaded Data from: " + tempPath.Replace("/", "\\"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Load Data from: " + tempPath.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }

        //Convert to json string
        string jsonData = Encoding.ASCII.GetString(jsonByte);
        jsonData = jsonData.Replace("\"", "");
        jsonData = jsonData.Replace('\'', '\"');


        var N = JSON.Parse(jsonData);
        Debug.Log("Total number of frames = " + N[video].Count);
        Pose[] pose1 = new Pose[N[video].Count + 1];
        for (int j = 1; j <= N[video].Count; j++)
        {
            string video1 = video + j.ToString();
            pose1[j] = new Pose();
            pose1[j].Hip = N[video][video1]["pelvis"]["t"].AsArray;
            pose1[j].RLeg = N[video][video1]["pelvis"]["r_hip"]["t"].AsArray;
            pose1[j].RL_Hint = N[video][video1]["pelvis"]["r_hip"]["r_knee"]["t"].AsArray;
            pose1[j].RL_Target = N[video][video1]["pelvis"]["r_hip"]["r_knee"]["r_ankle"]["t"].AsArray;
            pose1[j].LLeg = N[video][video1]["pelvis"]["l_hip"]["t"].AsArray;
            pose1[j].LL_Hint = N[video][video1]["pelvis"]["l_hip"]["l_knee"]["t"].AsArray;
            pose1[j].LL_Target = N[video][video1]["pelvis"]["l_hip"]["l_knee"]["l_ankle"]["t"].AsArray;
            pose1[j].Spine = N[video][video1]["pelvis"]["spine"]["t"].AsArray;
            pose1[j].Thorax = N[video][video1]["pelvis"]["spine"]["neck"]["t"].AsArray;
            //pose1.Hip Neck = N[video][video1]["pelvis"]["t"].AsArray;Debug.Log(pose1.Hip);
            pose1[j].Head_IK = N[video][video1]["pelvis"]["spine"]["neck"]["head"]["head_top"]["t"].AsArray;
            pose1[j].LHand = N[video][video1]["pelvis"]["spine"]["neck"]["l_shoulder"]["t"].AsArray;
            pose1[j].LH_Hint = N[video][video1]["pelvis"]["spine"]["neck"]["l_shoulder"]["l_elbow"]["t"].AsArray;
            pose1[j].LH_Target = N[video][video1]["pelvis"]["spine"]["neck"]["l_shoulder"]["l_elbow"]["l_wrist"]["t"].AsArray;
            pose1[j].RHand = N[video][video1]["pelvis"]["spine"]["neck"]["r_shoulder"]["t"].AsArray;
            pose1[j].RH_Hint = N[video][video1]["pelvis"]["spine"]["neck"]["r_shoulder"]["r_elbow"]["t"].AsArray;
            pose1[j].RH_Target = N[video][video1]["pelvis"]["spine"]["neck"]["r_shoulder"]["r_elbow"]["r_wrist"]["t"].AsArray;
         }
        Pose.setPointsScale(pose1[5], humanJoints);
        return pose1;
    }

    public static void setPointsScale(Pose pose, List<Vector3> humanjoints)  //use incoming pose points in the first few frames to generate a scaling factor to multiply with the human joint positions
    {
        UnityHuman.RL_Target = humanjoints[0];
        UnityHuman.RL_Hint = humanjoints[1];
        UnityHuman.LL_Target = humanjoints[2];
        UnityHuman.LL_Hint = humanjoints[3];
        UnityHuman.RHip = humanjoints[4];
        UnityHuman.LH_Target = humanjoints[5];
        UnityHuman.LH_Hint = humanjoints[6];
        UnityHuman.RH_Target = humanjoints[7];
        UnityHuman.RH_Hint = humanjoints[8];
        UnityHuman.RShoulder = humanjoints[9];
        UnityHuman.Head_IK = humanjoints[10];
        scale.sc_RL_Target = pose.RL_Target.magnitude / UnityHuman.RL_Target.magnitude;
        scale.sc_RL_Hint = pose.RL_Hint.magnitude / UnityHuman.RL_Hint.magnitude;
        scale.sc_LL_Target = pose.LL_Target.magnitude / UnityHuman.LL_Target.magnitude;
        scale.sc_LL_Hint = pose.LL_Hint.magnitude - UnityHuman.LL_Hint.magnitude;
        scale.sc_RHip = pose.RLeg.magnitude / UnityHuman.RHip.magnitude;
        scale.sc_LH_Target = pose.LH_Target.magnitude / UnityHuman.LH_Target.magnitude;
        scale.sc_LH_Hint = pose.LH_Hint.magnitude / UnityHuman.LH_Hint.magnitude;
        scale.sc_RH_Target = pose.RH_Target.magnitude / UnityHuman.RH_Target.magnitude;
        scale.sc_RH_Hint = pose.RH_Hint.magnitude / UnityHuman.RH_Hint.magnitude;
        scale.sc_RShoulder = pose.RHand.magnitude / UnityHuman.RShoulder.magnitude;
        scale.sc_Head_IK = pose.Head_IK.magnitude / UnityHuman.Head_IK.magnitude;
    } // need to fix data handling between static methods and non static properties
}
//public Vector3 Hip;
//public Vector3 RLeg;
//public Vector3 RL_Hint;
//public Vector3 RL_Target;
//public Vector3 LLeg;
//public Vector3 LL_Hint;
//public Vector3 LL_Target;
//public Vector3 Spine;
//public Vector3 Thorax;
////public Vector3 Neck;
//public Vector3 Head_IK;
//public Vector3 LHand;
//public Vector3 LH_Hint;
//public Vector3 LH_Target;
//public Vector3 RHand;
//public Vector3 RH_Hint;
//public Vector3 RH_Target;

//private int frameCount;
//private struct UnityHuman
//{
//    Vector3 RL_Target;
//    Vector3 RL_Hint;
//    Vector3 LL_Target;
//    Vector3 LL_Hint;
//    Vector3 RHip;
//    Vector3 LH_Target;
//    Vector3 LH_Hint;
//    Vector3 RH_Target;
//    Vector3 RH_Hint;
//    Vector3 RShoulder;
//    Vector3 Head_IK;
//}
//public float k; //scaling factor

