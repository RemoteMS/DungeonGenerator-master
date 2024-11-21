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

        private void Awake()
        {
            if (randomGenerate)
                mapGeneratorSettings.seed = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        private void Start()
        {
            _mapGenerator = new MapGenerator(mapGeneratorSettings);
            var mapData = _mapGenerator.Generate();

            _mapDrawer = new MapDrawer();
            _mapDrawer.Draw(mapData);
        }
    }
}