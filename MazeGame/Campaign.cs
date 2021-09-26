using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGame
{
    class Campaign
    {
        public string Name { get; private set; }
        public Level[] Levels { get; private set; }
        public string[][] LevelBriefings { get; private set; }
        public string[][] LevelOutros { get; private set; }

        public Campaign(string name, Level[] levels, string[][] levelBriefings, string[][] levelOutros)
        {
            Name = name;
            Levels = levels;
            LevelBriefings = levelBriefings;
            LevelOutros = levelOutros;
        }
    }

    class CampaignConfig
    {
        public string Name { get; set; }
        public string[] LevelFiles { get; set; }
        public string[][] LevelBriefings { get; set; }
        public string[][] LevelOutros { get; set; }

        public CampaignConfig(string name, string[] levels, string[][] levelBriefings, string[][] levelOutros)
        {
            Name = name;
            LevelFiles = levels;
            LevelBriefings = levelBriefings;
            LevelOutros = levelOutros;
        }

        public CampaignConfig()
        {
            //The deserialization requires a default, parameterless constructor
        }
    }
}
