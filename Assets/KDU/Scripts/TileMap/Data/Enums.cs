// 타일 지형 타입
public enum TileType
{
    Plain,      // 평지 (건설 가능)
    Mountain,   // 산 (건설 불가, 이동 불가)
    River,      // 강 (건설 불가, 이동 불가)
    Farmland,   // 농경지 (농업 전용)
    Resource    // 자원 타일
}

// 건물 타입
public enum BuildingType
{
    None,       // 빈 타일
    House,      // 민가 (인구 한도 증가)
    Market,     // 시장 (금 생산)
    Lab,        // 연구소 (연구 생산)
    Barracks,   // 병영 (유닛 생산)
    Farm        // 농장 (식량 생산)
}

// 안개 전쟁 상태
public enum FogState
{
    Hidden,     // 완전 암흑
    Explored,   // 지형만 공개
    Visible     // 완전 공개
}
