using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using UnityEngine.Networking;
using System.Text;
using SimpleJSON;
using System;

public class webCamHandler : MonoBehaviour
{
    Pose pose;
    /// <summary>
    /// Create instance of object
    /// </summary>
    private static webCamHandler _instance;
    public static webCamHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<webCamHandler>();
                if (_instance == null)
                {
                    GameObject go = new GameObject();
                    go.name = typeof(webCamHandler).Name;
                    _instance = go.AddComponent<webCamHandler>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public IEnumerator Post(string url, Byte[] bytes, System.Action<Pose> callBack)
    {


        // Create a Web Form
        WWWForm form = new WWWForm();
        //form.AddField("frameCount", Time.frameCount.ToString());
        form.AddBinaryData("fileUpload", bytes);

        // Upload to a cgi script
        var w = UnityWebRequest.Post(url, form);
        yield return w.SendWebRequest();

        if (w.isNetworkError || w.isHttpError)
        {
            Debug.Log(w.error);
        }
        else
        {
            if (w.isDone)
            {
                Debug.Log(w.downloadHandler.text);
                pose = loadPose(Encoding.ASCII.GetString(w.downloadHandler.data), "root_xyz");
                Debug.Log("Received Coordinates");
                callBack(pose);
            }
        }
    }
    public Pose loadPose(string jsonData, string video)
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

}