using System;
using MapGeneration.Presentation;
using MapGeneration.Presentation.MapInfo;
using MapGeneration.Presentation.Subsidiary;
using MapGeneration.Settings;
using Unity.AI.Navigation;
using UnityEngine;

namespace MapGeneration.Generators
{
    public class GeneratorGO : MonoBehaviour
    {
        [SerializeField] private bool randomGenerate;
        [SerializeField] private MapGeneratorSettings mapGeneratorSettings;

        [SerializeField] private float initialCellSize = 1;
        [SerializeField] private float cellSizeMultiplayer = 1;

        [SerializeField] private NavMeshSurface navMesh;

        private IMapGenerator _mapGenerator;
        private IMapDrawer _mapDrawer;

        private GameObject _map;

        private void Start()
        {
            GenerateMap();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Regenerate();
            }
        }

        public void GenerateMap()
        {
            if (randomGenerate)
                mapGeneratorSettings.seed = (int)DateTimeOffset.Now.ToUnixTimeSeconds();

            _mapGenerator = new MapGenerator(mapGeneratorSettings);
            var mapData = _mapGenerator.Generate();

            _mapDrawer = new MapDrawer(initialCellSize);
            _map = _mapDrawer.Draw(mapData);
            navMesh.BuildNavMesh();
        }

        public void Regenerate()
        {
            DestroyImmediate(_map);
            GenerateMap();
        }

        private void OnDestroy()
        {
            navMesh.RemoveData();
            Debug.Log("navMesh");
        }
    }
}