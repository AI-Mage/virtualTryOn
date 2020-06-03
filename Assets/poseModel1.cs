using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using SimpleJSON;
using UnityEditor;

public class poseModel1 : MonoBehaviour
{
    //public Transform Body;
    // Start is called before the first frame update
    public int FrameRate = 60;
    public string jsonFile_notWorking;
    public float ScalingFactor = -1f;
    public bool is2D = true;
    float zC;
    public string URL = "localhost:8097";
    WebCamTexture webcamTexture;
    Texture2D tex;
    private float update ;
    // Update is called once per 2frame
    void Start()
    {
        // Start web cam feed
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();
        update = 1000.0f;
        tex = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
        Application.targetFrameRate = FrameRate;

        //Application.Quit();
    }
    private void Update()
    {
        //update += Time.deltaTime;
        if (update > FrameRate)
        {
            //update = 100.0f;
            tex.SetPixels(webcamTexture.GetPixels());
            tex.Apply();
            //EditorUtility.CompressTexture(tex, TextureFormat.RGB24, 0);

            byte[] bytes = tex.EncodeToPNG();
            Debug.Log("Request sent");
            StartCoroutine(webCamHandler.Instance.Post(URL, bytes, playPose));
        }

    }
    void playPose(Pose pose)
    {
        Debug.Log(pose.Hip);
        float k = ScalingFactor;
        //double magRange = 0.1;

        if (is2D)
        {
            zC = 0f;
            //Fixing z components of all joints to a single value i.e close to zero
            pose.Hip.z = zC;// pose.Hip.z * (-1);
            pose.RLeg.z = zC;//pose.RLeg.z * (-1);
            pose.RL_Hint.z = zC;//pose.RL_Hint.z * (-1);
            pose.RL_Target.z = zC;//pose.RL_Target.z * (-1);
            pose.LLeg.z = zC;//pose.LLeg.z * (-1);
            pose.LL_Hint.z = zC;//pose.LL_Hint.z * (-1);
            pose.LL_Target.z = zC;//pose.LL_Target.z * (-1);
            pose.Spine.z = zC;//pose.Spine.z * (-1);
            pose.Thorax.z = zC;//pose.Thorax.z * (-1);
            pose.Head_IK.z = zC;//pose.Head_IK.z * (-1);
            pose.LHand.z = zC;//pose.LHand.z * (-1);
            pose.LH_Hint.z = zC;//pose.LH_Hint.z * (-1);
            pose.LH_Target.z = zC;//pose.LH_Target.z * (-1);
            pose.RHand.z = zC;//pose.RHand.z * (-1);
            pose.RH_Hint.z = zC;//pose.RH_Hint.z * (-1);
            pose.RH_Target.z = zC;//pose.RH_Target.z * (-1);
        }





        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (Transform part in allChildren)
        {

            if (part.transform.name == "Hip_Target")
                part.transform.position = pose.Hip * k;
            if (part.transform.name == "RL_Constraint")
                part.transform.position = pose.RLeg * k;
            if (part.transform.name == "Head_IK")
                part.transform.position = pose.Head_IK * k;
            if (part.transform.name == "RL_Target")
                part.transform.position = pose.RL_Target * k;
            if (part.transform.name == "RL_Hint")
                part.transform.position = pose.RL_Hint * k;
            if (part.transform.name == "LL_Target")
                part.transform.position = pose.LL_Target * k;
            if (part.transform.name == "LL_Hint")
                part.transform.position = pose.LL_Hint * k;
            if (part.transform.name == "LH_Target")
                part.transform.position = pose.LH_Target * k;
            if (part.transform.name == "LH_Hint")
                part.transform.position = pose.LH_Hint * k;
            if (part.transform.name == "RH_Target")
                part.transform.position = pose.RH_Target * k;
            if (part.transform.name == "RH_Hint")
                part.transform.position = pose.RH_Hint * k;
        }
    }

}
