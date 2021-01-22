using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using SimpleJSON;
using UnityEditor;
using UnityEngine.UI;

public class UpdatePose : MonoBehaviour
{

    public int FrameRate = 60;
    public float ScalingFactor = -1f;
    public bool is2D = false;
    public bool ExtrapolatePoints = false;
    public string URL = "localhost:8097";
    WebCamTexture webcamTexture;
    Texture2D tex;
    private float update= 0 ;
    private Transform Hip_Target, RLeg_Constraint, Hip_Rotation, RHip, Head_IK, RL_Target, RL_Hint, LL_Target, LL_Hint, LH_Target, LH_Hint, RH_Target, RH_Hint, RShoulder, RHand_Constraint;
	public float SpineRotationTolerance = 0.1f;
	public float HipRotationTolerance = 0.1f;
	Pose oldPose1, oldPose2;
	Pose pose;
	float t1, t2;

	public RawImage img;
	void Start()
    {
		// Cache the desired joints of the body into Transform variables
		#region 
		Hip_Target = GameObject.Find("Hip_Target").transform;
		RLeg_Constraint = GameObject.Find("RLeg_Constraint").transform;
		Hip_Rotation = GameObject.Find("Hip_Rotation").transform;
		RHip = GameObject.Find("RHip").transform;
		Head_IK = GameObject.Find("Head_IK").transform;
		RL_Target = GameObject.Find("RL_Target").transform;
		RL_Hint = GameObject.Find("RL_Hint").transform;
		LL_Target = GameObject.Find("LL_Target").transform;
		LL_Hint = GameObject.Find("LL_Hint").transform;
		LH_Target = GameObject.Find("LH_Target").transform;
		LH_Hint = GameObject.Find("LH_Hint").transform;
		RH_Target = GameObject.Find("RH_Target").transform;
		RH_Hint = GameObject.Find("RH_Hint").transform;
		RShoulder = GameObject.Find("RShoulder").transform;
		RHand_Constraint = GameObject.Find("RHand_Constraint").transform;
		#endregion

		// Start web cam feed
		webcamTexture = new WebCamTexture();
        webcamTexture.Play();
        //update = 1000f;
        tex = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
        

        //Application.Quit();
    }
	private void Update()
	{
		Application.targetFrameRate = FrameRate;
        update = Time.frameCount;
        if (ExtrapolatePoints && Time.time > 3)
		{
			Debug.Log("Extrapolating points");
			try
			{

				pose = Pose.Extrapolate(oldPose1, oldPose2, t1, t2);
				oldPose1 = oldPose2;
				t1 = t2;
				t2 += Time.deltaTime;
				oldPose2 = pose;
				playPose(pose);
			}
			catch (NullReferenceException)
			{
				Debug.Log("Need more points to extrapolate");
				oldPose1 = oldPose2;
				t1 = t2;
				t2 += Time.deltaTime;

			}
		}
		if (update % 4 == 0)
		{
            //update = 0.0f;
            tex.SetPixels(webcamTexture.GetPixels());
			tex.Apply();
			EditorUtility.CompressTexture(tex, TextureFormat.RGB24, 0);
			img.texture = tex;
			byte[] bytes = tex.EncodeToPNG();
			Debug.Log("Request sent");
			StartCoroutine(webCamHandler.Instance.Post(URL, bytes, playPose, ExtrapolatePoints));
			
			
		}

	}
	void playPose(Pose pose)
	{
		float k = ScalingFactor;
		//double magRange = 0.1;

		if (is2D)
			pose.Ensure2D();
		
		floorLimit = RL_Target.position.y;
		Hip_Target.position = pose.Hip * k;
		//if (k * pose.RLeg.magnitude >= RHip.position.magnitude)// &&
            //k * pose.RLeg.magnitude < (1 + HipRotationTolerance) * RHip.position.magnitude)
        RLeg_Constraint.position = pose.RLeg * k;
		Head_IK.position = pose.Head_IK * k;
		RL_Target.position = pose.RL_Target * k;
		LL_Target.position = pose.LL_Target * k;
        if (Hip_Target.position.y < 0)  // Fix for knee bending sitting postures.
        {
            RL_Target.SetPositionAndRotation(new Vector3(RL_Target.position.x, floorLimit - Hip_Target.position.y, RL_Target.position.z), RL_Target.rotation);
            LL_Target.SetPositionAndRotation(new Vector3(LL_Target.position.x, floorLimit - Hip_Target.position.y, LL_Target.position.z), LL_Target.rotation);
        }
		RL_Hint.position = pose.RL_Hint * k;
		LL_Hint.position = pose.LL_Hint * k;
		//if (k * pose.RHand.magnitude >= RShoulder.position.magnitude &&
			//k * pose.RHand.magnitude < (1 + SpineRotationTolerance) * RShoulder.position.magnitude)
			RHand_Constraint.position = pose.RHand * k;
		LH_Target.position = pose.LH_Target * k;
		LH_Hint.position = pose.LH_Hint * k;
		RH_Target.position = pose.RH_Target * k;
		RH_Hint.position = pose.RH_Hint * k;
		t2 = Time.time;
		Debug.Log("Received Coordinates");
		oldPose2 = pose;
	}

}
