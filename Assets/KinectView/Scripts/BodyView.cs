using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

using HoloToolkit.Sharing;
using HoloToolkit.Unity;

public class BodyView : MonoBehaviour
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();

    private BodyDataConverter _BodyDataConverter;
    private BodyDataReceiver _BodyDataReceiver;

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

    private Dictionary<string, string> _RigMap = new Dictionary<string, string>()
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

        Dictionary<ulong, Vector3[]> bodies_pos = null;
        Dictionary<ulong, Quaternion[]> bodies_rot = null;

        _BodyDataConverter = BodySourceManager.GetComponent<BodyDataConverter>();
        if (_BodyDataConverter == null)
        {
            _BodyDataReceiver = BodySourceManager.GetComponent<BodyDataReceiver>();
            if (_BodyDataReceiver == null)
            {
                Debug.Log("No body data receiver");
                return;
            }
            else
            {
                Debug.Log("Getting bodies position orientation data");
                bodies_pos = _BodyDataReceiver.GetPosData();
                bodies_rot = _BodyDataReceiver.GetRotData();
            }
        }
        else
        {
            //bodies = _BodyDataConverter.GetData();
        }

        if (bodies_pos == null || bodies_rot == null)
        {
            Debug.Log("No bodies position orientation data");
            return;
        }

        List<ulong> trackedIDs = new List<ulong>(bodies_pos.Keys);
        List<ulong> knownIDs = new List<ulong>(_Bodies.Keys);
        foreach (ulong trackingID in knownIDs)
        {

            if (!trackedIDs.Contains(trackingID))
            {
                Destroy(_Bodies[trackingID]);
                _Bodies.Remove(trackingID);
            }
        }

        foreach (ulong trackingID in bodies_pos.Keys)
        {
            Debug.Log("Checking bodies with tracking ID:" + trackingID);
            if (!_Bodies.ContainsKey(trackingID))
            {
                Debug.Log("Create Body Object");
                _Bodies[trackingID] = CreateBodyObject(trackingID);
            }

            RefreshBodyObject(trackingID, bodies_pos, bodies_rot);
        }
    }

    private GameObject CreateBodyObject(ulong id)
    {
        Debug.Log("Created Avatar Object");
        GameObject body = new GameObject("Body:" + id);
        GameObject avatar = Instantiate(Resources.Load("Jill", typeof(GameObject))) as GameObject;
        avatar.transform.parent = body.transform;
        avatar.name = "Avatar";
        return body;
    }

/*    private void SetAvatarScale(GameObject bodyObject)
    {

        Transform avatar = bodyObject.transform.FindChild("Avatar");
        if (avatar.localScale.x != 1)
        {
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

    }*/

    private void RefreshBodyObject(ulong id, Dictionary<ulong, Vector3[]> bodies_pos, Dictionary<ulong, Quaternion[]> bodies_rot)
    {

        Debug.Log("Updating Avatar Object");
        GameObject bodyObject = _Bodies[id];

        Transform avatar = bodyObject.transform.FindChild("Avatar");

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            if (_BoneMap.ContainsKey(jt))
            {
                if (_RigMap.ContainsKey(jt.ToString()))
                {
                    Transform avatarItem = avatar.FindChild(_RigMap[jt.ToString()]);
                    Transform bodyItem = bodyObject.transform.FindChild(jt.ToString());

                    if (jt.ToString() == "SpineBase")
                    {
                        avatarItem.position = bodies_pos[id][(int)jt];
                    }
                    avatarItem.rotation = bodies_rot[id][(int)jt] * _RigMapOffsets[jt.ToString()];
                }
            }
        }
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 3, joint.Position.Y * 3, joint.Position.Z * 3);
    }

    private static Quaternion GetQuaternionFromJointOrientation(Kinect.JointOrientation jointOrientation)
    {
        return new Quaternion(jointOrientation.Orientation.X, jointOrientation.Orientation.Y, jointOrientation.Orientation.Z, jointOrientation.Orientation.W);
    }
}
