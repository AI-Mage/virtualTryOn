using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class poseModel : MonoBehaviour
{
    
    
    int i = 1;
    Pose[] pose;
    public int FrameRate = 30;
    public string jsonFile_notWorking;
    public float ScalingFactor = -0.795f;
    public bool is2D = true;
    public float SpineRotationTolerance = 0.1f;
    public float HipRotationTolerance = 0.1f;

    private Transform Hip_Target, RLeg_Constraint, Hip_Rotation, RHip, Head_IK, RL_Target, RL_Hint, LL_Target, LL_Hint, LH_Target, LH_Hint, RH_Target, RH_Hint, RShoulder;
    Pose oldie = null;

    void Start()
    {
        
        Application.targetFrameRate = FrameRate;
        //pose = Pose.LoadPoseFromFile("jumpingJack.json", "root_Jumping Jacks (2)_"); //scaling factor = -0.795
        pose = Pose.LoadPoseFromFile("Indian_man.json", "root_Indian man_");

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
        #endregion

    }

    private void Update()
    {
        //Debug.Log(GameObject.Find("Hip_Rotation").GetComponent<AimConstraint>().constraintActive);

        if (GameObject.Find("RLeg_Constraint").transform.position.magnitude < GameObject.Find("RHip").transform.position.magnitude ||
            GameObject.Find("RLeg_Constraint").transform.position.magnitude > GameObject.Find("RHip").transform.position.magnitude * 1.5)
            GameObject.Find("Hip_Rotation").GetComponent<AimConstraint>().constraintActive = false;
        else
            GameObject.Find("Hip_Rotation").GetComponent<AimConstraint>().constraintActive = true;

        if (GameObject.Find("RHand_Constraint").transform.position.magnitude < GameObject.Find("RShoulder").transform.position.magnitude ||
            GameObject.Find("RHand_Constraint").transform.position.magnitude > GameObject.Find("RShoulder").transform.position.magnitude * 1.1)
            GameObject.Find("Spine_Rotation").GetComponent<AimConstraint>().constraintActive = false;
        else
            GameObject.Find("Spine_Rotation").GetComponent<AimConstraint>().constraintActive = true;
    }

    //void Update()
    //{
    //    float k = ScalingFactor;
    //    if (i < pose.Length)
    //    {
    //        if (is2D)
    //            pose[i].Ensure2D();

    //        Hip_Target.position = pose[i].Hip * k;
    //        if (k * pose[i].RLeg.magnitude >= RHip.position.magnitude &&
    //            k * pose[i].RLeg.magnitude < (1+ HipRotationTolerance) * RHip.position.magnitude)
    //            RLeg_Constraint.position = pose[i].RLeg * k;
    //        Head_IK.position = pose[i].Head_IK * k;
    //        RL_Target.position = pose[i].RL_Target * k;
    //        RL_Hint.position = pose[i].RL_Hint * k;
    //        LL_Target.position = pose[i].LL_Target * k;
    //        LL_Hint.position = pose[i].LL_Hint * k;
    //        if (k * pose[i].RHand.magnitude >= RShoulder.position.magnitude &&
    //            k * pose[i].RHand.magnitude < (1+ SpineRotationTolerance) * RShoulder.position.magnitude)
    //            RLeg_Constraint.position = pose[i].RLeg * k;
    //        LH_Target.position = pose[i].LH_Target * k;
    //        LH_Hint.position = pose[i].LH_Hint * k;
    //        RH_Target.position = pose[i].RH_Target * k;
    //        RH_Hint.position = pose[i].RH_Hint * k;
    //        i++;
    //    }
    //}
}

