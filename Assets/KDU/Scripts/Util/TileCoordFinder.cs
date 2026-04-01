using UnityEngine;
using UnityEngine.Tilemaps;

// 씬 뷰에서 클릭한 타일 좌표를 Console에 출력하는 임시 디버그 도구
// 좌표 확인 후 이 파일은 삭제해도 됩니다
public class TileCoordFinder : MonoBehaviour
{
    public Tilemap targetTilemap;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        Vector3Int cellPos = targetTilemap.WorldToCell(worldPos);

        Debug.Log($"[TileCoordFinder] 클릭 타일 좌표: {cellPos}  →  centerTilePos에 넣을 값: ({cellPos.x + 3}, {cellPos.y + 3}, 0)");
    }
}
