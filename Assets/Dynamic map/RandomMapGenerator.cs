using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class RandomMapGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _floorPrefab; 
    private int _floorCount = 7;  //������ �ٴ��� ����
    private float _floorSize = 14f;  //�ٴ� ũ��
    private List<Vector3> _floorPositionsList = new List<Vector3>();  //�ٴ� ��ġ �����ϴ� ����Ʈ

    [SerializeField] private GameObject[] _fencePrefabs;  //��Ÿ�� ������ �迭
    private float[] _weights = { 0.5f, 0.4f, 0.1f}; //����ġ �迭 - ��Ÿ�� �������� ���� 
    [SerializeField] private GameObject[] _objectPrefabs; //������Ʈ ������ �迭
    void Start()
    {
        GenerateFloors();

        PlaceRandomObject();

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

        foreach (Vector3 floor in _floorPositionsList)
        {
            PlaceFence(floor);
        }
    }

    /// <summary>
    /// �ٴ� ��ġ �Լ�
    /// </summary>
    /// <param name="position"></param>
    private void PlaceFloor(Vector3 position)
    {
        if (!_floorPrefab)
        {
            Debug.Log("There's no Prefabs");
            return;
        }

        Instantiate(_floorPrefab, position, Quaternion.identity);

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


    /// <summary>
    /// ���� �ٴ��� �����ڸ����� Ȯ���ϰ�, ����Ʈ�� ��ȯ�ϴ� �Լ�
    /// </summary>
    /// <param name="Position"></param>
    /// <returns></returns>
    private void PlaceFence(Vector3 Position)
    {
        Vector3[] directions =
        {
            new Vector3(_floorSize,0,0),  //������
            new Vector3(-_floorSize,0,0), //����
            new Vector3(0,0,_floorSize),  //����
            new Vector3(0,0,-_floorSize)  //�Ʒ���
        };

        //�� ���⿡ �ٴ��� �ִ��� Ȯ��
        foreach (Vector3 direction in directions)
        {
            Vector3 neighborPos = Position + direction;
            GameObject FencePrefab = GetRandomFencePrefab();

            if (!_floorPositionsList.Contains(neighborPos))
            {
                Vector3 FencePos = Position + direction / 2;
                if (direction.x != 0)
                    Instantiate(FencePrefab, FencePos, Quaternion.Euler(0,90,0));
                else if (direction.z != 0)
                    Instantiate(FencePrefab, FencePos, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// ����ġ �迭 ũ��� ��Ÿ�� �������� ������ ������ Ȯ���ϴ� �Լ� 
    /// </summary>
    /// <returns></returns>
    private bool isValidWeights()
    {
        if(_fencePrefabs.Length == _weights.Length)
            return true;
        return false;
    }

    /// <summary>
    /// ��Ÿ�� �������� �������� �ϳ� �������� �Լ�
    /// </summary>
    /// <returns></returns>
    private GameObject GetRandomFencePrefab()
    {
        if (isValidWeights())
        {
            float totalWeight = _weights.Sum();  //����ġ �� ��� 

            float randomNum = Random.Range(0, totalWeight);  // 0~totalWeight ������ ���� �� ���� 

            float cumulativeWeight = 0f;
            for (int i = 0; i < _fencePrefabs.Length; i++)
            {
                cumulativeWeight += _weights[i];
                if (randomNum <= cumulativeWeight)
                    return _fencePrefabs[i];
            }
        }
        else
        {
            Debug.Log("����ġ ���� �������� ������ ���� �ʽ��ϴ�.");
        }

        return _fencePrefabs[0];
    }

    /// <summary>
    /// �������� ������Ʈ�� ��ġ�ϴ� �Լ�
    /// </summary>
    private void PlaceRandomObject()
    {
        if (_objectPrefabs.Length == 0)
        {
            Debug.Log("No Object prefabs assigned.");
            return;
        }

        HashSet<Vector3> usedPositions = new HashSet<Vector3>();
        //_floorPositionsList.OrderBy(x=>Random.value).ToList();
        foreach (Vector3 pos in _floorPositionsList)
        {
            Debug.Log(pos);
        }
        foreach (GameObject obj in _objectPrefabs)
        {
            float prefabRadius = GetPrefabRadius(obj);  //������ �ݰ� ���

            bool placed = false; //��ġ ���� Ȯ�� 

            foreach (Vector3 basePosition in _floorPositionsList )
            {
                // Ÿ�� ���� ���� ����
                Vector3 randomOffset = new Vector3(
                    Random.Range(-_floorSize / 3f, _floorSize / 3f),
                    0,
                    Random.Range(-_floorSize / 3f, _floorSize / 3f)
                );
                Vector3 finalPosition = basePosition + randomOffset;

                // �ּ� �Ÿ� ���� Ȯ��
                if (isPositionTooClose(finalPosition, usedPositions, prefabRadius))
                {
                    // ������Ʈ ��ġ
                    Instantiate(obj, finalPosition, Quaternion.identity);

                    // ������ ��ġ �߰�
                    AddPositionsToForbidden(finalPosition, prefabRadius, usedPositions);

                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"Could not place object {obj.name}. No valid positions available.");
            }
        }
    }

    /// <summary>
    /// �Ÿ��� �ʹ� ������� Ȯ���ϴ� �Լ� 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="usedPosition"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    private bool isPositionTooClose(Vector3 position, HashSet<Vector3> usedPosition, float radius)
    {
        foreach (Vector3 used in usedPosition)
        {
            if (Vector3.Distance(position, used) < radius)
            {
                return false;
            }
        }

        return true;   //���ϴ� ��ġ�� ���� ��ġ�鿡�� �ش� �ݰ濡 �ش��� �ȵ� �� false��ȯ 
    }

    /// <summary>
    /// �ֺ� ������ ��ġ ���� ��ġ�� ����Ʈ�� �߰��ϴ� �Լ�
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    /// <param name="forbiddenPositions"></param>
    private void AddPositionsToForbidden(Vector3 position, float radius, HashSet<Vector3> forbiddenPositions)
    {
        forbiddenPositions.Add(position);  //�߽� ��ġ�� ������ ��ġ�� �߰� 

        // �ݰ��� �������� �ֺ� ������ ���� ��ġ�� �߰�
        float step = radius / 2f; // ���� ���� ������ ����
        for (float x = -radius; x <= radius; x += step)
        {
            for (float z = -radius; z <= radius; z += step)
            {
                Vector3 offset = new Vector3(x, 0, z);
                if (offset.magnitude <= radius) // �� ������ ���� ����
                {
                    forbiddenPositions.Add(position + offset);
                }
            }
        }
    }

    /// <summary>
    /// �������� �ݰ��� ��ȯ�ϴ� �Լ�
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    private float GetPrefabRadius(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            float maxExtent = 0f;

            foreach (Renderer renderer in renderers)
            {
                maxExtent = Mathf.Max(maxExtent, renderer.bounds.extents.x, renderer.bounds.extents.z);
            }
            Debug.Log($"{prefab.name} maxExtent : {maxExtent}");
            return maxExtent;
        }
        else
        {
            Debug.LogWarning($"Prefab {prefab.name} does not have any Renderer components in its hierarchy.");
            return 10f; // �⺻��
        }
    }
}
