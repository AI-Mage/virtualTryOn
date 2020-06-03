using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using SimpleJSON;


public class poseModel : MonoBehaviour
{
    //public Transform Body;
    // Start is called before the first frame update
    Vector3[][] x;
    int i = 1;
    Pose[] pose;
    Pose oldPose = new Pose();
    public int FrameRate = 35;
    public string jsonFile_notWorking;
    public float ScalingFactor = -1f;
    public bool is2D = true;
    float zC;

    // Update is called once per frame
    void Start()
    {
        Application.targetFrameRate = FrameRate;
        //pose = loadPose("testfile.json");
        //pose = loadPose("jumpingJacks2.json", "root_JumpingJacks_");
        pose = loadPose("jumpingJack.json", "root_Jumping Jacks (2)_");

    }

    void Update()
    {        
        float k = ScalingFactor;
        //double magRange = 0.1;
        if (i < pose.Length)
        {

            
            if (i > 1)
            {
                if ((pose[i].RL_Target.y < 0)) 
                    return;


                if ((pose[i].LL_Target.y < 0))
                    return;

            }
            oldPose = pose[i];


            if (is2D)
            {
                zC = 0f;
                //Fixing z components of all joints to a single value i.e close to zero
                pose[i].Hip.z = zC;// pose.Hip.z * (-1);
                pose[i].RLeg.z = zC;//pose.RLeg.z * (-1);
                pose[i].RL_Hint.z = zC;//pose.RL_Hint.z * (-1);
                pose[i].RL_Target.z = zC;//pose.RL_Target.z * (-1);
                pose[i].LLeg.z = zC;//pose.LLeg.z * (-1);
                pose[i].LL_Hint.z = zC;//pose.LL_Hint.z * (-1);
                pose[i].LL_Target.z = zC;//pose.LL_Target.z * (-1);
                pose[i].Spine.z = zC;//pose.Spine.z * (-1);
                pose[i].Thorax.z = zC;//pose.Thorax.z * (-1);
                pose[i].Head_IK.z = zC;//pose.Head_IK.z * (-1);
                pose[i].LHand.z = zC;//pose.LHand.z * (-1);
                pose[i].LH_Hint.z = zC;//pose.LH_Hint.z * (-1);
                pose[i].LH_Target.z = zC;//pose.LH_Target.z * (-1);
                pose[i].RHand.z = zC;//pose.RHand.z * (-1);
                pose[i].RH_Hint.z = zC;//pose.RH_Hint.z * (-1);
                pose[i].RH_Target.z = zC;//pose.RH_Target.z * (-1);
            }
            
                
           


            Transform[] allChildren = GetComponentsInChildren<Transform>();

            foreach (Transform part in allChildren)
            {
                
                if (part.transform.name == "Hip")
                    part.transform.position = pose[i].Hip * k;
                if (part.transform.name == "Head_IK")
                    part.transform.position = pose[i].Head_IK * k;
                if (part.transform.name == "RL_Target")
                    part.transform.position = pose[i].RL_Target * k;
                if (part.transform.name == "RL_Hint")
                    part.transform.position = pose[i].RL_Hint * k;
                if (part.transform.name == "LL_Target")
                    part.transform.position = pose[i].LL_Target * k;
                if (part.transform.name == "LL_Hint")
                    part.transform.position = pose[i].LL_Hint * k;
                if (part.transform.name == "LH_Target")
                    part.transform.position = pose[i].LH_Target * k;
                if (part.transform.name == "LH_Hint")
                    part.transform.position = pose[i].LH_Hint * k;
                if (part.transform.name == "RH_Target")
                    part.transform.position = pose[i].RH_Target * k;
                if (part.transform.name == "RH_Hint")
                    part.transform.position = pose[i].RH_Hint * k;
            }
            i++;

        }
        else
            return;
     
    }


    public Pose[] loadPose(string dataFileName, string video)
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
        jsonData = jsonData.Replace("\"","");
        jsonData = jsonData.Replace('\'', '\"');
        
        
        var N = JSON.Parse(jsonData);
        Debug.Log("Total number of frames = " + N[video].Count);
        Pose[] pose1 = new Pose[N[video].Count+1];
        for (int j = 1; j<= N[video].Count; j++) {
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
        return pose1;
    }
}

// Commented out code that may be needed later

//Debug.Log(jsonData);


//Output pose data for debug purposes
//Debug.Log(pose1[j].Hip);
//Debug.Log(pose1[j].RLeg);
//Debug.Log(pose1[j].RL_Hint);
//Debug.Log(pose1[j].RL_Target);
//Debug.Log(pose1[j].LLeg);
//Debug.Log(pose1[j].LL_Hint);
//Debug.Log(pose1[j].LL_Target);
//Debug.Log(pose1[j].Spine);
//Debug.Log(pose1[j].Thorax);
//Debug.Log(pose1[j].Head_IK);
//Debug.Log(pose1[j].LHand);
//Debug.Log(pose1[j].LH_Hint);
//Debug.Log(pose1[j].LH_Target);
//Debug.Log(pose1[j].RHand);
//Debug.Log(pose1[j].RH_Hint);
//Debug.Log(pose1[j].RH_Target);



//if ((pose[i].LH_Target - oldPose.LH_Target).magnitude < (pose[i].LH_Target).magnitude ) { }
////pose[i].LH_Target = oldPose.LH_Target;
//else
//    return;

//if ((pose[i].RH_Target - oldPose.RH_Target).magnitude < (pose[i].RH_Target).magnitude ) { }
////pose[i].RH_Target = oldPose.RH_Target;
//else
//    return;
