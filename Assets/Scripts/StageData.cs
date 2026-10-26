using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "StageData_01", menuName = "Tetris/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Stage Info")]
    public int stageIndex = 1;             // 스테이지 번호
    public string stageTitle = "Stage 1";  // 스테이지 이름
    public Sprite stageIcon;

    [Header("Gimmick Toggles")]
    public bool sealIBlock = false;        // 1. I-블록 봉인
    public bool variableDropSpeed = false; // 2. 변속 낙하
    public bool isTimeAttack = false;      // 3. 미션 타임 어택
    public bool infectedBlock = false;     // 4. 감염 블록
    public bool earthquake = false;        // 5. 지진
    public bool disableHardDrop = false;   // 6. 하드 드롭 봉인

    [Header("Stage Rules")]
    public int targetLines = 40;           // 목표 라인 수
    public float timeAttackLimit = 60f;    // 타임어택 제한시간

    [Header("Gimmick Settings")]
    public float minDropDelay = 0.15f;
    public float maxDropDelay = 0.8f;
    public float infectionInterval = 6f;
    public Tile infectedTile;              // 감염 타일 자원
    public float earthquakeInterval = 10f;
}