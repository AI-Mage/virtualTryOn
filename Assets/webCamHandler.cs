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

    public IEnumerator Post(string url, Byte[] bytes, System.Action<Pose> callBack, bool extrapolate)
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
                pose = Pose.loadPose(Encoding.ASCII.GetString(w.downloadHandler.data), "root_xyz");
                callBack(pose);
            }
           
        }
    }
}