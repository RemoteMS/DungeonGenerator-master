using System;
using MapGeneration.Presentation;
using MapGeneration.Presentation.MapInfo;
using MapGeneration.Presentation.Subsidiary;
using MapGeneration.Settings;
using UnityEngine;

namespace MapGeneration.Generators
{
    public class GeneratorGO : MonoBehaviour
    {
        [SerializeField] private bool randomGenerate;
        [SerializeField] private MapGeneratorSettings mapGeneratorSettings;

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

            _mapDrawer = new MapDrawer();
            _map = _mapDrawer.Draw(mapData);
        }

        public void Regenerate()
        {
            DestroyImmediate(_map);
            GenerateMap();
        }
    }
}