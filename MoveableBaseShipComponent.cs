// Decompiled with JetBrains decompiler
// Type: ValheimRAFT.MoveableBaseShipComponent
// Assembly: ValheimRAFT, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 98A52806-B1EE-47F9-B4D8-4FBCE5F3450B
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins\ValheimRAFT\ValheimRAFT.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ValheimRAFT
{
  public class MoveableBaseShipComponent : MonoBehaviour
  {
    internal MoveableBaseRootComponent m_baseRoot;
    internal Rigidbody m_rigidbody;
    internal Ship m_ship;
    internal ZNetView m_nview;
    internal GameObject m_baseRootObject;
    internal ZSyncTransform m_zsync;
    public float m_targetHeight;
    public float m_balanceForce = 0.03f;
    public float m_liftForce = 20f;
    internal bool m_anchored;

    public void Awake()
    {
      Ship component = ((Component) this).GetComponent<Ship>();
      this.m_nview = ((Component) this).GetComponent<ZNetView>();
      this.m_zsync = ((Component) this).GetComponent<ZSyncTransform>();
      this.m_ship = ((Component) this).GetComponent<Ship>();
      GameObject gameObject = new GameObject();
      ((Object) gameObject).name = "MoveableBase";
      gameObject.layer = 0;
      this.m_baseRootObject = gameObject;
      this.m_baseRoot = this.m_baseRootObject.AddComponent<MoveableBaseRootComponent>();
      this.m_baseRoot.m_moveableBaseShip = this;
      this.m_baseRoot.m_nview = this.m_nview;
      this.m_baseRoot.m_ship = component;
      this.m_baseRoot.m_id = this.m_nview.m_zdo.m_uid;
      this.m_rigidbody = ((Component) this).GetComponent<Rigidbody>();
      this.m_baseRoot.m_syncRigidbody = this.m_rigidbody;
      this.m_rigidbody.mass = 1000f;
      this.m_baseRootObject.transform.SetParent((Transform) null);
      this.m_baseRootObject.transform.position = ((Component) this).transform.position;
      this.m_baseRootObject.transform.rotation = ((Component) this).transform.rotation;
      ((Component) ((Component) component).transform.Find("ship/visual/mast"))?.gameObject.SetActive(false);
      ((Component) ((Component) component).transform.Find("ship/colliders/log"))?.gameObject.SetActive(false);
      ((Component) ((Component) component).transform.Find("ship/colliders/log (1)"))?.gameObject.SetActive(false);
      ((Component) ((Component) component).transform.Find("ship/colliders/log (2)"))?.gameObject.SetActive(false);
      ((Component) ((Component) component).transform.Find("ship/colliders/log (3)"))?.gameObject.SetActive(false);
      this.m_baseRoot.m_onboardcollider = ((IEnumerable<BoxCollider>) ((Component) ((Component) this).transform).GetComponentsInChildren<BoxCollider>()).FirstOrDefault<BoxCollider>((Func<BoxCollider, bool>) (k => ((Object) ((Component) k).gameObject).name == "OnboardTrigger"));
      ((Component) this.m_baseRoot.m_onboardcollider).transform.localScale = new Vector3(1f, 1f, 1f);
      this.m_baseRoot.m_floatcollider = component.m_floatCollider;
      ((Component) this.m_baseRoot.m_floatcollider).transform.localScale = new Vector3(1f, 1f, 1f);
      this.m_baseRoot.m_blockingcollider = ((Component) ((Component) component).transform.Find("ship/colliders/Cube")).GetComponentInChildren<BoxCollider>();
      ((Component) this.m_baseRoot.m_blockingcollider).transform.localScale = new Vector3(1f, 1f, 1f);
      ((Component) this.m_baseRoot.m_blockingcollider).gameObject.layer = ValheimRAFT.ValheimRAFT.CustomRaftLayer;
      ((Component) ((Component) this.m_baseRoot.m_blockingcollider).transform.parent).gameObject.layer = ValheimRAFT.ValheimRAFT.CustomRaftLayer;
      this.m_baseRoot.ActivatePendingPieces();
      this.FirstTimeCreation();
    }

    public void OnDestroy()
    {
      if (!Object.op_Implicit((Object) this.m_baseRoot))
        return;
      this.m_baseRoot.CleanUp();
      Object.Destroy((Object) ((Component) this.m_baseRoot).gameObject);
    }

    private void FirstTimeCreation()
    {
      if (this.m_baseRoot.GetPieceCount() != 0)
        return;
      GameObject prefab = ZNetScene.instance.GetPrefab("wood_floor");
      for (float num1 = -1f; (double) num1 < 1.00999999046326; num1 += 2f)
      {
        for (float num2 = -2f; (double) num2 < 2.00999999046326; num2 += 2f)
        {
          Vector3 vector3 = ((Component) this).transform.TransformPoint(new Vector3(num1, 1f, num2));
          this.m_baseRoot.AddNewPiece(Object.Instantiate<GameObject>(prefab, vector3, ((Component) this).transform.rotation).GetComponent<Piece>());
        }
      }
    }

    internal void Accend()
    {
      if (this.m_anchored)
        this.ToggleAnchor();
      if (!ValheimRAFT.ValheimRAFT.Instance.AllowFlight.Value)
      {
        this.m_targetHeight = 0.0f;
      }
      else
      {
        if (!Object.op_Implicit((Object) this.m_baseRoot) || !Object.op_Implicit((Object) this.m_baseRoot.m_floatcollider))
          return;
        this.m_targetHeight = Mathf.Clamp(((Component) this.m_baseRoot.m_floatcollider).transform.position.y + 1f, ZoneSystem.instance.m_waterLevel, 200f);
      }
      this.m_nview.m_zdo.Set("MBTargetHeight", this.m_targetHeight);
    }

    internal void Descent()
    {
      if (this.m_anchored)
        this.ToggleAnchor();
      float targetHeight = this.m_targetHeight;
      if (!ValheimRAFT.ValheimRAFT.Instance.AllowFlight.Value)
      {
        this.m_targetHeight = 0.0f;
      }
      else
      {
        if (!Object.op_Implicit((Object) this.m_baseRoot) || !Object.op_Implicit((Object) this.m_baseRoot.m_floatcollider))
          return;
        this.m_targetHeight = Mathf.Clamp(((Component) this.m_baseRoot.m_floatcollider).transform.position.y - 1f, ZoneSystem.instance.m_waterLevel, 200f);
        if ((double) ((Component) this.m_baseRoot.m_floatcollider).transform.position.y - 1.0 <= (double) ZoneSystem.instance.m_waterLevel)
          this.m_targetHeight = 0.0f;
      }
      this.m_nview.m_zdo.Set("MBTargetHeight", this.m_targetHeight);
    }

    internal void ToggleAnchor()
    {
      this.m_anchored = !this.m_nview.m_zdo.GetBool("MBAnchored", false);
      this.m_nview.m_zdo.Set("MBAnchored", this.m_anchored);
    }

    internal void UpdateStats(bool flight)
    {
      if (!Object.op_Implicit((Object) this.m_rigidbody) || !Object.op_Implicit((Object) this.m_baseRoot) || this.m_baseRoot.m_statsOverride)
        return;
      this.m_rigidbody.mass = 3000f;
      this.m_rigidbody.angularDrag = flight ? 1f : 0.0f;
      this.m_rigidbody.drag = flight ? 1f : 0.0f;
      if (Object.op_Implicit((Object) this.m_ship))
      {
        this.m_ship.m_angularDamping = flight ? 5f : 0.8f;
        this.m_ship.m_backwardForce = 1f;
        this.m_ship.m_damping = flight ? 5f : 0.35f;
        this.m_ship.m_dampingSideway = flight ? 3f : 0.3f;
        this.m_ship.m_force = 3f;
        this.m_ship.m_forceDistance = 5f;
        this.m_ship.m_sailForceFactor = flight ? 0.2f : 0.05f;
        this.m_ship.m_stearForce = flight ? 0.2f : 1f;
        this.m_ship.m_stearVelForceFactor = 1.3f;
        this.m_ship.m_waterImpactDamage = 0.0f;
        ImpactEffect component = ((Component) this.m_ship).GetComponent<ImpactEffect>();
        if (Object.op_Implicit((Object) component))
        {
          component.m_interval = 0.1f;
          component.m_minVelocity = 0.1f;
          component.m_damages.m_damage = 100f;
        }
      }
    }
  }
}
