using System.Collections;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    #region Variables
    [Header("Target Prefab")]
    [SerializeField] private GameObject _targetPrefab;

    [Header("Spawn Area")]
    [SerializeField] private RectTransform _spawnArea;

    [Header("Spawn Settings")]
    [SerializeField] private float _spawnInterval = 1.2f;
    #endregion

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (ReactionGameManager.Instance != null && ReactionGameManager.Instance.GameEnded) yield break;

            SpawnTarget();

            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnTarget()
    {
        if (_targetPrefab == null || _spawnArea == null) return;

        GameObject target = Instantiate(_targetPrefab, _spawnArea);

        RectTransform targetRect = target.GetComponent<RectTransform>();

        Vector2 randomPosition = GetRandomPositionInsideSpawnArea();

        targetRect.anchoredPosition = randomPosition;
    }

    private Vector2 GetRandomPositionInsideSpawnArea()
    {
        Rect rect = _spawnArea.rect;

        float randomX = Random.Range(rect.xMin, rect.xMax);
        float randomY = Random.Range(rect.yMin, rect.yMax);

        return new Vector2(randomX, randomY);
    }
}
