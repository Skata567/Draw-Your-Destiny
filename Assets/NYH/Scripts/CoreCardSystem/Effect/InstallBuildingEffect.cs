using NYH.CoreCardSystem;
using UnityEngine;

namespace NYH.CoreCardSystem
{
    [System.Serializable]
    public class InstallBuildingEffect : PlacementEffect
    {
        [Header("설치할 건물 데이터")]
        public BuildingData buildingData;

        public override Sprite GetPreviewSprite() => buildingData != null ? buildingData.sprite : null;

        public override GameAction GetGameAction(int effectIndex = 0, Card sourceCard = null)
        {
            // 좌표는 나중에 주입되므로 초기값은 zero로 생성합니다.
            return new PlayBuildingGA(buildingData, Vector3Int.zero);
        }
    }
}
