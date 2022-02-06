// Decompiled with JetBrains decompiler
// Type: ValheimRAFT.MoveableBaseRootComponent
// Assembly: ValheimRAFT, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 98A52806-B1EE-47F9-B4D8-4FBCE5F3450B
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins\ValheimRAFT\ValheimRAFT.dll

using System.Collections.Generic;
using UnityEngine;

namespace ValheimRAFT
{
  public class MoveableBaseRootComponent : MonoBehaviour
  {
    public static readonly KeyValuePair<int, int> MBParentHash = ZDO.GetHashZDOID("MBParent");
    public static readonly int MBPositionHash = StringExtensionMethods.GetStableHashCode("MBPosition");
    public static readonly int MBRotationHash = StringExtensionMethods.GetStableHashCode("MBRotation");
    private static Dictionary<ZDOID, List<Piece>> m_pendingPieces = new Dictionary<ZDOID, List<Piece>>();
    internal MoveableBaseShipComponent m_moveableBaseShip;
    internal Rigidbody m_rigidbody;
    internal ZNetView m_nview;
    internal Rigidbody m_syncRigidbody;
    internal Ship m_ship;
    internal List<Piece> m_pieces = new List<Piece>();
    internal List<MastComponent> m_mastPieces = new List<MastComponent>();
    internal List<RudderComponent> m_rudderPieces = new List<RudderComponent>();
    internal List<Piece> m_portals = new List<Piece>();
    internal List<RopeLadderComponent> m_ladders = new List<RopeLadderComponent>();
    private Vector2i m_sector;
    private Bounds m_bounds = new Bounds();
    internal BoxCollider m_blockingcollider;
    internal BoxCollider m_floatcollider;
    internal BoxCollider m_onboardcollider;
    internal ZDOID m_id;
    public bool m_statsOverride;
    private float m_lastPortalUpdate;

    public void Awake()
    {
      this.m_rigidbody = ((Component) this).gameObject.AddComponent<Rigidbody>();
      this.m_rigidbody.isKinematic = true;
      this.m_rigidbody.interpolation = (RigidbodyInterpolation) 1;
      this.m_rigidbody.mass = 99999f;
    }

    public void CleanUp()
    {
      if (!Object.op_Implicit((Object) ZNetScene.instance) || !ZDOID.op_Inequality(this.m_id, ZDOID.None))
        return;
      for (int index = 0; index < this.m_pieces.Count; ++index)
      {
        Piece piece = this.m_pieces[index];
        if (Object.op_Implicit((Object) piece))
        {
          ((Component) piece).transform.SetParent((Transform) null);
          MoveableBaseRootComponent.AddInactivePiece(this.m_id, piece);
        }
      }
      List<Player> allPlayers = Player.GetAllPlayers();
      for (int index = 0; index < allPlayers.Count; ++index)
      {
        if (Object.op_Implicit((Object) allPlayers[index]) && Object.op_Equality((Object) ((Component) allPlayers[index]).transform.parent, (Object) ((Component) this).transform))
          ((Component) allPlayers[index]).transform.SetParent((Transform) null);
      }
    }

    private void Sync()
    {
      if (!Object.op_Implicit((Object) this.m_syncRigidbody))
        return;
      this.m_rigidbody.MovePosition(((Component) this.m_syncRigidbody).transform.position);
      this.m_rigidbody.MoveRotation(((Component) this.m_syncRigidbody).transform.rotation);
    }

    public void FixedUpdate() => this.Sync();

    public void LateUpdate()
    {
      this.Sync();
      Vector2i zone = ZoneSystem.instance.GetZone(((Component) this).transform.position);
      if (Vector2i.op_Inequality(zone, this.m_sector))
      {
        this.m_sector = zone;
        this.UpdateAllPieces();
      }
      else
        this.UpdatePortals();
    }

    private void UpdatePortals()
    {
      if ((double) Time.time - (double) this.m_lastPortalUpdate <= 0.5)
        return;
      this.m_lastPortalUpdate = Time.time;
      for (int index = 0; index < this.m_portals.Count; ++index)
      {
        Piece portal = this.m_portals[index];
        if (!Object.op_Implicit((Object) portal) || !Object.op_Implicit((Object) portal.m_nview) || portal.m_nview.m_zdo == null)
        {
          this.m_pieces.RemoveAt(index);
          --index;
        }
        else
        {
          Vector3 position = portal.m_nview.m_zdo.GetPosition();
          Vector3 vector3 = Vector3.op_Subtraction(((Component) portal).transform.position, position);
          if ((double) ((Vector3) ref vector3).sqrMagnitude > 1.0)
            portal.m_nview.m_zdo.SetPosition(((Component) portal).transform.position);
        }
      }
    }

    internal float GetColliderBottom() => (float) ((double) ((Component) this.m_blockingcollider).transform.position.y + (double) this.m_blockingcollider.center.y - (double) this.m_blockingcollider.size.y / 2.0);

    internal void UpdateAllPieces()
    {
      for (int index = 0; index < this.m_pieces.Count; ++index)
      {
        Piece piece = this.m_pieces[index];
        if (!Object.op_Implicit((Object) piece))
        {
          this.m_pieces.RemoveAt(index);
          --index;
        }
        else
        {
          ZNetView component = ((Component) piece).GetComponent<ZNetView>();
          if (Object.op_Implicit((Object) component))
            component.m_zdo.SetPosition(((Component) this).transform.position);
        }
      }
    }

    public static void AddInactivePiece(ZDOID id, Piece piece)
    {
      List<Piece> pieceList;
      if (!MoveableBaseRootComponent.m_pendingPieces.TryGetValue(id, out pieceList))
      {
        pieceList = new List<Piece>();
        MoveableBaseRootComponent.m_pendingPieces.Add(id, pieceList);
      }
      pieceList.Add(piece);
      WearNTear component = ((Component) piece).GetComponent<WearNTear>();
      if (!Object.op_Implicit((Object) component))
        return;
      ((Behaviour) component).enabled = false;
    }

    public void RemovePiece(Piece piece)
    {
      if (!this.m_pieces.Remove(piece))
        return;
      MastComponent component1 = ((Component) piece).GetComponent<MastComponent>();
      if (Object.op_Implicit((Object) component1))
        this.m_mastPieces.Remove(component1);
      RudderComponent component2 = ((Component) piece).GetComponent<RudderComponent>();
      if (Object.op_Implicit((Object) component2))
        this.m_rudderPieces.Remove(component2);
      if (Object.op_Implicit((Object) ((Component) piece).GetComponent<TeleportWorld>()))
        this.m_portals.Remove(piece);
      RopeLadderComponent component3 = ((Component) piece).GetComponent<RopeLadderComponent>();
      if (Object.op_Implicit((Object) component3))
      {
        this.m_ladders.Remove(component3);
        component3.m_mbroot = (MoveableBaseRootComponent) null;
      }
      this.UpdateStats();
    }

    private void UpdateStats()
    {
    }

    public void DestroyPiece(WearNTear wnt)
    {
      this.RemovePiece(((Component) wnt).GetComponent<Piece>());
      this.UpdatePieceCount();
      if (this.GetPieceCount() != 0)
        return;
      ((Component) this.m_ship).GetComponent<WearNTear>().Destroy();
      Object.Destroy((Object) ((Component) this).gameObject);
    }

    public void ActivatePendingPieces()
    {
      if (!Object.op_Implicit((Object) this.m_nview) || this.m_nview.m_zdo == null)
        return;
      ZDOID uid = this.m_nview.m_zdo.m_uid;
      List<Piece> pieceList;
      if (!MoveableBaseRootComponent.m_pendingPieces.TryGetValue(uid, out pieceList))
        return;
      for (int index = 0; index < pieceList.Count; ++index)
      {
        Piece piece = pieceList[index];
        if (Object.op_Implicit((Object) piece))
          this.ActivatePiece(piece);
      }
      pieceList.Clear();
      MoveableBaseRootComponent.m_pendingPieces.Remove(uid);
    }

    public static void InitPiece(Piece piece)
    {
      Rigidbody componentInChildren = ((Component) piece).GetComponentInChildren<Rigidbody>();
      if (Object.op_Implicit((Object) componentInChildren) && !componentInChildren.isKinematic)
        return;
      ZDOID zdoid = piece.m_nview.m_zdo.GetZDOID(MoveableBaseRootComponent.MBParentHash);
      if (!ZDOID.op_Inequality(zdoid, ZDOID.None))
        return;
      GameObject instance = ZNetScene.instance.FindInstance(zdoid);
      if (Object.op_Implicit((Object) instance))
      {
        MoveableBaseShipComponent component = instance.GetComponent<MoveableBaseShipComponent>();
        if (Object.op_Implicit((Object) component) && Object.op_Implicit((Object) component.m_baseRoot))
          component.m_baseRoot.ActivatePiece(piece);
      }
      else
        MoveableBaseRootComponent.AddInactivePiece(zdoid, piece);
    }

    public void ActivatePiece(Piece piece)
    {
      ZNetView component1 = ((Component) piece).GetComponent<ZNetView>();
      if (!Object.op_Implicit((Object) component1))
        return;
      ((Component) piece).transform.SetParent(((Component) this).transform);
      ((Component) piece).transform.localPosition = component1.m_zdo.GetVec3(MoveableBaseRootComponent.MBPositionHash, ((Component) piece).transform.localPosition);
      ((Component) piece).transform.localRotation = component1.m_zdo.GetQuaternion(MoveableBaseRootComponent.MBRotationHash, ((Component) piece).transform.localRotation);
      WearNTear component2 = ((Component) piece).GetComponent<WearNTear>();
      if (Object.op_Implicit((Object) component2))
        ((Behaviour) component2).enabled = true;
      this.AddPiece(piece);
    }

    public void AddNewPiece(Piece piece)
    {
      ((Component) piece).transform.SetParent(((Component) this).transform);
      ZNetView component = ((Component) piece).GetComponent<ZNetView>();
      component.m_zdo.Set(MoveableBaseRootComponent.MBParentHash, this.m_nview.m_zdo.m_uid);
      component.m_zdo.Set(MoveableBaseRootComponent.MBPositionHash, ((Component) piece).transform.localPosition);
      component.m_zdo.Set(MoveableBaseRootComponent.MBRotationHash, ((Component) piece).transform.localRotation);
      this.AddPiece(piece);
    }

    public void AddPiece(Piece piece)
    {
      this.m_pieces.Add(piece);
      this.UpdatePieceCount();
      this.EncapsulateBounds(piece);
      WearNTear component1 = ((Component) piece).GetComponent<WearNTear>();
      if (Object.op_Implicit((Object) component1) && ValheimRAFT.ValheimRAFT.Instance.MakeAllPiecesWaterProof.Value)
        component1.m_noRoofWear = false;
      MastComponent component2 = ((Component) piece).GetComponent<MastComponent>();
      if (Object.op_Implicit((Object) component2))
        this.m_mastPieces.Add(component2);
      RudderComponent component3 = ((Component) piece).GetComponent<RudderComponent>();
      if (Object.op_Implicit((Object) component3))
      {
        if (!Object.op_Implicit((Object) component3.m_controls))
          component3.m_controls = ((Component) piece).GetComponentInChildren<ShipControlls>();
        if (!Object.op_Implicit((Object) component3.m_wheel))
          component3.m_wheel = ((Component) piece).transform.Find("controls/wheel");
        component3.m_controls.m_nview = this.m_nview;
        component3.m_controls.m_ship = ((Component) this.m_moveableBaseShip).GetComponent<Ship>();
        this.m_rudderPieces.Add(component3);
      }
      if (Object.op_Implicit((Object) ((Component) piece).GetComponent<TeleportWorld>()))
        this.m_portals.Add(piece);
      RopeLadderComponent component4 = ((Component) piece).GetComponent<RopeLadderComponent>();
      if (Object.op_Implicit((Object) component4))
      {
        this.m_ladders.Add(component4);
        component4.m_mbroot = this;
      }
      foreach (MeshRenderer componentsInChild in ((Component) piece).GetComponentsInChildren<MeshRenderer>(true))
      {
        if (Object.op_Implicit((Object) ((Renderer) componentsInChild).sharedMaterial))
        {
          Material[] sharedMaterials = ((Renderer) componentsInChild).sharedMaterials;
          for (int index = 0; index < sharedMaterials.Length; ++index)
          {
            Material material = new Material(sharedMaterials[index]);
            material.SetFloat("_RippleDistance", 0.0f);
            material.SetFloat("_ValueNoise", 0.0f);
            sharedMaterials[index] = material;
          }
          ((Renderer) componentsInChild).sharedMaterials = sharedMaterials;
        }
      }
      Rigidbody[] componentsInChildren = ((Component) piece).GetComponentsInChildren<Rigidbody>();
      for (int index = 0; index < componentsInChildren.Length; ++index)
      {
        if (componentsInChildren[index].isKinematic)
          Object.Destroy((Object) componentsInChildren[index]);
      }
      this.UpdateStats();
    }

    private void UpdatePieceCount()
    {
      if (!Object.op_Implicit((Object) this.m_nview) || this.m_nview.m_zdo == null)
        return;
      this.m_nview.m_zdo.Set("MBPieceCount", this.m_pieces.Count);
    }

    public void EncapsulateBounds(Piece piece)
    {
      List<Collider> allColliders = ((StaticTarget) piece).GetAllColliders();
      Door componentInChildren = ((Component) piece).GetComponentInChildren<Door>();
      RopeLadderComponent component1 = ((Component) piece).GetComponent<RopeLadderComponent>();
      RopeAnchorComponent component2 = ((Component) piece).GetComponent<RopeAnchorComponent>();
      if (!Object.op_Implicit((Object) componentInChildren) && !Object.op_Implicit((Object) component1) && !Object.op_Implicit((Object) component2))
        ((Bounds) ref this.m_bounds).Encapsulate(((Component) piece).transform.localPosition);
      for (int index = 0; index < allColliders.Count; ++index)
      {
        Physics.IgnoreCollision(allColliders[index], (Collider) this.m_blockingcollider, true);
        Physics.IgnoreCollision(allColliders[index], (Collider) this.m_floatcollider, true);
        Physics.IgnoreCollision(allColliders[index], (Collider) this.m_onboardcollider, true);
      }
      this.m_blockingcollider.size = new Vector3(((Bounds) ref this.m_bounds).size.x, 3f, ((Bounds) ref this.m_bounds).size.z);
      this.m_blockingcollider.center = new Vector3(((Bounds) ref this.m_bounds).center.x, -0.2f, ((Bounds) ref this.m_bounds).center.z);
      this.m_floatcollider.size = new Vector3(((Bounds) ref this.m_bounds).size.x, 3f, ((Bounds) ref this.m_bounds).size.z);
      this.m_floatcollider.center = new Vector3(((Bounds) ref this.m_bounds).center.x, -0.2f, ((Bounds) ref this.m_bounds).center.z);
      this.m_onboardcollider.size = ((Bounds) ref this.m_bounds).size;
      this.m_onboardcollider.center = ((Bounds) ref this.m_bounds).center;
    }

    internal int GetPieceCount() => !Object.op_Implicit((Object) this.m_nview) || this.m_nview.m_zdo == null ? this.m_pieces.Count : this.m_nview.m_zdo.GetInt("MBPieceCount", this.m_pieces.Count);
  }
}
