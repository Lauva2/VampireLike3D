using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class UIRefs : MonoBehaviour
    {
        public GameObject menu, selection, hud, upgrade, skills, pause, map, progress, settings, results, victory, loading;
        public UIWidget cardTemplate, rowTemplate;
        public Transform characterList, biomeList, upgradeList, skillList, taskList, metaList;
        public Text title, subtitle;
        public Image healthFill, experienceFill, damageFlash;
        public Image dodgeFill, bossFill;
        public GameObject bossBar;
        public Slider musicSlider, soundSlider;
        public MapGraphic largeMap, miniMap;
        public Transform abilityList;
        public UIWidget abilityTemplate;
    }
}
