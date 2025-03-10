﻿// Decompiled with JetBrains decompiler
// Type: ValheimRAFT.RudderComponent
// Assembly: ValheimRAFT, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 98A52806-B1EE-47F9-B4D8-4FBCE5F3450B
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins\ValheimRAFT\ValheimRAFT.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ValheimRAFT
{
  public class RudderComponent : MonoBehaviour
  {
    public ShipControlls m_controls;
    public Transform m_wheel;
    public List<Transform> m_spokes = new List<Transform>();
    public Vector3 m_leftHandPosition = new Vector3(0.0f, 0.0f, 2f);
    public Vector3 m_rightHandPosition = new Vector3(0.0f, 0.0f, -2f);
    public float m_holdWheelTime = 0.7f;
    public float m_wheelRotationFactor = 4f;
    public float m_handIKSpeed = 0.2f;
    private float m_movingLeftAlpha;
    private float m_movingRightAlpha;
    private Transform m_currentLeftHand;
    private Transform m_currentRightHand;
    private Transform m_targetLeftHand;
    private Transform m_targetRightHand;

    public void UpdateSpokes()
    {
      this.m_spokes.Clear();
      this.m_spokes.AddRange(((IEnumerable<Transform>) ((Component) this.m_wheel).GetComponentsInChildren<Transform>()).Where<Transform>((Func<Transform, bool>) (k => ((Object) ((Component) k).gameObject).name.StartsWith("grabpoint"))));
    }

    public void UpdateIK(Animator animator)
    {
      if (!Object.op_Implicit((Object) this.m_wheel))
        return;
      if (!Object.op_Implicit((Object) this.m_currentLeftHand))
        this.m_currentLeftHand = this.GetNearestSpoke(((Component) this).transform.TransformPoint(this.m_leftHandPosition));
      if (!Object.op_Implicit((Object) this.m_currentRightHand))
        this.m_currentRightHand = this.GetNearestSpoke(((Component) this).transform.TransformPoint(this.m_rightHandPosition));
      if (!Object.op_Implicit((Object) this.m_targetLeftHand) && !Object.op_Implicit((Object) this.m_targetRightHand))
      {
        Vector3 vector3_1 = ((Component) this).transform.InverseTransformPoint(this.m_currentLeftHand.position);
        Vector3 vector3_2 = ((Component) this).transform.InverseTransformPoint(this.m_currentRightHand.position);
        if ((double) vector3_1.z < 0.200000002980232)
        {
          Vector3 vector3_3;
          // ISSUE: explicit constructor call
          ((Vector3) ref vector3_3).\u002Ector(0.0f, (double) vector3_1.y > 0.5 ? -2f : 2f, 0.0f);
          this.m_targetLeftHand = this.GetNearestSpoke(((Component) this).transform.TransformPoint(Vector3.op_Addition(this.m_leftHandPosition, vector3_3)));
          this.m_movingLeftAlpha = Time.time;
        }
        else if ((double) vector3_2.z > -0.200000002980232)
        {
          Vector3 vector3_4;
          // ISSUE: explicit constructor call
          ((Vector3) ref vector3_4).\u002Ector(0.0f, (double) vector3_2.y > 0.5 ? -2f : 2f, 0.0f);
          this.m_targetRightHand = this.GetNearestSpoke(((Component) this).transform.TransformPoint(Vector3.op_Addition(this.m_rightHandPosition, vector3_4)));
          this.m_movingRightAlpha = Time.time;
        }
      }
      float num1 = Mathf.Clamp01((Time.time - this.m_movingLeftAlpha) / this.m_handIKSpeed);
      float num2 = Mathf.Clamp01((Time.time - this.m_movingRightAlpha) / this.m_handIKSpeed);
      float num3 = Mathf.Sin(num1 * 3.141593f) * (1f - this.m_holdWheelTime) + this.m_holdWheelTime;
      float num4 = Mathf.Sin(num2 * 3.141593f) * (1f - this.m_holdWheelTime) + this.m_holdWheelTime;
      if (Object.op_Implicit((Object) this.m_targetLeftHand) && (double) num1 > 0.990000009536743)
      {
        this.m_currentLeftHand = this.m_targetLeftHand;
        this.m_targetLeftHand = (Transform) null;
      }
      if (Object.op_Implicit((Object) this.m_targetRightHand) && (double) num2 > 0.990000009536743)
      {
        this.m_currentRightHand = this.m_targetRightHand;
        this.m_targetRightHand = (Transform) null;
      }
      Vector3 vector3_5 = Object.op_Implicit((Object) this.m_targetLeftHand) ? Vector3.Lerp(((Component) this.m_currentLeftHand).transform.position, ((Component) this.m_targetLeftHand).transform.position, num1) : ((Component) this.m_currentLeftHand).transform.position;
      Vector3 vector3_6 = Object.op_Implicit((Object) this.m_targetRightHand) ? Vector3.Lerp(((Component) this.m_currentRightHand).transform.position, ((Component) this.m_targetRightHand).transform.position, num2) : ((Component) this.m_currentRightHand).transform.position;
      Vector3 vector3_7;
      if (!Object.op_Implicit((Object) this.m_targetLeftHand))
      {
        Quaternion rotation = ((Component) this.m_currentLeftHand).transform.rotation;
        vector3_7 = ((Quaternion) ref rotation).eulerAngles;
      }
      else
      {
        Quaternion rotation1 = ((Component) this.m_currentLeftHand).transform.rotation;
        Vector3 eulerAngles1 = ((Quaternion) ref rotation1).eulerAngles;
        Quaternion rotation2 = ((Component) this.m_targetLeftHand).transform.rotation;
        Vector3 eulerAngles2 = ((Quaternion) ref rotation2).eulerAngles;
        double num5 = (double) num1;
        vector3_7 = Vector3.Slerp(eulerAngles1, eulerAngles2, (float) num5);
      }
      animator.SetIKPositionWeight((AvatarIKGoal) 2, num3);
      animator.SetIKPosition((AvatarIKGoal) 2, vector3_5);
      animator.SetIKPositionWeight((AvatarIKGoal) 3, num4);
      animator.SetIKPosition((AvatarIKGoal) 3, vector3_6);
    }

    public Transform GetNearestSpoke(Vector3 position)
    {
      Transform nearestSpoke = (Transform) null;
      float num = 0.0f;
      for (int index = 0; index < this.m_spokes.Count; ++index)
      {
        Transform spoke = this.m_spokes[index];
        Vector3 vector3 = Vector3.op_Subtraction(((Component) spoke).transform.position, position);
        float sqrMagnitude = ((Vector3) ref vector3).sqrMagnitude;
        if (Object.op_Equality((Object) nearestSpoke, (Object) null) || (double) sqrMagnitude < (double) num)
        {
          nearestSpoke = spoke;
          num = sqrMagnitude;
        }
      }
      return nearestSpoke;
    }
  }
}
