using UnityEngine;
using System.Collections;

public class FallingObjectSpawner : MonoBehaviour
{
    #region Variables
    [Header("Object Prefabs")]
    [SerializeField] private GameObject[] _goodObjectPrefabs;
    [SerializeField] private GameObject[] _badObjectPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float _spawnInterval = 1f;
    [SerializeField] private float _minX = -2.5f;
    [SerializeField] private float _maxX = 2.5f;
    [SerializeField] private float _spawnY = 6f;

    [SerializeField, Range(0f, 1f)]
    private float _badObjectChance = 0.25f;

    #endregion

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (CatchGameManager.Instance != null && CatchGameManager.Instance.GameEnded)yield break;

            SpawnObject();

            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnObject()
    {
        bool spawnBadObject = Random.value < _badObjectChance; // Generate random value to spawn a good or bad object based on threshold

        GameObject[] selectedArray = spawnBadObject ? _badObjectPrefabs : _goodObjectPrefabs; // Select array of objects to spawn

        if (selectedArray.Length == 0) return;

        GameObject selectedPrefab = selectedArray[Random.Range(0, selectedArray.Length)]; // Chose a random prefab to spawn

        Vector3 spawnPosition = new Vector3(Random.Range(_minX, _maxX), _spawnY, 0f); // Generate random position for the object to spawn

        Instantiate(selectedPrefab, spawnPosition, Quaternion.identity); // spawn object
    }
}