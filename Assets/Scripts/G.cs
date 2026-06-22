using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class G : MonoBehaviour
{
    private GameObject playerPfb;
    private GameObject enemyPfb;

    private CMSEntity playerModel;
    private CMSEntity enemyModel;

    private SliderOfController healthPlayerSlider;
    private SliderOfController staminaPlayerSlider;

    private void Awake()
    {
        StartMainScene();
    }

    public void StartMainScene()
    {
        CMS.Init();

        playerPfb = Resources.Load<GameObject>("CMS/Prefabs/GameObjects/Player");
        playerModel = CMS.Get<CMSEntity>("CMS/Prefabs/Models/Entities/PlayerModel");
        enemyPfb = Resources.Load<GameObject>("CMS/Prefabs/GameObjects/Enemy");
        enemyModel = CMS.Get<CMSEntity>("CMS/Prefabs/Models/Entities/EnemyModel");

        GameObject player = Factory.Create(playerPfb, playerModel);
        player.transform.position = new Vector3(0, 0, 0);

        staminaPlayerSlider = GameObject.FindGameObjectWithTag("StaminaBar").GetComponent<SliderOfController>();
        staminaPlayerSlider.Init(player.GetComponent<StaminaController>());
        healthPlayerSlider = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<SliderOfController>();
        healthPlayerSlider.Init(player.GetComponent<HealthController>());

        GenerateMap map = GameObject.FindGameObjectWithTag("Map").GetComponent<GenerateMap>();
        map.Init(player);

        // Передаём данные о врагах (если не назначены в инспекторе)
        map.SetEnemyData(enemyPfb, enemyModel, 40);

        map.GenerateFullMap(); // теперь внутри вызывается AddEnemy

        CameraMove camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMove>();
        camera.Init(player.transform);
    }

    public void CreateEnemy()
    {
        GameObject enemy = Factory.Create(
            enemyPfb,
            enemyModel);
        enemy.transform.position = new Vector3(-5, 2, 0);
    }
}