using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class AvatarSourceView : MonoBehaviour
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;


    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };


    // Jill Zombie Avatar from Mixamo
  
    private Dictionary<string,string> _RigMap = new Dictionary<string, string>()
    {
        {"SpineBase", "Hips"},
        {"KneeLeft", "Hips/RightUpLeg"},
        {"KneeRight", "Hips/LeftUpLeg"},
        {"AnkleLeft", "Hips/RightUpLeg/RightLeg"},
        {"AnkleRight", "Hips/LeftUpLeg/LeftLeg"},
        //{"FootLeft", "Hips/RightUpLeg/RightLeg/RightFoot"},
        //{"FootRight", "Hips/LeftUpLeg/LeftLeg/LeftFoot"},

        {"SpineMid", "Hips/Spine"},
        {"SpineShoulder", "Hips/Spine/Spine1/Spine2"},
        {"ShoulderLeft", "Hips/Spine/Spine1/Spine2/RightShoulder"},
        {"ShoulderRight", "Hips/Spine/Spine1/Spine2/LeftShoulder"},
        {"ElbowLeft", "Hips/Spine/Spine1/Spine2/RightShoulder/RightArm"},
        {"ElbowRight", "Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm"},
        {"WristLeft", "Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm"},
        {"WristRight", "Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm"},

        //{"HandLeft", "Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand"},
        //{"HandRight", "Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm/LeftHand"},

        {"Neck", "Hips/Spine/Spine1/Spine2/Neck"},
        {"Head", "Hips/Spine/Spine1/Spine2/Neck/Neck1/Head"},

    };

    private Dictionary<string, Quaternion> _RigMapOffsets = new Dictionary<string, Quaternion>()
    {
        {"SpineBase", Quaternion.Euler(0.0f,0.0f, 0.0f)},
        {"KneeLeft", Quaternion.Euler(0.0f, 90.0f, 0.0f)},
        {"KneeRight", Quaternion.Euler(0.0f, -90.0f, 0.0f)},
        {"AnkleLeft", Quaternion.Euler(0.0f, 90.0f, 0.0f)},
        {"AnkleRight", Quaternion.Euler(0.0f, -90.0f, 0.0f)},
        //{"FootLeft", Quaternion.Euler(225.0f, 0.0f, 0.0f)},
        //{"FootRight", Quaternion.Euler(225.0f, 0.0f, 0.0f)},

        {"SpineMid",Quaternion.Euler(0.0f, 0.0f, 0.0f)},
        {"SpineShoulder",Quaternion.Euler(0.0f, 0.0f, 0.0f)},
        {"ShoulderLeft", Quaternion.Euler(0.0f, 90.0f, 0.0f)},
        {"ShoulderRight", Quaternion.Euler(0.0f, -90.0f, 0.0f)},
        {"ElbowLeft", Quaternion.Euler(0.0f, 180.0f, 0.0f)},
        {"ElbowRight", Quaternion.Euler(0f, -180.0f, 0.0f)},
        {"WristLeft", Quaternion.Euler(0.0f, 90.0f, 0.0f)},
        {"WristRight", Quaternion.Euler(0.0f, -90.0f, 0.0f)},

        //{"HandLeft", Quaternion.Euler(0.0f, 90.0f, 0.0f)},
        //{"HandRight", Quaternion.Euler(0.0f, 0.0f, 0.0f)},

        {"Neck", Quaternion.Euler(0.0f, 0.0f, 0.0f)},
        {"Head", Quaternion.Euler(0.0f, 0.0f, 0.0f)},

    };

    // Zombie Avatar
    /*
  private Dictionary<string, string> _RigMap = new Dictionary<string, string>()
  {
      {"SpineBase", "Bip01"},
      {"KneeRight", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 L Thigh"},
      {"KneeLeft", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 R Thigh"},
      {"AnkleRight", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 L Thigh/Bip01 L Calf"},
      {"AnkleLeft", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 R Thigh/Bip01 R Calf"},
      {"FootRight", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot"},
      {"FootLeft", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot"},
  };
   */
    void Update()
    {
        if (BodySourceManager == null)
        {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }

        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

                RefreshBodyObject(body, _Bodies[body.TrackingId]);
                SetAvatarScale(_Bodies[body.TrackingId]);
            }

        }

    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = new GameObject();
            //GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;

        }
        // Add avatar gameobject from source
        GameObject avatar = Instantiate(Resources.Load("Jill", typeof(GameObject))) as GameObject;
        avatar.transform.parent = body.transform;
        avatar.name = "Avatar";
        //avatar.transform.parent = body.transform.Find("SpineBase");
        return body;
    }

    private void SetAvatarScale(GameObject bodyObject)
    {

        Transform avatar = bodyObject.transform.FindChild("Avatar");
        if (avatar.localScale.x != 1){
            return;
        }

        //Scale avatar based on torso distance
        Transform hips = avatar.FindChild("Hips");
        Transform spineBase = bodyObject.transform.FindChild("SpineBase");
        Transform spineShoulder = bodyObject.transform.FindChild("SpineShoulder");
        float bodyScale = Vector3.Magnitude(spineShoulder.position - spineBase.position);
        Transform neck = avatar.FindChild("Hips/Spine/Spine1/Spine2/Neck/Neck1/Head");
        float avatarScale = Vector3.Magnitude(neck.position - hips.position);
        float scaleFactor = bodyScale / avatarScale;
        avatar.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        Transform avatar = bodyObject.transform.FindChild("Avatar");

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.JointOrientation sourceJointOrientation = body.JointOrientations[jt];
            Kinect.Joint? targetJoint = null;
            Kinect.JointOrientation? targetJointOrientation = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
                targetJointOrientation = body.JointOrientations[_BoneMap[jt]];

                if (_RigMap.ContainsKey(jt.ToString())) {
                    Transform avatarItem = avatar.FindChild(_RigMap[jt.ToString()]);
                    Transform bodyItem = bodyObject.transform.FindChild(jt.ToString());

                    if (jt.ToString() == "SpineBase")
                        {
                        avatarItem.position = bodyItem.position;
                    }
                    avatarItem.rotation =  bodyItem.rotation * _RigMapOffsets[jt.ToString()];
                }
            }
            
            Transform jointObj = bodyObject.transform.FindChild(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            jointObj.localRotation = GetQuaternionFromJointOrientation(sourceJointOrientation);
            
        }
    }
    
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 3, joint.Position.Y * 3 , joint.Position.Z * 3);
    }

    private static Quaternion GetQuaternionFromJointOrientation(Kinect.JointOrientation jointOrientation)
    {
        return new Quaternion(jointOrientation.Orientation.X, jointOrientation.Orientation.Y, jointOrientation.Orientation.Z, jointOrientation.Orientation.W);
    }
}
