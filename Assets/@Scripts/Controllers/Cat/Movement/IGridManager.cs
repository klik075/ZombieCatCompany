using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 그리드 시스템 인터페이스 (다른 프로젝트에서도 재사용 가능)
/// </summary>
public interface IGridManager
{
    /// <summary>
    /// 특정 위치로 이동 가능한지 확인
    /// </summary>
    bool CanMoveTo(Vector2Int position);
    
    /// <summary>
    /// 특정 위치가 걸을 수 있는 타일인지 확인 (점유 무시)
    /// </summary>
    bool IsWalkable(Vector2Int position);
    
    /// <summary>
    /// 위치 점유
    /// </summary>
    void Occupy(Vector2Int position, Cat occupant);
    
    /// <summary>
    /// 위치 점유 해제
    /// </summary>
    void Release(Vector2Int position);
    
    /// <summary>
    /// 셀 좌표를 월드 좌표로 변환
    /// </summary>
    Vector3 CellToWorld(Vector2Int cell);
    
    /// <summary>
    /// 월드 좌표를 셀 좌표로 변환
    /// </summary>
    Vector2Int WorldToCell(Vector3 pos);
    
    /// <summary>
    /// A* 경로 찾기
    /// </summary>
    List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal);
    
    /// <summary>
    /// 걸을 수 있는 모든 셀 가져오기
    /// </summary>
    List<Vector2Int> GetWalkableCells();
}
