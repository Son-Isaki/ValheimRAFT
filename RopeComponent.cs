// Decompiled with JetBrains decompiler
// Type: ValheimRAFT.RopeComponent
// Assembly: ValheimRAFT, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 98A52806-B1EE-47F9-B4D8-4FBCE5F3450B
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins\ValheimRAFT\ValheimRAFT.dll

using UnityEngine;

namespace ValheimRAFT
{
  public class RopeComponent : MonoBehaviour, Interactable, Hoverable
  {
    public string GetHoverName() => "";

    public string GetHoverText() => "$mb_rope_use";

    public bool Interact(Humanoid user, bool hold, bool alt) => true;

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;
  }
}
