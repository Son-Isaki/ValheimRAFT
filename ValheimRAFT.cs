// Decompiled with JetBrains decompiler
// Type: ValheimRAFT.ValheimRAFT
// Assembly: ValheimRAFT, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 98A52806-B1EE-47F9-B4D8-4FBCE5F3450B
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins\ValheimRAFT\ValheimRAFT.dll

using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.U2D;

namespace ValheimRAFT
{
    [BepInPlugin("BepIn.Sarcen.ValheimRAFT", "ValheimRAFT", "1.3.0")]
    [BepInDependency]
    [NetworkCompatibility]
    public class ValheimRAFT : BaseUnityPlugin
    {
        internal const string Author = "Sarcen,Isaki";
        internal const string Name = "ValheimRAFT";
        internal const string Version = "1.3.0";
        internal const string BepInGUID = "BepIn.Sarcen.ValheimRAFT";
        internal const string HarmonyGUID = "Harmony.Sarcen.ValheimRAFT";
        internal static Harmony m_harmony;
        internal static int CustomRaftLayer = 29;
        private static AssetBundle m_assetBundle;
        private bool m_customItemsAdded;

        public static ValheimRAFT.ValheimRAFT Instance { get; private set; }

        public ConfigEntry<bool> MakeAllPiecesWaterProof { get; set; }

        public ConfigEntry<bool> AllowFlight { get; set; }

        private void Awake()
        {
            ValheimRAFT.ValheimRAFT.Instance = this;
            this.MakeAllPiecesWaterProof = this.Config.Bind<bool>("Server config", "MakeAllPiecesWaterProof", true, new ConfigDescription("Makes it so all building pieces (walls, floors, etc) on the ship don't take rain damage.", (AcceptableValueBase)null, new object[1]
            {
        (object) new ConfigurationManagerAttributes()
        {
          IsAdminOnly = true
        }
            }));
            this.AllowFlight = this.Config.Bind<bool>("Server config", "AllowFlight", false, new ConfigDescription("Allow the raft to fly (jump\\crouch to go up and down)", (AcceptableValueBase)null, new object[1]
            {
        (object) new ConfigurationManagerAttributes()
        {
          IsAdminOnly = true
        }
            }));
            ValheimRAFT.ValheimRAFT.m_harmony = new Harmony("Harmony.Sarcen.ValheimRAFT");
            ValheimRAFT.ValheimRAFT.m_harmony.PatchAll();
            int layer = LayerMask.NameToLayer("vehicle");
            for (int index = 0; index < 32; ++index)
                Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, index, Physics.GetIgnoreLayerCollision(layer, index));
            Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, LayerMask.NameToLayer("piece"), true);
            Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, LayerMask.NameToLayer("character"), true);
            Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, LayerMask.NameToLayer("smoke"), true);
            Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, LayerMask.NameToLayer("character_ghost"), true);
            Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, LayerMask.NameToLayer("weapon"), true);
            Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, LayerMask.NameToLayer("blocker"), true);
            Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, LayerMask.NameToLayer("pathblocker"), true);
            Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, LayerMask.NameToLayer("viewblock"), true);
            Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, LayerMask.NameToLayer("character_net"), true);
            Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, LayerMask.NameToLayer("character_noenv"), true);
            Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, LayerMask.NameToLayer("Default_small"), false);
            Physics.IgnoreLayerCollision(ValheimRAFT.ValheimRAFT.CustomRaftLayer, LayerMask.NameToLayer("Default"), false);
            ValheimRAFT.ValheimRAFT.m_assetBundle = AssetUtils.LoadAssetBundleFromResources("valheimraft", Assembly.GetExecutingAssembly());
            CommandManager.Instance.AddConsoleCommand((ConsoleCommand)new CreativeModeConsoleCommand());
        }

        internal void AddCustomPieces()
        {
            if (this.m_customItemsAdded)
                return;
            this.m_customItemsAdded = true;
            GameObject gameObject1 = ValheimRAFT.ValheimRAFT.m_assetBundle.LoadAsset<GameObject>("Assets/steering_wheel.prefab");
            GameObject gameObject2 = ValheimRAFT.ValheimRAFT.m_assetBundle.LoadAsset<GameObject>("Assets/rope_ladder.prefab");
            ValheimRAFT.ValheimRAFT.m_assetBundle.LoadAsset<GameObject>("Assets/rope_anchor.prefab");
            SpriteAtlas spriteAtlas = ValheimRAFT.ValheimRAFT.m_assetBundle.LoadAsset<SpriteAtlas>("Assets/icons.spriteatlas");
            PrefabManager instance1 = PrefabManager.Instance;
            GameObject prefab = ZNetScene.instance.GetPrefab("Raft");
            Piece component1 = ZNetScene.instance.GetPrefab("wood_floor").GetComponent<Piece>();
            WearNTear component2 = ((Component)component1).GetComponent<WearNTear>();
            GameObject gameObject3 = ((Component)prefab.transform.Find("ship/visual/mast")).gameObject;
            GameObject gameObject4 = ((Component)ZNetScene.instance.GetPrefab("Karve").transform.Find("ship/mast")).gameObject;
            GameObject gameObject5 = ((Component)ZNetScene.instance.GetPrefab("VikingShip").transform.Find("ship/visual/Mast")).gameObject;
            PieceManager instance2 = PieceManager.Instance;
            GameObject clonedPrefab1 = instance1.CreateClonedPrefab("MBRaft", prefab);
            ((Component)clonedPrefab1.transform.Find("ship/visual/mast")).gameObject.SetActive(false);
            ((Component)clonedPrefab1.transform.Find("interactive/mast")).gameObject.SetActive(false);
            clonedPrefab1.GetComponent<Rigidbody>().mass = 1000f;
            Object.Destroy((Object)((Component)clonedPrefab1.transform.Find("ship/colliders/log")).gameObject);
            Object.Destroy((Object)((Component)clonedPrefab1.transform.Find("ship/colliders/log (1)")).gameObject);
            Object.Destroy((Object)((Component)clonedPrefab1.transform.Find("ship/colliders/log (2)")).gameObject);
            Object.Destroy((Object)((Component)clonedPrefab1.transform.Find("ship/colliders/log (3)")).gameObject);
            Piece component3 = clonedPrefab1.GetComponent<Piece>();
            component3.m_name = "$mb_raft";
            component3.m_description = "$mb_raft_desc";
            clonedPrefab1.GetComponent<ZNetView>().m_persistent = true;
            WearNTear component4 = clonedPrefab1.GetComponent<WearNTear>();
            component4.m_health = 10000f;
            component4.m_noRoofWear = false;
            clonedPrefab1.GetComponent<ImpactEffect>().m_damageToSelf = false;
            PieceManager pieceManager1 = instance2;
            GameObject gameObject6 = clonedPrefab1;
            PieceConfig pieceConfig1 = new PieceConfig();
            pieceConfig1.PieceTable = "Hammer";
            pieceConfig1.Description = "$mb_raft_desc";
            pieceConfig1.Requirements = new RequirementConfig[1]
            {
        new RequirementConfig() { Amount = 20, Item = "Wood" }
            };
            PieceConfig pieceConfig2 = pieceConfig1;
            CustomPiece customPiece1 = new CustomPiece(gameObject6, false, pieceConfig2);
            pieceManager1.AddPiece(customPiece1);
            GameObject clonedPrefab2 = instance1.CreateClonedPrefab("MBRaftMast", gameObject3);
            Piece piece1 = clonedPrefab2.AddComponent<Piece>();
            piece1.m_name = "$mb_raft_mast";
            piece1.m_description = "$mb_raft_mast_desc";
            piece1.m_placeEffect = component1.m_placeEffect;
            clonedPrefab2.AddComponent<ZNetView>().m_persistent = true;
            MastComponent mastComponent1 = clonedPrefab2.AddComponent<MastComponent>();
            mastComponent1.m_sailObject = ((Component)clonedPrefab2.transform.Find("Sail")).gameObject;
            mastComponent1.m_sailCloth = mastComponent1.m_sailObject.GetComponentInChildren<Cloth>();
            WearNTear wearNtear1 = clonedPrefab2.AddComponent<WearNTear>();
            wearNtear1.m_health = 1000f;
            wearNtear1.m_destroyedEffect = component2.m_destroyedEffect;
            wearNtear1.m_hitEffect = component2.m_hitEffect;
            wearNtear1.m_noRoofWear = false;
            ValheimRAFT.ValheimRAFT.FixedRopes(clonedPrefab2);
            this.FixCollisionLayers(clonedPrefab2);
            PieceManager pieceManager2 = instance2;
            GameObject gameObject7 = clonedPrefab2;
            PieceConfig pieceConfig3 = new PieceConfig();
            pieceConfig3.PieceTable = "Hammer";
            pieceConfig3.Description = "$mb_raft_mast_desc";
            pieceConfig3.Icon = spriteAtlas.GetSprite("raftmast");
            pieceConfig3.Requirements = new RequirementConfig[2]
            {
        new RequirementConfig() { Amount = 10, Item = "Wood" },
        new RequirementConfig() { Amount = 6, Item = "DeerHide" }
            };
            PieceConfig pieceConfig4 = pieceConfig3;
            CustomPiece customPiece2 = new CustomPiece(gameObject7, false, pieceConfig4);
            pieceManager2.AddPiece(customPiece2);
            GameObject clonedPrefab3 = instance1.CreateClonedPrefab("MBKarveMast", gameObject4);
            Piece piece2 = clonedPrefab3.AddComponent<Piece>();
            piece2.m_name = "$mb_karve_mast";
            piece2.m_description = "$mb_karve_mast_desc";
            piece2.m_placeEffect = component1.m_placeEffect;
            clonedPrefab3.AddComponent<ZNetView>().m_persistent = true;
            MastComponent mastComponent2 = clonedPrefab3.AddComponent<MastComponent>();
            mastComponent2.m_sailObject = ((Component)clonedPrefab3.transform.Find("Sail")).gameObject;
            mastComponent2.m_sailCloth = mastComponent2.m_sailObject.GetComponentInChildren<Cloth>();
            WearNTear wearNtear2 = clonedPrefab3.AddComponent<WearNTear>();
            wearNtear2.m_health = 1000f;
            wearNtear2.m_noRoofWear = false;
            wearNtear2.m_destroyedEffect = component2.m_destroyedEffect;
            wearNtear2.m_hitEffect = component2.m_hitEffect;
            ValheimRAFT.ValheimRAFT.FixedRopes(clonedPrefab3);
            this.FixCollisionLayers(clonedPrefab3);
            PieceManager pieceManager3 = instance2;
            GameObject gameObject8 = clonedPrefab3;
            PieceConfig pieceConfig5 = new PieceConfig();
            pieceConfig5.PieceTable = "Hammer";
            pieceConfig5.Description = "$mb_karve_mast_desc";
            pieceConfig5.Icon = spriteAtlas.GetSprite("karvemast");
            pieceConfig5.Requirements = new RequirementConfig[3]
            {
        new RequirementConfig() { Amount = 10, Item = "FineWood" },
        new RequirementConfig() { Amount = 2, Item = "RoundLog" },
        new RequirementConfig() { Amount = 6, Item = "TrollHide" }
            };
            PieceConfig pieceConfig6 = pieceConfig5;
            CustomPiece customPiece3 = new CustomPiece(gameObject8, false, pieceConfig6);
            pieceManager3.AddPiece(customPiece3);
            GameObject clonedPrefab4 = instance1.CreateClonedPrefab("MBVikingShipMast", gameObject5);
            Piece piece3 = clonedPrefab4.AddComponent<Piece>();
            piece3.m_name = "$mb_vikingship_mast";
            piece3.m_description = "$mb_vikingship_mast_desc";
            piece3.m_placeEffect = component1.m_placeEffect;
            clonedPrefab4.AddComponent<ZNetView>().m_persistent = true;
            MastComponent mastComponent3 = clonedPrefab4.AddComponent<MastComponent>();
            mastComponent3.m_sailObject = ((Component)clonedPrefab4.transform.Find("Sail")).gameObject;
            mastComponent3.m_sailCloth = mastComponent3.m_sailObject.GetComponentInChildren<Cloth>();
            WearNTear wearNtear3 = clonedPrefab4.AddComponent<WearNTear>();
            wearNtear3.m_health = 1000f;
            wearNtear3.m_noRoofWear = false;
            wearNtear3.m_destroyedEffect = component2.m_destroyedEffect;
            wearNtear3.m_hitEffect = component2.m_hitEffect;
            ValheimRAFT.ValheimRAFT.FixedRopes(clonedPrefab4);
            this.FixCollisionLayers(clonedPrefab4);
            PieceManager pieceManager4 = instance2;
            GameObject gameObject9 = clonedPrefab4;
            PieceConfig pieceConfig7 = new PieceConfig();
            pieceConfig7.PieceTable = "Hammer";
            pieceConfig7.Description = "$mb_vikingship_mast_desc";
            pieceConfig7.Icon = spriteAtlas.GetSprite("vikingmast");
            pieceConfig7.Requirements = new RequirementConfig[3]
            {
        new RequirementConfig() { Amount = 10, Item = "FineWood" },
        new RequirementConfig() { Amount = 2, Item = "RoundLog" },
        new RequirementConfig() { Amount = 6, Item = "WolfPelt" }
            };
            PieceConfig pieceConfig8 = pieceConfig7;
            CustomPiece customPiece4 = new CustomPiece(gameObject9, false, pieceConfig8);
            pieceManager4.AddPiece(customPiece4);
            GameObject clonedPrefab5 = instance1.CreateClonedPrefab("MBRudder", gameObject1);
            Piece piece4 = clonedPrefab5.AddComponent<Piece>();
            piece4.m_name = "$mb_rudder";
            piece4.m_description = "$mb_rudder_desc";
            piece4.m_placeEffect = component1.m_placeEffect;
            clonedPrefab5.AddComponent<ZNetView>().m_persistent = true;
            RudderComponent rudderComponent = clonedPrefab5.AddComponent<RudderComponent>();
            rudderComponent.m_controls = clonedPrefab5.AddComponent<ShipControlls>();
            rudderComponent.m_controls.m_hoverText = "$mb_rudder_use";
            rudderComponent.m_controls.m_attachPoint = clonedPrefab5.transform.Find("attachpoint");
            rudderComponent.m_controls.m_attachAnimation = "Standing Torch Idle right";
            rudderComponent.m_controls.m_detachOffset = new Vector3(0.0f, 0.0f, 0.0f);
            rudderComponent.m_wheel = clonedPrefab5.transform.Find("controls/wheel");
            rudderComponent.UpdateSpokes();
            WearNTear wearNtear4 = clonedPrefab5.AddComponent<WearNTear>();
            wearNtear4.m_health = 1000f;
            wearNtear4.m_noRoofWear = false;
            wearNtear4.m_destroyedEffect = component2.m_destroyedEffect;
            wearNtear4.m_hitEffect = component2.m_hitEffect;
            this.FixCollisionLayers(clonedPrefab5);
            PieceManager pieceManager5 = instance2;
            GameObject gameObject10 = clonedPrefab5;
            PieceConfig pieceConfig9 = new PieceConfig();
            pieceConfig9.PieceTable = "Hammer";
            pieceConfig9.Description = "$mb_rudder_desc";
            pieceConfig9.Icon = spriteAtlas.GetSprite("steering_wheel");
            pieceConfig9.Requirements = new RequirementConfig[1]
            {
        new RequirementConfig() { Amount = 10, Item = "Wood" }
            };
            PieceConfig pieceConfig10 = pieceConfig9;
            CustomPiece customPiece5 = new CustomPiece(gameObject10, false, pieceConfig10);
            pieceManager5.AddPiece(customPiece5);
            GameObject clonedPrefab6 = instance1.CreateClonedPrefab("MBRopeLadder", gameObject2);
            Piece piece5 = clonedPrefab6.AddComponent<Piece>();
            piece5.m_name = "$mb_rope_ladder";
            piece5.m_description = "$mb_rope_ladder_desc";
            piece5.m_placeEffect = component1.m_placeEffect;
            ((StaticTarget)piece5).m_primaryTarget = false;
            ((StaticTarget)piece5).m_randomTarget = false;
            clonedPrefab6.AddComponent<ZNetView>().m_persistent = true;
            RopeLadderComponent ropeLadderComponent = clonedPrefab6.AddComponent<RopeLadderComponent>();
            LineRenderer componentInChildren = gameObject3.GetComponentInChildren<LineRenderer>(true);
            ropeLadderComponent.m_ropeLine = ((Component)ropeLadderComponent).GetComponent<LineRenderer>();
            ((Renderer)ropeLadderComponent.m_ropeLine).material = new Material(((Renderer)componentInChildren).material);
            ropeLadderComponent.m_ropeLine.textureMode = (LineTextureMode)1;
            ropeLadderComponent.m_ropeLine.widthMultiplier = 0.05f;
            ropeLadderComponent.m_stepObject = ((Component)((Component)ropeLadderComponent).transform.Find("step")).gameObject;
            ((Renderer)ropeLadderComponent.m_stepObject.GetComponentInChildren<MeshRenderer>()).material = new Material(((Renderer)((Component)component1).GetComponentInChildren<MeshRenderer>()).material);
            WearNTear wearNtear5 = clonedPrefab6.AddComponent<WearNTear>();
            wearNtear5.m_health = 10000f;
            wearNtear5.m_destroyedEffect = component2.m_destroyedEffect;
            wearNtear5.m_hitEffect = component2.m_hitEffect;
            wearNtear5.m_noRoofWear = false;
            wearNtear5.m_supports = false;
            this.FixCollisionLayers(clonedPrefab6);
            PieceManager pieceManager6 = instance2;
            GameObject gameObject11 = clonedPrefab6;
            PieceConfig pieceConfig11 = new PieceConfig();
            pieceConfig11.PieceTable = "Hammer";
            pieceConfig11.Description = "$mb_rope_ladder_desc";
            pieceConfig11.Icon = spriteAtlas.GetSprite("rope_ladder");
            pieceConfig11.Requirements = new RequirementConfig[1]
            {
        new RequirementConfig() { Amount = 10, Item = "Wood" }
            };
            PieceConfig pieceConfig12 = pieceConfig11;
            CustomPiece customPiece6 = new CustomPiece(gameObject11, false, pieceConfig12);
            pieceManager6.AddPiece(customPiece6);
        }

        private void FixCollisionLayers(GameObject r)
        {
            int layer = LayerMask.NameToLayer("piece");
            r.layer = layer;
            foreach (Component componentsInChild in ((Component)r.transform).GetComponentsInChildren<Transform>())
                componentsInChild.gameObject.layer = layer;
        }

        private static void FixedRopes(GameObject r)
        {
            LineAttach[] componentsInChildren = r.GetComponentsInChildren<LineAttach>();
            for (int index = 0; index < componentsInChildren.Length; ++index)
            {
                ((Component)componentsInChildren[index]).GetComponent<LineRenderer>().positionCount = 2;
                componentsInChildren[index].m_attachments.Clear();
                componentsInChildren[index].m_attachments.Add(r.transform);
            }
        }

        private void PrintCollisionMatrix()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.Append(" ".PadLeft(23));
            for (int index = 0; index < 32; ++index)
                stringBuilder.Append(index.ToString().PadRight(3));
            stringBuilder.AppendLine("");
            for (int index1 = 0; index1 < 32; ++index1)
            {
                stringBuilder.Append(LayerMask.LayerToName(index1).PadLeft(20) + index1.ToString().PadLeft(3));
                for (int index2 = 0; index2 < 32; ++index2)
                {
                    bool flag = !Physics.GetIgnoreLayerCollision(index1, index2);
                    stringBuilder.Append(flag ? "[X]" : "[ ]");
                }
                stringBuilder.AppendLine("");
            }
            stringBuilder.AppendLine("");
            ZLog.Log((object)stringBuilder.ToString());
        }
    }
}