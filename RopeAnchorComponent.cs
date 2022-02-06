// Decompiled with JetBrains decompiler
// Type: ValheimRAFT.RopeAnchorComponent
// Assembly: ValheimRAFT, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 98A52806-B1EE-47F9-B4D8-4FBCE5F3450B
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins\ValheimRAFT\ValheimRAFT.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ValheimRAFT
{
  public class RopeAnchorComponent : MonoBehaviour, Interactable, Hoverable
  {
    public float m_maxRopeDistance = 64f;
    internal LineRenderer m_rope;
    internal ZNetView m_nview;
    private static RopeAnchorComponent m_draggingRopeFrom;
    private List<RopeAnchorComponent.Rope> m_ropes = new List<RopeAnchorComponent.Rope>();
    private uint m_zdoDataRevision;
    private float m_lastRopeCheckTime;

    public void Awake()
    {
      this.m_rope = ((Component) this).GetComponent<LineRenderer>();
      this.m_nview = ((Component) this).GetComponent<ZNetView>();
      ((Component) this).GetComponent<WearNTear>().m_onDestroyed += new Action(this.DestroyAllRopes);
      this.LoadFromZDO();
    }

    private void DestroyAllRopes()
    {
      while (this.m_ropes.Count > 0)
        this.RemoveRopeAt(0);
    }

    public string GetHoverName() => "";

    public string GetHoverText() => Localization.instance.Localize("$mb_rope_anchor_attach");

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
      if (!Object.op_Implicit((Object) RopeAnchorComponent.m_draggingRopeFrom))
      {
        RopeAnchorComponent.m_draggingRopeFrom = this;
        ((Renderer) this.m_rope).enabled = true;
      }
      else if (Object.op_Equality((Object) RopeAnchorComponent.m_draggingRopeFrom, (Object) this))
      {
        RopeAnchorComponent.m_draggingRopeFrom = (RopeAnchorComponent) null;
        ((Renderer) this.m_rope).enabled = false;
      }
      else
      {
        RopeAnchorComponent.m_draggingRopeFrom.AttachRope(this);
        ((Renderer) RopeAnchorComponent.m_draggingRopeFrom.m_rope).enabled = false;
        RopeAnchorComponent.m_draggingRopeFrom = (RopeAnchorComponent) null;
        ((Renderer) this.m_rope).enabled = false;
      }
      return true;
    }

    private void AttachRope(RopeAnchorComponent ropeAnchorComponent)
    {
      if (this.RemoveRopeWithID(ropeAnchorComponent.GetParentID()) || ropeAnchorComponent.RemoveRopeWithID(this.GetParentID()))
        return;
      this.CreateNewRope(ropeAnchorComponent.GetParentID());
      this.SaveToZDO();
      this.CheckRopes();
    }

    private void CreateNewRope(ZDOID targetID)
    {
      RopeAnchorComponent.Rope rope = new RopeAnchorComponent.Rope()
      {
        m_ropeAnchorTargetID = targetID,
        m_ropeObject = new GameObject()
      };
      rope.m_ropeObject.layer = LayerMask.NameToLayer("piece");
      rope.m_collider = rope.m_ropeObject.AddComponent<BoxCollider>();
      rope.m_ropeComponent = rope.m_ropeObject.AddComponent<RopeComponent>();
      rope.m_rope = rope.m_ropeObject.AddComponent<LineRenderer>();
      rope.m_rope.widthMultiplier = this.m_rope.widthMultiplier;
      ((Renderer) rope.m_rope).material = ((Renderer) this.m_rope).material;
      rope.m_rope.textureMode = (LineTextureMode) 1;
      rope.m_ropeObject.transform.SetParent(((Component) this).transform.parent);
      this.m_ropes.Add(rope);
    }

    private ZDOID GetParentID() => !Object.op_Implicit((Object) this.m_nview) || this.m_nview.m_zdo == null ? ZDOID.None : this.m_nview.m_zdo.m_uid;

    private bool RemoveRopeWithID(ZDOID zdoid)
    {
      for (int index = 0; index < this.m_ropes.Count; ++index)
      {
        if (ZDOID.op_Equality(this.m_ropes[index].m_ropeAnchorTargetID, zdoid))
        {
          this.RemoveRopeAt(index);
          return true;
        }
      }
      return false;
    }

    private void SaveToZDO()
    {
      if (!Object.op_Implicit((Object) this.m_nview) || this.m_nview.m_zdo == null)
        return;
      ZPackage zpackage = new ZPackage();
      for (int index = 0; index < this.m_ropes.Count; ++index)
        zpackage.Write(this.m_ropes[index].m_ropeAnchorTargetID);
      this.m_nview.m_zdo.Set("MBRopeAnchor_Ropes", zpackage.GetArray());
    }

    private void LoadFromZDO()
    {
      if (!Object.op_Implicit((Object) this.m_nview) || this.m_nview.m_zdo == null)
        return;
      List<ZDOID> ropeIds = new List<ZDOID>();
      this.GetRopesFromZDO((ICollection<ZDOID>) ropeIds);
      for (int index = 0; index < ropeIds.Count; ++index)
        this.CreateNewRope(ropeIds[index]);
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;

    public void LateUpdate()
    {
      if (!Object.op_Implicit((Object) this.m_nview) || this.m_nview.m_zdo == null)
        return;
      if (!this.m_nview.IsOwner() && (int) this.m_nview.m_zdo.m_dataRevision != (int) this.m_zdoDataRevision)
        this.UpdateRopesFromZDO();
      if (((Renderer) this.m_rope).enabled && Object.op_Implicit((Object) Player.m_localPlayer))
      {
        this.m_rope.SetPosition(0, ((Component) this).transform.position);
        this.m_rope.SetPosition(1, ((Component) ((Humanoid) Player.m_localPlayer).m_visEquipment.m_rightHand).transform.position);
      }
      if ((double) Time.time - (double) this.m_lastRopeCheckTime > 2.0)
      {
        this.CheckRopes();
        this.m_lastRopeCheckTime = Time.time;
      }
      for (int index = 0; index < this.m_ropes.Count; ++index)
      {
        RopeAnchorComponent.Rope rope = this.m_ropes[index];
        if (Object.op_Implicit((Object) rope.m_ropeTarget))
        {
          Vector3 vector3 = Vector3.op_Subtraction(((Component) this).transform.position, rope.m_ropeTarget.transform.position);
          float magnitude = ((Vector3) ref vector3).magnitude;
          if (this.m_nview.IsOwner() && ((double) magnitude > (double) this.m_maxRopeDistance || (double) Mathf.Abs(magnitude - rope.m_ropeAttachDistance) > 3.0))
          {
            this.RemoveRopeAt(index);
            --index;
          }
          else
          {
            rope.m_collider.size = new Vector3(this.m_rope.widthMultiplier, magnitude, this.m_rope.widthMultiplier);
            ((Component) rope.m_collider).transform.rotation = Quaternion.LookRotation(vector3);
            ((Component) rope.m_collider).transform.position = Vector3.op_Addition(((Component) this).transform.position, Vector3.op_Multiply(vector3, 0.5f));
            rope.m_rope.SetPosition(0, ((Component) this).transform.position);
            rope.m_rope.SetPosition(1, rope.m_ropeTarget.transform.position);
          }
        }
      }
    }

    private void CheckRopes()
    {
      for (int index = 0; index < this.m_ropes.Count; ++index)
      {
        RopeAnchorComponent.Rope rope1 = this.m_ropes[index];
        if (!Object.op_Implicit((Object) rope1.m_ropeTarget))
        {
          rope1.m_ropeTarget = ZNetScene.instance.FindInstance(rope1.m_ropeAnchorTargetID);
          if (!Object.op_Implicit((Object) rope1.m_ropeTarget))
          {
            if (ZNet.instance.IsServer() && ZDOMan.instance.GetZDO(rope1.m_ropeAnchorTargetID) == null)
            {
              this.RemoveRopeAt(index);
              --index;
            }
          }
          else
          {
            RopeAnchorComponent.Rope rope2 = rope1;
            Vector3 vector3 = Vector3.op_Subtraction(((Component) this).transform.position, rope1.m_ropeTarget.transform.position);
            double magnitude = (double) ((Vector3) ref vector3).magnitude;
            rope2.m_ropeAttachDistance = (float) magnitude;
          }
        }
      }
    }

    private void UpdateRopesFromZDO()
    {
      this.m_zdoDataRevision = this.m_nview.m_zdo.m_dataRevision;
      HashSet<ZDOID> ropeIds = new HashSet<ZDOID>();
      this.GetRopesFromZDO((ICollection<ZDOID>) ropeIds);
      for (int index = 0; index < this.m_ropes.Count; ++index)
      {
        RopeAnchorComponent.Rope rope = this.m_ropes[index];
        if (!ropeIds.Contains(rope.m_ropeAnchorTargetID))
        {
          this.RemoveRopeAt(index);
          --index;
        }
      }
      foreach (ZDOID zdoid in ropeIds)
      {
        ZDOID ropeId = zdoid;
        if (!this.m_ropes.Any<RopeAnchorComponent.Rope>((Func<RopeAnchorComponent.Rope, bool>) (k => ZDOID.op_Equality(k.m_ropeAnchorTargetID, ropeId))))
          this.CreateNewRope(ropeId);
      }
    }

    private void GetRopesFromZDO(ICollection<ZDOID> ropeIds)
    {
      byte[] byteArray = this.m_nview.m_zdo.GetByteArray("MBRopeAnchor_Ropes");
      if (byteArray == null || (uint) byteArray.Length <= 0U)
        return;
      ZPackage zpackage = new ZPackage(byteArray);
      while (zpackage.GetPos() < zpackage.Size())
        ropeIds.Add(zpackage.ReadZDOID());
    }

    private void RemoveRopeAt(int i)
    {
      Object.Destroy((Object) this.m_ropes[i].m_ropeObject);
      this.m_ropes.RemoveAt(i);
      this.SaveToZDO();
    }

    private class Rope
    {
      internal GameObject m_ropeTarget;
      internal ZDOID m_ropeAnchorTargetID;
      internal GameObject m_ropeObject;
      internal LineRenderer m_rope;
      internal BoxCollider m_collider;
      internal float m_ropeAttachDistance;
      internal RopeComponent m_ropeComponent;
    }
  }
}
