using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomFloorGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] _floorPrefabs; //4개의 바닥 프리팹 배열 
    private int _floorCount = 6;  //생성할 바닥의 개수
    private float _floorSize = 14f;  //바닥 크기 (가로, 세로가 동일하다고 가정)

    private List<Vector3> _floorPositionsList = new List<Vector3>();  //바닥 위치 추적

    void Start()
    {
        if (_floorPrefabs.Length > 0)
        {
            //_floorSize = _floorPrefabs[0].GetComponent<Renderer>().bounds.size.x;
            Debug.Log(_floorSize);
        }
        else
        {
            Debug.Log("프리팹이 존재하지 않습니다.");
        }
        GenerateFloors();
    }

    /// <summary>
    /// 바닥 생성 함수
    /// </summary>
    private void GenerateFloors()
    {
        Vector3 currentPos = Vector3.zero;  //첫 번째 바닥 (0,0,0)에 생성 
        _floorPositionsList.Add(currentPos);

        for (int i = 0; i < _floorCount; i++)
        {
            if (i == 0)
            {
                PlaceFloor(currentPos);
            }
            else
            {
                currentPos = GetRandomPosition();
                PlaceFloor(currentPos);

            }
        }
    }

    /// <summary>
    /// 바닥 배치 함수
    /// </summary>
    /// <param name="position"></param>
    private void PlaceFloor(Vector3 position)
    {
        if (_floorPrefabs.Count() == 0)
        {
            Debug.Log("There's no Prefabs");
            return;
        }
        GameObject randomFloor = _floorPrefabs[Random.Range(0, _floorPrefabs.Count())];

        Instantiate(randomFloor, position, Quaternion.identity);

        _floorPositionsList.Add(position);
    }

    /// <summary>
    /// 랜덤 위치 값을 반환해주는 함수
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomPosition()
    {
        Vector3 newPos;  //배치할 위치

        if (_floorPositionsList.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3[] directions =
        {
            new Vector3(_floorSize,0,0), //오른쪽
            new Vector3(-_floorSize,0,0), //왼쪽
            new Vector3(0,0,_floorSize), //위쪽
            new Vector3(0,0,-_floorSize)  //아래쪽
        };

        do
        {
            // 기존 바닥 중 하나를 랜덤으로 생성
            Vector3 basePos = _floorPositionsList[Random.Range(0, _floorPositionsList.Count)];

            // 상하좌우 중 랜덤 방향 선택
            Vector3 randomDir = directions[Random.Range(0, directions.Length)];

            //새로운 위치 계산
            newPos = basePos + randomDir;
        }
        while (_floorPositionsList.Contains(newPos));

        return newPos;

    }


}
