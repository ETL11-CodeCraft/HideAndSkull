using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomFloorGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] _floorPrefabs; //4���� �ٴ� ������ �迭 
    private int _floorCount = 6;  //������ �ٴ��� ����
    private float _floorSize = 14f;  //�ٴ� ũ�� (����, ���ΰ� �����ϴٰ� ����)

    private List<Vector3> _floorPositionsList = new List<Vector3>();  //�ٴ� ��ġ ����

    void Start()
    {
        if (_floorPrefabs.Length > 0)
        {
            //_floorSize = _floorPrefabs[0].GetComponent<Renderer>().bounds.size.x;
            Debug.Log(_floorSize);
        }
        else
        {
            Debug.Log("�������� �������� �ʽ��ϴ�.");
        }
        GenerateFloors();
    }

    /// <summary>
    /// �ٴ� ���� �Լ�
    /// </summary>
    private void GenerateFloors()
    {
        Vector3 currentPos = Vector3.zero;  //ù ��° �ٴ� (0,0,0)�� ���� 
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
    /// �ٴ� ��ġ �Լ�
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
    /// ���� ��ġ ���� ��ȯ���ִ� �Լ�
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomPosition()
    {
        Vector3 newPos;  //��ġ�� ��ġ

        if (_floorPositionsList.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3[] directions =
        {
            new Vector3(_floorSize,0,0), //������
            new Vector3(-_floorSize,0,0), //����
            new Vector3(0,0,_floorSize), //����
            new Vector3(0,0,-_floorSize)  //�Ʒ���
        };

        do
        {
            // ���� �ٴ� �� �ϳ��� �������� ����
            Vector3 basePos = _floorPositionsList[Random.Range(0, _floorPositionsList.Count)];

            // �����¿� �� ���� ���� ����
            Vector3 randomDir = directions[Random.Range(0, directions.Length)];

            //���ο� ��ġ ���
            newPos = basePos + randomDir;
        }
        while (_floorPositionsList.Contains(newPos));

        return newPos;

    }


}
