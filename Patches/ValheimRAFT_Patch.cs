﻿// Decompiled with JetBrains decompiler
// Type: ValheimRAFT.Patches.ValheimRAFT_Patch
// Assembly: ValheimRAFT, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 98A52806-B1EE-47F9-B4D8-4FBCE5F3450B
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins\ValheimRAFT\ValheimRAFT.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace ValheimRAFT.Patches
{
  [HarmonyPatch]
  public class ValheimRAFT_Patch
  {
    public static float yawOffset;
    private static ShipControlls m_lastUsedControls;
    private static Piece m_lastRayPiece;

    [HarmonyPatch(typeof (ZNetScene), "Awake")]
    [HarmonyPostfix]
    private static void ZNetScene_Awake() => ValheimRAFT.ValheimRAFT.Instance.AddCustomPieces();

    [HarmonyPatch(typeof (ShipControlls), "Awake")]
    [HarmonyPrefix]
    private static bool ShipControlls_Awake(Ship __instance) => !Object.op_Implicit((Object) ((Component) __instance).GetComponentInParent<RudderComponent>());

    [HarmonyPatch(typeof (ShipControlls), "Interact")]
    [HarmonyPrefix]
    private static void Interact(ShipControlls __instance, Humanoid character)
    {
      if (!Object.op_Equality((Object) character, (Object) Player.m_localPlayer))
        return;
      ValheimRAFT_Patch.m_lastUsedControls = __instance;
      __instance.m_ship.m_controlGuiPos.position = ((Component) __instance).transform.position;
    }

    [HarmonyPatch(typeof (ShipControlls), "RPC_RequestRespons")]
    [HarmonyPrefix]
    private static bool ShipControlls_RPC_RequestRespons(
      ShipControlls __instance,
      long sender,
      bool granted)
    {
      if (!Object.op_Inequality((Object) __instance, (Object) ValheimRAFT_Patch.m_lastUsedControls))
        return true;
      ValheimRAFT_Patch.m_lastUsedControls.RPC_RequestRespons(sender, granted);
      return false;
    }

    [HarmonyPatch(typeof (Ship), "Awake")]
    [HarmonyPostfix]
    private static void Ship_Awake(Ship __instance)
    {
      if (!Object.op_Implicit((Object) __instance.m_nview) || __instance.m_nview.m_zdo == null || !((Object) __instance).name.StartsWith("MBRaft"))
        return;
      foreach (Ladder componentsInChild in ((Component) __instance).GetComponentsInChildren<Ladder>())
        componentsInChild.m_useDistance = 10f;
      ((Component) __instance).gameObject.AddComponent<MoveableBaseShipComponent>();
    }

    [HarmonyPatch(typeof (Ship), "UpdateUpsideDmg")]
    [HarmonyPrefix]
    private static bool Ship_UpdateUpsideDmg(Ship __instance) => !Object.op_Implicit((Object) ((Component) __instance).GetComponent<MoveableBaseShipComponent>());

    [HarmonyPatch(typeof (Ship), "UpdateSail")]
    [HarmonyPostfix]
    private static void Ship_UpdateSail(Ship __instance)
    {
      MoveableBaseShipComponent component = ((Component) __instance).GetComponent<MoveableBaseShipComponent>();
      if (!Object.op_Implicit((Object) component) || !Object.op_Implicit((Object) component.m_baseRoot))
        return;
      for (int index = 0; index < component.m_baseRoot.m_mastPieces.Count; ++index)
      {
        MastComponent mastPiece = component.m_baseRoot.m_mastPieces[index];
        if (!Object.op_Implicit((Object) mastPiece))
        {
          component.m_baseRoot.m_mastPieces.RemoveAt(index);
          --index;
        }
        else
          ((Component) mastPiece).transform.localRotation = __instance.m_mastObject.transform.localRotation;
      }
    }

    [HarmonyPatch(typeof (Ship), "UpdateSail")]
    [HarmonyPostfix]
    private static void Ship_UpdateSailSize(Ship __instance)
    {
      MoveableBaseShipComponent component = ((Component) __instance).GetComponent<MoveableBaseShipComponent>();
      if (!Object.op_Implicit((Object) component) || !Object.op_Implicit((Object) component.m_baseRoot))
        return;
      for (int index = 0; index < component.m_baseRoot.m_mastPieces.Count; ++index)
      {
        MastComponent mastPiece = component.m_baseRoot.m_mastPieces[index];
        if (!Object.op_Implicit((Object) mastPiece))
        {
          component.m_baseRoot.m_mastPieces.RemoveAt(index);
          --index;
        }
        else
        {
          mastPiece.m_sailObject.transform.localScale = __instance.m_sailObject.transform.localScale;
          mastPiece.m_sailCloth.enabled = __instance.m_sailCloth.enabled;
        }
      }
      for (int index = 0; index < component.m_baseRoot.m_rudderPieces.Count; ++index)
      {
        RudderComponent rudderPiece = component.m_baseRoot.m_rudderPieces[index];
        if (!Object.op_Implicit((Object) rudderPiece))
        {
          component.m_baseRoot.m_rudderPieces.RemoveAt(index);
          --index;
        }
        else if (Object.op_Implicit((Object) rudderPiece.m_wheel))
          rudderPiece.m_wheel.localRotation = Quaternion.Slerp(rudderPiece.m_wheel.localRotation, Quaternion.Euler(__instance.m_rudderRotationMax * (0.0f - __instance.m_rudderValue) * rudderPiece.m_wheelRotationFactor, 0.0f, 0.0f), 0.5f);
      }
    }

    [HarmonyPatch(typeof (Ship), "FixedUpdate")]
    [HarmonyPrefix]
    private static bool Ship_FixedUpdate(Ship __instance)
    {
      MoveableBaseShipComponent component = ((Component) __instance).GetComponent<MoveableBaseShipComponent>();
      if (!Object.op_Implicit((Object) component) || !Object.op_Implicit((Object) __instance.m_nview) || __instance.m_nview.m_zdo == null)
        return true;
      component.m_targetHeight = __instance.m_nview.m_zdo.GetFloat("MBTargetHeight", component.m_targetHeight);
      component.m_anchored = __instance.m_nview.m_zdo.GetBool("MBAnchored", component.m_anchored);
      component.m_zsync.m_useGravity = (double) component.m_targetHeight == 0.0;
      bool flag = __instance.HaveControllingPlayer();
      __instance.UpdateControlls(Time.fixedDeltaTime);
      __instance.UpdateSail(Time.fixedDeltaTime);
      __instance.UpdateRudder(Time.fixedDeltaTime, flag);
      if (Object.op_Implicit((Object) __instance.m_nview) && !__instance.m_nview.IsOwner())
        return false;
      __instance.UpdateUpsideDmg(Time.fixedDeltaTime);
      if (__instance.m_players.Count == 0 || component.m_anchored)
      {
        __instance.m_speed = (Ship.Speed) 0;
        __instance.m_rudderValue = 0.0f;
        if (!component.m_anchored)
        {
          component.m_anchored = true;
          __instance.m_nview.m_zdo.Set("MBAnchored", component.m_anchored);
        }
      }
      if (!flag && (__instance.m_speed == 2 || __instance.m_speed == 1))
        __instance.m_speed = (Ship.Speed) 0;
      float num1 = 1f;
      Vector3 worldCenterOfMass = __instance.m_body.worldCenterOfMass;
      Vector3 vector3_1 = Vector3.op_Addition(((Component) __instance.m_floatCollider).transform.position, Vector3.op_Division(Vector3.op_Multiply(((Component) __instance.m_floatCollider).transform.forward, __instance.m_floatCollider.size.z), 2f));
      Vector3 vector3_2 = Vector3.op_Subtraction(((Component) __instance.m_floatCollider).transform.position, Vector3.op_Division(Vector3.op_Multiply(((Component) __instance.m_floatCollider).transform.forward, __instance.m_floatCollider.size.z), 2f));
      Vector3 vector3_3 = Vector3.op_Subtraction(((Component) __instance.m_floatCollider).transform.position, Vector3.op_Division(Vector3.op_Multiply(((Component) __instance.m_floatCollider).transform.right, __instance.m_floatCollider.size.x), 2f));
      Vector3 vector3_4 = Vector3.op_Addition(((Component) __instance.m_floatCollider).transform.position, Vector3.op_Division(Vector3.op_Multiply(((Component) __instance.m_floatCollider).transform.right, __instance.m_floatCollider.size.x), 2f));
      float liquidLevel1 = Floating.GetLiquidLevel(worldCenterOfMass, num1, (LiquidType) 10);
      float liquidLevel2 = Floating.GetLiquidLevel(vector3_3, num1, (LiquidType) 10);
      float liquidLevel3 = Floating.GetLiquidLevel(vector3_4, num1, (LiquidType) 10);
      float liquidLevel4 = Floating.GetLiquidLevel(vector3_1, num1, (LiquidType) 10);
      float liquidLevel5 = Floating.GetLiquidLevel(vector3_2, num1, (LiquidType) 10);
      float num2 = (float) (((double) liquidLevel1 + (double) liquidLevel2 + (double) liquidLevel3 + (double) liquidLevel4 + (double) liquidLevel5) / 5.0);
      float num3 = worldCenterOfMass.y - num2 - __instance.m_waterLevelOffset;
      if ((double) num3 <= (double) __instance.m_disableLevel)
      {
        component.UpdateStats(false);
        __instance.m_body.WakeUp();
        __instance.UpdateWaterForce(num3, Time.fixedDeltaTime);
        Vector3 vector3_5;
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3_5).\u002Ector(vector3_3.x, liquidLevel2, vector3_3.z);
        Vector3 vector3_6;
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3_6).\u002Ector(vector3_4.x, liquidLevel3, vector3_4.z);
        Vector3 vector3_7;
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3_7).\u002Ector(vector3_1.x, liquidLevel4, vector3_1.z);
        Vector3 vector3_8;
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3_8).\u002Ector(vector3_2.x, liquidLevel5, vector3_2.z);
        float num4 = Time.fixedDeltaTime * 50f;
        float num5 = Mathf.Clamp01(Mathf.Abs(num3) / __instance.m_forceDistance);
        Vector3 vector3_9 = Vector3.op_Multiply(Vector3.op_Multiply(Vector3.up, __instance.m_force), num5);
        __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(vector3_9, num4), worldCenterOfMass, (ForceMode) 2);
        float num5_1 = Vector3.Dot(__instance.m_body.velocity, ((Component) __instance).transform.forward);
        float num6 = Vector3.Dot(__instance.m_body.velocity, ((Component) __instance).transform.right);
        Vector3 velocity1 = __instance.m_body.velocity;
        float num7 = velocity1.y * velocity1.y * Mathf.Sign(velocity1.y) * __instance.m_damping * num5;
        float num8 = num5_1 * num5_1 * Mathf.Sign(num5_1) * __instance.m_dampingForward * num5;
        float num9 = num6 * num6 * Mathf.Sign(num6) * __instance.m_dampingSideway * num5;
        velocity1.y -= Mathf.Clamp(num7, -1f, 1f);
        Vector3 vector3_10 = Vector3.op_Subtraction(Vector3.op_Subtraction(velocity1, Vector3.op_Multiply(((Component) __instance).transform.forward, Mathf.Clamp(num8, -1f, 1f))), Vector3.op_Multiply(((Component) __instance).transform.right, Mathf.Clamp(num9, -1f, 1f)));
        double magnitude1 = (double) ((Vector3) ref vector3_10).magnitude;
        Vector3 velocity2 = __instance.m_body.velocity;
        double magnitude2 = (double) ((Vector3) ref velocity2).magnitude;
        if (magnitude1 > magnitude2)
        {
          Vector3 normalized = ((Vector3) ref vector3_10).normalized;
          Vector3 velocity3 = __instance.m_body.velocity;
          double magnitude3 = (double) ((Vector3) ref velocity3).magnitude;
          vector3_10 = Vector3.op_Multiply(normalized, (float) magnitude3);
        }
        if (__instance.m_players.Count == 0)
        {
          vector3_10.x *= 0.1f;
          vector3_10.z *= 0.1f;
        }
        __instance.m_body.velocity = vector3_10;
        Rigidbody body = __instance.m_body;
        body.angularVelocity = Vector3.op_Subtraction(body.angularVelocity, Vector3.op_Multiply(Vector3.op_Multiply(__instance.m_body.angularVelocity, __instance.m_angularDamping), num5));
        float num10 = 0.15f;
        float num11 = 0.5f;
        float num12 = Mathf.Clamp((vector3_7.y - vector3_1.y) * num10, 0.0f - num11, num11);
        float num13 = Mathf.Clamp((vector3_8.y - vector3_2.y) * num10, 0.0f - num11, num11);
        float num14 = Mathf.Clamp((vector3_5.y - vector3_3.y) * num10, 0.0f - num11, num11);
        float num15 = Mathf.Clamp((vector3_6.y - vector3_4.y) * num10, 0.0f - num11, num11);
        float num16 = Mathf.Sign(num12) * Mathf.Abs(Mathf.Pow(num12, 2f));
        float num17 = Mathf.Sign(num13) * Mathf.Abs(Mathf.Pow(num13, 2f));
        float num18 = Mathf.Sign(num14) * Mathf.Abs(Mathf.Pow(num14, 2f));
        float num19 = Mathf.Sign(num15) * Mathf.Abs(Mathf.Pow(num15, 2f));
        __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(Vector3.op_Multiply(Vector3.up, num16), num4), vector3_1, (ForceMode) 2);
        __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(Vector3.op_Multiply(Vector3.up, num17), num4), vector3_2, (ForceMode) 2);
        __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(Vector3.op_Multiply(Vector3.up, num18), num4), vector3_3, (ForceMode) 2);
        __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(Vector3.op_Multiply(Vector3.up, num19), num4), vector3_4, (ForceMode) 2);
        ValheimRAFT_Patch.ApplySailForce(__instance, num5_1);
        __instance.ApplyEdgeForce(Time.fixedDeltaTime);
        if ((double) component.m_targetHeight > 0.0)
        {
          Vector3 position = ((Component) __instance.m_floatCollider).transform.position;
          float upwardsForce = ValheimRAFT_Patch.GetUpwardsForce(component.m_targetHeight, position.y + __instance.m_body.velocity.y, component.m_liftForce);
          __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(Vector3.up, upwardsForce), position, (ForceMode) 2);
        }
      }
      else if ((double) component.m_targetHeight > 0.0)
      {
        component.UpdateStats(true);
        Vector3 vector3_11 = Vector3.op_Addition(((Component) __instance.m_floatCollider).transform.position, Vector3.op_Division(Vector3.op_Multiply(((Component) __instance.m_floatCollider).transform.forward, __instance.m_floatCollider.size.z), 2f));
        Vector3 vector3_12 = Vector3.op_Subtraction(((Component) __instance.m_floatCollider).transform.position, Vector3.op_Division(Vector3.op_Multiply(((Component) __instance.m_floatCollider).transform.forward, __instance.m_floatCollider.size.z), 2f));
        Vector3 vector3_13 = Vector3.op_Subtraction(((Component) __instance.m_floatCollider).transform.position, Vector3.op_Division(Vector3.op_Multiply(((Component) __instance.m_floatCollider).transform.right, __instance.m_floatCollider.size.x), 2f));
        Vector3 vector3_14 = Vector3.op_Addition(((Component) __instance.m_floatCollider).transform.position, Vector3.op_Division(Vector3.op_Multiply(((Component) __instance.m_floatCollider).transform.right, __instance.m_floatCollider.size.x), 2f));
        Vector3 position = ((Component) __instance.m_floatCollider).transform.position;
        Vector3 pointVelocity1 = __instance.m_body.GetPointVelocity(vector3_11);
        Vector3 pointVelocity2 = __instance.m_body.GetPointVelocity(vector3_12);
        Vector3 pointVelocity3 = __instance.m_body.GetPointVelocity(vector3_13);
        Vector3 pointVelocity4 = __instance.m_body.GetPointVelocity(vector3_14);
        float upwardsForce1 = ValheimRAFT_Patch.GetUpwardsForce(component.m_targetHeight, vector3_11.y + pointVelocity1.y, component.m_balanceForce);
        float upwardsForce2 = ValheimRAFT_Patch.GetUpwardsForce(component.m_targetHeight, vector3_12.y + pointVelocity2.y, component.m_balanceForce);
        float upwardsForce3 = ValheimRAFT_Patch.GetUpwardsForce(component.m_targetHeight, vector3_13.y + pointVelocity3.y, component.m_balanceForce);
        float upwardsForce4 = ValheimRAFT_Patch.GetUpwardsForce(component.m_targetHeight, vector3_14.y + pointVelocity4.y, component.m_balanceForce);
        float upwardsForce5 = ValheimRAFT_Patch.GetUpwardsForce(component.m_targetHeight, position.y + __instance.m_body.velocity.y, component.m_liftForce);
        __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(Vector3.up, upwardsForce1), vector3_11, (ForceMode) 2);
        __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(Vector3.up, upwardsForce2), vector3_12, (ForceMode) 2);
        __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(Vector3.up, upwardsForce3), vector3_13, (ForceMode) 2);
        __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(Vector3.up, upwardsForce4), vector3_14, (ForceMode) 2);
        __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(Vector3.up, upwardsForce5), position, (ForceMode) 2);
        float num5 = Vector3.Dot(__instance.m_body.velocity, ((Component) __instance).transform.forward);
        ValheimRAFT_Patch.ApplySailForce(__instance, num5);
      }
      return false;
    }

    private static void ApplySailForce(Ship __instance, float num5)
    {
      float num1 = 0.0f;
      if (__instance.m_speed == 4)
        num1 = 1f;
      else if (__instance.m_speed == 3)
        num1 = 0.5f;
      Vector3 sailForce = __instance.GetSailForce(num1, Time.fixedDeltaTime);
      Vector3 worldCenterOfMass = __instance.m_body.worldCenterOfMass;
      __instance.m_body.AddForceAtPosition(sailForce, worldCenterOfMass, (ForceMode) 2);
      Vector3 vector3_1 = Vector3.op_Subtraction(((Component) __instance.m_floatCollider).transform.position, Vector3.op_Division(Vector3.op_Multiply(((Component) __instance.m_floatCollider).transform.forward, __instance.m_floatCollider.size.z), 2f));
      float num2 = num5 * __instance.m_stearVelForceFactor;
      __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(Vector3.op_Multiply(Vector3.op_Multiply(((Component) __instance).transform.right, num2), 0.0f - __instance.m_rudderValue), Time.fixedDeltaTime), vector3_1, (ForceMode) 2);
      Vector3 vector3_2 = Vector3.zero;
      Ship.Speed speed = __instance.m_speed;
      if (speed != 1)
      {
        if (speed == 2)
          vector3_2 = Vector3.op_Addition(vector3_2, Vector3.op_Multiply(Vector3.op_Multiply(((Component) __instance).transform.forward, __instance.m_backwardForce), 1f - Mathf.Abs(__instance.m_rudderValue)));
      }
      else
        vector3_2 = Vector3.op_Addition(vector3_2, Vector3.op_Multiply(Vector3.op_Multiply(Vector3.op_UnaryNegation(((Component) __instance).transform.forward), __instance.m_backwardForce), 1f - Mathf.Abs(__instance.m_rudderValue)));
      if (__instance.m_speed == 1 || __instance.m_speed == 2)
      {
        float num3 = __instance.m_speed != 1 ? 1f : -1f;
        vector3_2 = Vector3.op_Addition(vector3_2, Vector3.op_Multiply(Vector3.op_Multiply(Vector3.op_Multiply(((Component) __instance).transform.right, __instance.m_stearForce), 0.0f - __instance.m_rudderValue), num3));
      }
      __instance.m_body.AddForceAtPosition(Vector3.op_Multiply(vector3_2, Time.fixedDeltaTime), vector3_1, (ForceMode) 2);
    }

    private static float GetUpwardsForce(float targetY, float currentY, float maxForce)
    {
      float num = targetY - currentY;
      return (double) num == 0.0 ? 0.0f : Mathf.Clamp((float) (1.0 / (25.0 / ((double) num * (double) num)) * ((double) num > 0.0 ? (double) maxForce : -(double) maxForce)), -maxForce, maxForce);
    }

    [HarmonyPatch(typeof (Piece), "Awake")]
    [HarmonyPostfix]
    private static void Piece_Awake(Piece __instance)
    {
      if (!Object.op_Implicit((Object) __instance.m_nview) || __instance.m_nview.m_zdo == null)
        return;
      MoveableBaseRootComponent.InitPiece(__instance);
    }

    [HarmonyPatch(typeof (Piece), "OnDestroy")]
    [HarmonyPrefix]
    private static void Piece_OnDestroy(Piece __instance)
    {
      MoveableBaseRootComponent componentInParent = ((Component) __instance).GetComponentInParent<MoveableBaseRootComponent>();
      if (!Object.op_Implicit((Object) componentInParent))
        return;
      componentInParent.RemovePiece(__instance);
    }

    [HarmonyPatch(typeof (WearNTear), "Destroy")]
    [HarmonyPrefix]
    private static void WearNTear_Destroy(WearNTear __instance)
    {
      MoveableBaseRootComponent componentInParent = ((Component) __instance).GetComponentInParent<MoveableBaseRootComponent>();
      if (!Object.op_Implicit((Object) componentInParent))
        return;
      componentInParent.DestroyPiece(__instance);
    }

    [HarmonyPatch(typeof (WearNTear), "ApplyDamage")]
    [HarmonyPrefix]
    private static bool WearNTear_ApplyDamage(WearNTear __instance, float damage) => !Object.op_Implicit((Object) ((Component) __instance).GetComponent<MoveableBaseShipComponent>());

    [HarmonyPatch(typeof (WearNTear), "UpdateSupport")]
    [HarmonyPatch(typeof (WearNTear), "SetupColliders")]
    [HarmonyPatch(typeof (Player), "PieceRayTest")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> WearNTear_AttachShip(
      IEnumerable<CodeInstruction> instructions)
    {
      List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
      for (int index = 0; index < list.Count; ++index)
      {
        if (CodeInstructionExtensions.Calls(list[index], AccessTools.PropertyGetter(typeof (Collider), "attachedRigidbody")))
        {
          list[index] = new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (ValheimRAFT_Patch), "AttachRigidbodyMoveableBase", (Type[]) null, (Type[]) null));
          break;
        }
      }
      return (IEnumerable<CodeInstruction>) list;
    }

    private static Rigidbody AttachRigidbodyMoveableBase(Collider collider)
    {
      Rigidbody attachedRigidbody = collider.attachedRigidbody;
      return !Object.op_Implicit((Object) attachedRigidbody) || Object.op_Implicit((Object) ((Component) attachedRigidbody).GetComponent<MoveableBaseRootComponent>()) ? (Rigidbody) null : attachedRigidbody;
    }

    [HarmonyPatch(typeof (Player), "UpdatePlacementGhost")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> UpdatePlacementGhost(
      IEnumerable<CodeInstruction> instructions)
    {
      List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
      for (int index = 0; index < list.Count; ++index)
      {
        if (CodeInstructionExtensions.Calls(list[index], AccessTools.Method(typeof (Quaternion), "Euler", new Type[3]
        {
          typeof (float),
          typeof (float),
          typeof (float)
        }, (Type[]) null)))
          list[index] = new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (ValheimRAFT_Patch), "RelativeEuler", (Type[]) null, (Type[]) null));
      }
      return (IEnumerable<CodeInstruction>) list;
    }

    private static Quaternion RelativeEuler(float x, float y, float z)
    {
      Quaternion quaternion = Quaternion.Euler(x, y, z);
      if (!Object.op_Implicit((Object) ValheimRAFT_Patch.m_lastRayPiece))
        return quaternion;
      MoveableBaseRootComponent componentInParent = ((Component) ValheimRAFT_Patch.m_lastRayPiece).GetComponentInParent<MoveableBaseRootComponent>();
      return !Object.op_Implicit((Object) componentInParent) ? quaternion : Quaternion.op_Multiply(((Component) componentInParent).transform.rotation, quaternion);
    }

    [HarmonyPatch(typeof (Character), "GetStandingOnShip")]
    [HarmonyPrefix]
    private static bool Character_GetStandingOnShip(Character __instance, ref Ship __result)
    {
      if (!__instance.IsOnGround() || !Object.op_Implicit((Object) __instance.m_lastGroundBody))
        return false;
      __result = ((Component) __instance.m_lastGroundBody).GetComponent<Ship>();
      if (!Object.op_Implicit((Object) __result))
      {
        MoveableBaseRootComponent componentInParent = ((Component) __instance.m_lastGroundBody).GetComponentInParent<MoveableBaseRootComponent>();
        if (Object.op_Implicit((Object) componentInParent) && Object.op_Implicit((Object) componentInParent.m_moveableBaseShip))
          __result = ((Component) componentInParent.m_moveableBaseShip).GetComponent<Ship>();
      }
      return false;
    }

    [HarmonyPatch(typeof (Character), "UpdateGroundContact")]
    [HarmonyPostfix]
    private static void UpdateGroundContact(Character __instance)
    {
      if (__instance is Player player && player.m_debugFly)
      {
        if (!Object.op_Inequality((Object) ((Component) __instance).transform.parent, (Object) null))
          return;
        ((Component) __instance).transform.SetParent((Transform) null);
      }
      else
      {
        MoveableBaseRootComponent baseRootComponent = (MoveableBaseRootComponent) null;
        if (Object.op_Implicit((Object) __instance.m_lastGroundBody))
        {
          baseRootComponent = ((Component) __instance.m_lastGroundBody).GetComponentInParent<MoveableBaseRootComponent>();
          if (Object.op_Implicit((Object) baseRootComponent) && Object.op_Inequality((Object) ((Component) __instance).transform.parent, (Object) ((Component) baseRootComponent).transform))
            ((Component) __instance).transform.SetParent(((Component) baseRootComponent).transform);
        }
        if (Object.op_Implicit((Object) baseRootComponent) || !Object.op_Inequality((Object) ((Component) __instance).transform.parent, (Object) null))
          return;
        ((Component) __instance).transform.SetParent((Transform) null);
      }
    }

    [HarmonyPatch(typeof (Player), "PlacePiece")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> PlacePiece(
      IEnumerable<CodeInstruction> instructions)
    {
      List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
      for (int index = 0; index < list.Count; ++index)
      {
        if (list[index].operand != null && list[index].operand.ToString() == "UnityEngine.GameObject Instantiate[GameObject](UnityEngine.GameObject, UnityEngine.Vector3, UnityEngine.Quaternion)")
        {
          list.InsertRange(index + 2, (IEnumerable<CodeInstruction>) new CodeInstruction[3]
          {
            new CodeInstruction(OpCodes.Ldarg_0, (object) null),
            new CodeInstruction(OpCodes.Ldloc_3, (object) null),
            new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (ValheimRAFT_Patch), "PlacedPiece", (Type[]) null, (Type[]) null))
          });
          break;
        }
      }
      return (IEnumerable<CodeInstruction>) list;
    }

    private static void PlacedPiece(Player player, GameObject gameObject)
    {
      Piece component = gameObject.GetComponent<Piece>();
      if (!Object.op_Implicit((Object) component))
        return;
      Rigidbody componentInChildren = ((Component) component).GetComponentInChildren<Rigidbody>();
      if (Object.op_Implicit((Object) componentInChildren) && !componentInChildren.isKinematic || !Object.op_Implicit((Object) ValheimRAFT_Patch.m_lastRayPiece))
        return;
      MoveableBaseRootComponent componentInParent = ((Component) ValheimRAFT_Patch.m_lastRayPiece).GetComponentInParent<MoveableBaseRootComponent>();
      if (!Object.op_Implicit((Object) componentInParent))
        return;
      componentInParent.AddNewPiece(component);
    }

    [HarmonyPatch(typeof (Player), "PieceRayTest")]
    [HarmonyPrefix]
    private static bool PieceRayTest(
      Player __instance,
      ref bool __result,
      ref Vector3 point,
      ref Vector3 normal,
      ref Piece piece,
      ref Heightmap heightmap,
      ref Collider waterSurface,
      bool water)
    {
      int placeRayMask = __instance.m_placeRayMask;
      MoveableBaseRootComponent componentInParent = ((Component) __instance).GetComponentInParent<MoveableBaseRootComponent>();
      if (Object.op_Implicit((Object) componentInParent))
      {
        Vector3 vector3_1 = Vector3.op_Addition(((Component) componentInParent).transform.InverseTransformPoint(((Component) __instance).transform.position), Vector3.op_Multiply(Vector3.up, 2f));
        Vector3 vector3_2 = ((Component) componentInParent).transform.TransformPoint(vector3_1);
        Quaternion lookYaw = ((Character) __instance).m_lookYaw;
        double lookPitch = (double) __instance.m_lookPitch;
        Quaternion rotation = ((Component) componentInParent).transform.rotation;
        double num = -(double) ((Quaternion) ref rotation).eulerAngles.y + (double) ValheimRAFT_Patch.yawOffset;
        Quaternion quaternion1 = Quaternion.Euler((float) lookPitch, (float) num, 0.0f);
        Quaternion quaternion2 = Quaternion.op_Multiply(lookYaw, quaternion1);
        Vector3 vector3_3 = Quaternion.op_Multiply(Quaternion.op_Multiply(((Component) componentInParent).transform.rotation, quaternion2), Vector3.forward);
        RaycastHit raycastHit;
        if (Physics.Raycast(vector3_2, vector3_3, ref raycastHit, 10f, placeRayMask) && Object.op_Implicit((Object) ((RaycastHit) ref raycastHit).collider) && Object.op_Implicit((Object) ((Component) ((RaycastHit) ref raycastHit).collider).GetComponentInParent<MoveableBaseRootComponent>()))
        {
          point = ((RaycastHit) ref raycastHit).point;
          normal = ((RaycastHit) ref raycastHit).normal;
          piece = ((Component) ((RaycastHit) ref raycastHit).collider).GetComponentInParent<Piece>();
          heightmap = (Heightmap) null;
          waterSurface = (Collider) null;
          __result = true;
          return false;
        }
      }
      return true;
    }

    [HarmonyPatch(typeof (Player), "PieceRayTest")]
    [HarmonyPostfix]
    private static void PieceRayTestPostfix(
      Player __instance,
      ref bool __result,
      ref Vector3 point,
      ref Vector3 normal,
      ref Piece piece,
      ref Heightmap heightmap,
      ref Collider waterSurface,
      bool water)
    {
      ValheimRAFT_Patch.m_lastRayPiece = piece;
    }

    [HarmonyPatch(typeof (WearNTear), "UpdateSupport")]
    [HarmonyPrefix]
    private static bool UpdateSupport(WearNTear __instance)
    {
      if (!((Behaviour) __instance).isActiveAndEnabled)
        return false;
      if (!Object.op_Implicit((Object) ((Component) __instance).GetComponentInParent<MoveableBaseRootComponent>()))
        return true;
      if ((double) ((Component) __instance).transform.localPosition.y >= 1.0)
        return false;
      __instance.m_nview.GetZDO().Set("support", 1500f);
      return false;
    }

    [HarmonyPatch(typeof (Player), "FindHoverObject")]
    [HarmonyPrefix]
    private static bool FindHoverObject(
      Player __instance,
      ref GameObject hover,
      ref Character hoverCreature)
    {
      hover = (GameObject) null;
      hoverCreature = (Character) null;
      RaycastHit[] array = Physics.RaycastAll(((Component) GameCamera.instance).transform.position, ((Component) GameCamera.instance).transform.forward, 50f, __instance.m_interactMask);
      Array.Sort<RaycastHit>(array, (Comparison<RaycastHit>) ((x, y) => ((RaycastHit) ref x).distance.CompareTo(((RaycastHit) ref y).distance)));
      foreach (RaycastHit raycastHit in array)
      {
        if (!Object.op_Implicit((Object) ((RaycastHit) ref raycastHit).collider.attachedRigidbody) || !Object.op_Equality((Object) ((Component) ((RaycastHit) ref raycastHit).collider.attachedRigidbody).gameObject, (Object) ((Component) __instance).gameObject))
        {
          if (Object.op_Equality((Object) hoverCreature, (Object) null))
          {
            Character character = Object.op_Implicit((Object) ((RaycastHit) ref raycastHit).collider.attachedRigidbody) ? ((Component) ((RaycastHit) ref raycastHit).collider.attachedRigidbody).GetComponent<Character>() : ((Component) ((RaycastHit) ref raycastHit).collider).GetComponent<Character>();
            if (Object.op_Inequality((Object) character, (Object) null))
              hoverCreature = character;
          }
          if ((double) Vector3.Distance(((Character) __instance).m_eye.position, ((RaycastHit) ref raycastHit).point) < (double) __instance.m_maxInteractDistance)
          {
            if (((Component) ((RaycastHit) ref raycastHit).collider).GetComponent<Hoverable>() != null)
            {
              hover = ((Component) ((RaycastHit) ref raycastHit).collider).gameObject;
            }
            else
            {
              int num = !Object.op_Implicit((Object) ((RaycastHit) ref raycastHit).collider.attachedRigidbody) ? 0 : (!Object.op_Implicit((Object) ((Component) ((RaycastHit) ref raycastHit).collider.attachedRigidbody).GetComponent<MoveableBaseRootComponent>()) ? 1 : 0);
              hover = num == 0 ? ((Component) ((RaycastHit) ref raycastHit).collider).gameObject : ((Component) ((RaycastHit) ref raycastHit).collider.attachedRigidbody).gameObject;
            }
            break;
          }
          break;
        }
      }
      return false;
    }

    [HarmonyPatch(typeof (CharacterAnimEvent), "OnAnimatorIK")]
    [HarmonyPrefix]
    private static bool OnAnimatorIK(CharacterAnimEvent __instance, int layerIndex)
    {
      if (__instance.m_character is Player character && ((Character) character).IsAttached() && Object.op_Implicit((Object) character.m_attachPoint) && Object.op_Implicit((Object) character.m_attachPoint.parent))
      {
        RudderComponent component1 = ((Component) character.m_attachPoint.parent).GetComponent<RudderComponent>();
        if (Object.op_Implicit((Object) component1))
          component1.UpdateIK(((Character) character).m_animator);
        RopeLadderComponent component2 = ((Component) character.m_attachPoint.parent).GetComponent<RopeLadderComponent>();
        if (Object.op_Implicit((Object) component2))
        {
          component2.UpdateIK(((Character) character).m_animator);
          return false;
        }
      }
      return true;
    }

    [HarmonyPatch(typeof (Player), "AttachStop")]
    [HarmonyPrefix]
    private static void AttachStop(Player __instance)
    {
      if (!((Character) __instance).IsAttached() || !Object.op_Implicit((Object) __instance.m_attachPoint) || !Object.op_Implicit((Object) __instance.m_attachPoint.parent))
        return;
      RopeLadderComponent component = ((Component) __instance.m_attachPoint.parent).GetComponent<RopeLadderComponent>();
      if (Object.op_Implicit((Object) component))
        component.StepOffLadder(__instance);
      ((Character) __instance).m_animator.SetIKPositionWeight((AvatarIKGoal) 2, 0.0f);
      ((Character) __instance).m_animator.SetIKPositionWeight((AvatarIKGoal) 3, 0.0f);
      ((Character) __instance).m_animator.SetIKRotationWeight((AvatarIKGoal) 2, 0.0f);
      ((Character) __instance).m_animator.SetIKRotationWeight((AvatarIKGoal) 3, 0.0f);
    }

    [HarmonyPatch(typeof (Player), "SetControls")]
    [HarmonyPrefix]
    private static bool SetControls(
      Player __instance,
      Vector3 movedir,
      bool attack,
      bool attackHold,
      bool secondaryAttack,
      bool block,
      bool blockHold,
      bool jump,
      bool crouch,
      bool run,
      bool autoRun)
    {
      if (((Character) __instance).IsAttached() && Object.op_Implicit((Object) __instance.m_attachPoint) && Object.op_Implicit((Object) __instance.m_attachPoint.parent))
      {
        if ((double) movedir.x == 0.0 && (double) movedir.y == 0.0 && !jump && !crouch && !attack && !attackHold && !secondaryAttack && !block)
        {
          RopeLadderComponent component = ((Component) __instance.m_attachPoint.parent).GetComponent<RopeLadderComponent>();
          if (Object.op_Implicit((Object) component))
          {
            component.MoveOnLadder(__instance, movedir.z);
            return false;
          }
        }
        if (Object.op_Implicit((Object) ((Component) __instance.m_attachPoint.parent).GetComponent<RudderComponent>()) && __instance.m_doodadController != null)
        {
          __instance.SetDoodadControlls(ref movedir, ref ((Character) __instance).m_lookDir, ref run, ref autoRun, blockHold);
          if (__instance.m_doodadController is ShipControlls doodadController && Object.op_Implicit((Object) doodadController.m_ship))
          {
            MoveableBaseShipComponent component = ((Component) doodadController.m_ship).GetComponent<MoveableBaseShipComponent>();
            if (Object.op_Implicit((Object) component))
            {
              if (ZInput.GetButton("Jump") || ZInput.GetButton("JoyJump"))
                component.Accend();
              else if (ZInput.GetButton("Crouch") || ZInput.GetButton("JoyCrouch"))
                component.Descent();
              else if (ZInput.GetButtonDown("Run") || ZInput.GetButtonDown("JoyRun"))
                component.ToggleAnchor();
              if (component.m_anchored && Vector3.op_Inequality(movedir, Vector3.zero))
                component.ToggleAnchor();
            }
          }
          return false;
        }
      }
      return true;
    }

    [HarmonyPatch(typeof (Character), "OnCollisionStay")]
    [HarmonyPrefix]
    private static bool OnCollisionStay(Character __instance, Collision collision)
    {
      if (!__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner() || (double) __instance.m_jumpTimer < 0.100000001490116)
        return false;
      foreach (ContactPoint contact in collision.contacts)
      {
        Vector3 vector3_1 = ((ContactPoint) ref contact).normal;
        Vector3 vector3_2 = ((ContactPoint) ref contact).point;
        float num = Mathf.Abs(vector3_2.y - ((Component) __instance).transform.position.y);
        if (!__instance.m_groundContact && (double) vector3_1.y < 0.0 && (double) num < 0.100000001490116)
        {
          vector3_1 = Vector3.op_Multiply(vector3_1, -1f);
          vector3_2 = ((Component) __instance).transform.position;
        }
        if ((double) vector3_1.y > 0.100000001490116 && (double) num < (double) __instance.m_collider.radius)
        {
          if ((double) vector3_1.y > (double) __instance.m_groundContactNormal.y || !__instance.m_groundContact)
          {
            __instance.m_groundContact = true;
            __instance.m_groundContactNormal = vector3_1;
            __instance.m_groundContactPoint = vector3_2;
            __instance.m_lowestContactCollider = collision.collider;
          }
          else
          {
            Vector3 vector3_3 = Vector3.Normalize(Vector3.op_Addition(__instance.m_groundContactNormal, vector3_1));
            if ((double) vector3_3.y > (double) __instance.m_groundContactNormal.y)
            {
              __instance.m_groundContactNormal = vector3_3;
              __instance.m_groundContactPoint = Vector3.op_Multiply(Vector3.op_Addition(__instance.m_groundContactPoint, vector3_2), 0.5f);
            }
          }
        }
      }
      return false;
    }
  }
}
