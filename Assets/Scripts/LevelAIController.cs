using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

public class LevelAIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GenerateMap mapGenerator;
    [SerializeField] private PlayerStatsRecorder statsRecorder;

    [Header("AI Settings")]
    [SerializeField] private string apiUrl = "http://localhost:5000/predict";
    [SerializeField] private float timeout = 5f;


    private void Awake()
    {
        PlayerStatsRecorder.OnRunCompleted += OnRunCompletedHandler;
    }

    private void Start()
    {
        if (mapGenerator != null)
        {
            mapGenerator.ApplyDefaultMultipliers();
            mapGenerator.GenerateFullMap();
        }
    }

    private void OnDestroy()
    {
        PlayerStatsRecorder.OnRunCompleted -= OnRunCompletedHandler;
    }

    private void OnRunCompletedHandler(PlayerStatsRecorder.RunRecord record)
    {
        StartCoroutine(StartNewLevelCoroutine());
    }

    private IEnumerator StartNewLevelCoroutine()
    {
        if (statsRecorder == null || mapGenerator == null)
        {
            Debug.LogError("Missing components");
            yield break;
        }

        float[] features = statsRecorder.GetFeatures();

        float trapMult = 1f, shelterMult = 1f, enemyMult = 1f, speedMult = 1f;

        yield return RequestAdjustments(features, (t, s, e, sp) =>
        {
            trapMult = t;
            shelterMult = s;
            enemyMult = e;
            speedMult = sp;
        });

        mapGenerator.ApplyAdjustments(trapMult, shelterMult, enemyMult, speedMult);
        statsRecorder.ResetForNewRun();
        mapGenerator.GenerateFullMap();

        Debug.Log($"New level generated with multipliers: trap={trapMult}, shelter={shelterMult}, enemy={enemyMult}, speed={speedMult}");
    }

    private IEnumerator RequestAdjustments(float[] features, System.Action<float, float, float, float> callback)
    {
        string json = JsonUtility.ToJson(new { features = features });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = (int)timeout;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                var response = JsonUtility.FromJson<AdjustmentResponse>(responseText);
                callback?.Invoke(
                    response.trap_density_mult,
                    response.shelter_density_mult,
                    response.enemy_density_mult,
                    response.location_speed_mult
                );
                Debug.Log($"AI response: trap={response.trap_density_mult}, shelter={response.shelter_density_mult}, enemy={response.enemy_density_mult}, speed={response.location_speed_mult}");
            }
            else
            {
                Debug.LogError($"AI request failed: {request.error}. Using default multipliers (1).");
                callback?.Invoke(1f, 1f, 1f, 1f);
            }
        }

    }

    private (float trap, float shelter, float enemy, float speed) CalculateAdjustments(float[] predProbs)
    {
        int predClass = 0;
        float maxProb = predProbs[0];
        for (int i = 1; i < predProbs.Length; i++)
        {
            if (predProbs[i] > maxProb)
            {
                maxProb = predProbs[i];
                predClass = i;
            }
        }

        float trap = 1f, shelter = 1f, enemy = 1f, speed = 1f;

        if (predClass == 1) // Спидранер
        {
            trap = 1.6f;
        }
        else if (predClass == 2) // Воин
        {
            enemy = 0.65f;
            speed = 1.35f;
        }
        else if (predClass == 0) // Стелсер
        {
            shelter = 0.65f;
            enemy = 1.45f;
        }

        return (trap, shelter, enemy, speed);
    }

    [System.Serializable]
    public class AdjustmentResponse
    {
        public float trap_density_mult;
        public float shelter_density_mult;
        public float enemy_density_mult;
        public float location_speed_mult;
        public string predicted_class;
        public float[] probabilities;
    }
}