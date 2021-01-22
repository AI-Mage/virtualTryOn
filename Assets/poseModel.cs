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

    private Transform Hip_Target, RLeg_Constraint, Hip_Rotation, RHip, Head_IK, RL_Target, RL_Hint, LL_Target, LL_Hint, LH_Target, LH_Hint, RH_Target, RH_Hint, RShoulder, RHand_Constraint;
    private List<Vector3> humanjoints;
    Pose oldie = null;
    float floorLimit;

    void Start()
    {
        
        Application.targetFrameRate = FrameRate;
        

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

        //Debug.Log(RL_Target.position);

        #region
        humanjoints = new List<Vector3>();
        humanjoints.Add(RL_Target.position);
        humanjoints.Add(RL_Hint.position);
        humanjoints.Add(LL_Target.position);
        humanjoints.Add(LL_Hint.position);
        humanjoints.Add(RHip.position);
        humanjoints.Add(LH_Target.position);
        humanjoints.Add(LH_Hint.position);
        humanjoints.Add(RH_Target.position);
        humanjoints.Add(RH_Hint.position);
        humanjoints.Add(RShoulder.position);
        humanjoints.Add(Head_IK.position);
        #endregion

        floorLimit = RL_Target.position.y;

        pose = Pose.LoadPoseFromFile("jumpingJack.json", "root_Jumping Jacks (2)_", humanjoints); //scaling factor = -0.795
        //pose = Pose.LoadPoseFromFile("jumpingJack.json", "root_Jumping Jacks (2)_"); //scaling factor = -0.795
        //Debug.Log("Here man "+pose.ToString());
        //pose = Pose.LoadPoseFromFile("Indian_man.json", "root_Indian man_", humanjoints);
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

        //Debug.Log(Hip_Target.position.y);
        //Debug.Log(RL_Target.position.y);
        //Debug.Log(floorLimit);
        //if (RL_Target.position.y < floorLimit)
        //RL_Target.SetPositionAndRotation(new Vector3(RL_Target.position.x, floorLimit, RL_Target.position.z), RL_Target.rotation);
        if (Hip_Target.position.y < 0)  // Fix for knee bending sitting postures.
        {
            RL_Target.SetPositionAndRotation(new Vector3(RL_Target.position.x, floorLimit - Hip_Target.position.y, RL_Target.position.z), RL_Target.rotation);
            LL_Target.SetPositionAndRotation(new Vector3(LL_Target.position.x, floorLimit - Hip_Target.position.y, LL_Target.position.z), LL_Target.rotation);
        }




    }

    //void Update()
    //{
    //    float k = ScalingFactor;
    //    if (i < pose.Length)
    //    {
    //        if (is2D)
    //            pose[i].Ensure2D();

    //        Hip_Target.position = pose[i].Hip;
    //        if (k * pose[i].RLeg.magnitude >= RHip.position.magnitude &&
    //            k * pose[i].RLeg.magnitude < (1 + HipRotationTolerance) * RHip.position.magnitude)
    //            RLeg_Constraint.position = pose[i].RLeg * -Pose.scale.sc_RHip;
    //        Head_IK.position = pose[i].Head_IK * -Pose.scale.sc_Head_IK;
    //        if (Hip_Target.position.y < 0)  // Fix for knee bending sitting postures.
    //            RL_Target.SetPositionAndRotation(new Vector3(pose[i].RL_Target.x, -floorLimit + Hip_Target.position.y, pose[i].RL_Target.z) * -Pose.scale.sc_RL_Target, RL_Target.rotation);
    //        //RL_Target.position = pose[i].RL_Target * -Pose.scale.sc_RL_Target;
    //        RL_Hint.position = pose[i].RL_Hint * -Pose.scale.sc_RL_Hint;
    //        if (Hip_Target.position.y < 0)  // Fix for knee bending sitting postures.
    //            LL_Target.SetPositionAndRotation(new Vector3(pose[i].LL_Target.x, -floorLimit + Hip_Target.position.y, pose[i].LL_Target.z) * -Pose.scale.sc_LL_Target, LL_Target.rotation);
    //        //LL_Target.position = pose[i].LL_Target * -Pose.scale.sc_LL_Target;
    //        LL_Hint.position = pose[i].LL_Hint * -Pose.scale.sc_LL_Hint;
    //       if (k * pose[i].RHand.magnitude >= RShoulder.position.magnitude &&
    //            k * pose[i].RHand.magnitude < (1 + SpineRotationTolerance) * RShoulder.position.magnitude)
    //            RHand_Constraint.position = pose[i].RHand * -Pose.scale.sc_RShoulder;
    //        LH_Target.position = pose[i].LH_Target * -Pose.scale.sc_LH_Target;
    //        LH_Hint.position = pose[i].LH_Hint * -Pose.scale.sc_LH_Hint;
    //        RH_Target.position = pose[i].RH_Target * -Pose.scale.sc_RH_Target;
    //        RH_Hint.position = pose[i].RH_Hint * -Pose.scale.sc_RH_Hint;
    //        i++;
    //    }
    //}
}

