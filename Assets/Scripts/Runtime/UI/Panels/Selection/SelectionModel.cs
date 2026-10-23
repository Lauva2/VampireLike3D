using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class SelectionModel : UIModel
    {
        public string selectedCharacter = "adventurer", description;
        public BiomeId selectedBiome;
        public int crystals;
        public override void Refresh(GameManager game)
        {
            crystals = game.Save.Profile.crystals;
            var selected = game.Catalog.Character(selectedCharacter); description = selected.name + " · " + selected.description;
        }
    }
}
