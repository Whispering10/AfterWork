using UnityEngine;
using Unity.InferenceEngine; 
using System;

public class AdaptiveLevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GenerateMap generateMap;
    [SerializeField] private PlayerStatsRecorder statsRecorder;

    [Header("Neural Network")]
    [SerializeField] private ModelAsset modelAsset;

    private float currentTrapMult = 1f;
    private float currentShelterMult = 1f;
    private float currentEnemyMult = 1f;
    private float currentSpeedMult = 1f;

    private Model runtimeModel;
    private Worker worker;

    private readonly string[] classNames = { "Стелсер", "Спидранер", "Воин" };

    private readonly float[,] styleMultipliers = new float[3, 4]
    {
        { 0.5f, 0.25f, 1.0f, 2.5f },   // Стелсер
        { 1.0f, 0.5f, 2.0f, 0.5f },   // Спидранер
        { 1.0f, 2.0f, 0.25f, 1.5f }   // Воин
    };

    private void Awake()
    {
        if (generateMap == null)
            generateMap = FindFirstObjectByType<GenerateMap>();
        if (statsRecorder == null)
            statsRecorder = FindFirstObjectByType<PlayerStatsRecorder>();

        if (generateMap == null)
            Debug.LogError("GenerateMap не найден!");
        if (statsRecorder == null)
            Debug.LogError("PlayerStatsRecorder не найден!");

        if (modelAsset != null)
        {
            try
            {
                runtimeModel = ModelLoader.Load(modelAsset);
                worker = new Worker(runtimeModel, BackendType.GPUCompute);
                Debug.Log("Нейросеть загружена успешно.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка загрузки нейросети: {e.Message}");
                worker = null;
            }
        }
        else
        {
            Debug.Log("ModelAsset не назначен, используется эвристика.");
        }

        PlayerStatsRecorder.OnRunCompleted += OnRunCompleted;
    }

    private void OnDestroy()
    {
        PlayerStatsRecorder.OnRunCompleted -= OnRunCompleted;
        worker?.Dispose();
    }

    private void OnRunCompleted(PlayerStatsRecorder.RunRecord runRecord)
    {
        if (statsRecorder == null)
        {
            Debug.LogWarning("StatsRecorder отсутствует, адаптация пропущена.");
            return;
        }

        float[] rawFeatures = statsRecorder.GetFeatures();
        Debug.Log($"Сырые признаки: {string.Join(", ", rawFeatures)}");

        float[] normalized = NormalizeFeatures(rawFeatures);
        Debug.Log($"Нормализованные признаки: {string.Join(", ", normalized)}");

        float[] multipliers = ComputeMultipliers(normalized);

        currentTrapMult = multipliers[0];
        currentShelterMult = multipliers[1];
        currentEnemyMult = multipliers[2];
        currentSpeedMult = multipliers[3];

        Debug.Log($"Итоговые множители: traps={currentTrapMult:F2}, shelters={currentShelterMult:F2}, enemies={currentEnemyMult:F2}, speed={currentSpeedMult:F2}");

        // Применяем множители к генератору уровня
        if (generateMap != null)
        {
            generateMap.ApplyAdjustments(currentTrapMult, currentShelterMult, currentEnemyMult, currentSpeedMult);
            Debug.Log("Множители применены к GenerateMap.");
        }
        else
        {
            Debug.LogError("generateMap == null, не удалось применить множители!");
        }
    }

    public void ApplyAdaptation()
    {
        if (generateMap == null) return;
        generateMap.ApplyAdjustments(currentTrapMult, currentShelterMult, currentEnemyMult, currentSpeedMult);
        Debug.Log("Принудительное применение множителей к GenerateMap.");
    }

    private float[] ComputeMultipliers(float[] features)
    {
        if (worker != null && runtimeModel != null)
            return RunNeuralNetwork(features);
        else
            return HeuristicMultipliers(features);
    }

    private float[] RunNeuralNetwork(float[] features)
    {
        try
        {
            if (features.Length != 9)
            {
                Debug.LogError($"Ожидается 9 признаков, получено {features.Length}");
                return DefaultMultipliers();
            }

            TensorShape shape = new TensorShape(1, features.Length);
            Tensor<float> inputTensor = new Tensor<float>(shape, features);

            worker.Schedule(inputTensor);
            Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

            if (outputTensor == null)
            {
                Debug.LogError("Выходной тензор null");
                inputTensor.Dispose();
                return DefaultMultipliers();
            }

            float[] probs = outputTensor.DownloadToArray();
            inputTensor.Dispose();
            outputTensor.Dispose();

            if (probs.Length < 3)
            {
                Debug.LogError($"Выход модели содержит {probs.Length} значений, ожидается 3.");
                return DefaultMultipliers();
            }

            float p0 = probs[0], p1 = probs[1], p2 = probs[2];
            Debug.Log($"Вероятности: Стелсер={p0:F4}, Спидранер={p1:F4}, Воин={p2:F4}");

            int predClass = 0;
            float maxProb = p0;
            if (p1 > maxProb) { maxProb = p1; predClass = 1; }
            if (p2 > maxProb) { maxProb = p2; predClass = 2; }

            Debug.Log($"Предсказанный класс: {classNames[predClass]} (вероятность {maxProb:F4})");

            return ApplyClassMultipliers(predClass);
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка инференса: {e.Message}");
            return DefaultMultipliers();
        }
    }

    private float[] ApplyClassMultipliers(int classIndex)
    {
        float trapMult = styleMultipliers[classIndex, 0];
        float shelterMult = styleMultipliers[classIndex, 1];
        float enemyMult = styleMultipliers[classIndex, 2];
        float speedMult = styleMultipliers[classIndex, 3];

        Debug.Log($"Применены множители для класса {classNames[classIndex]}: trap={trapMult:F2}, shelter={shelterMult:F2}, enemy={enemyMult:F2}, speed={speedMult:F2}");

        return new float[] { trapMult, shelterMult, enemyMult, speedMult };
    }

    private float[] ApplyWeightedMultipliers(float[] probs)
    {
        float trap = 0f, shelter = 0f, enemy = 0f, speed = 0f;
        for (int i = 0; i < 3; i++)
        {
            trap += probs[i] * styleMultipliers[i, 0];
            shelter += probs[i] * styleMultipliers[i, 1];
            enemy += probs[i] * styleMultipliers[i, 2];
            speed += probs[i] * styleMultipliers[i, 3];
        }
        Debug.Log($"Взвешенные множители: trap={trap:F2}, shelter={shelter:F2}, enemy={enemy:F2}, speed={speed:F2}");
        return new float[] { trap, shelter, enemy, speed };
    }

    private float[] DefaultMultipliers() => new float[] { 1f, 1f, 1f, 1f };

    private float[] NormalizeFeatures(float[] raw)
    {
        float[] maxValues = { 20f, 5f, 60f, 60f, 5f, 5f, 20f, 20f, 120f };
        float[] norm = new float[raw.Length];
        for (int i = 0; i < raw.Length; i++)
            norm[i] = maxValues[i] > 0 ? raw[i] / maxValues[i] : raw[i];
        return norm;
    }

    private float[] HeuristicMultipliers(float[] features)
    {
        float speedMult = Mathf.Clamp(0.8f + features[0] * 0.2f, 0.5f, 2f);
        float attackFreq = features[4];
        float enemyMult = Mathf.Clamp(0.8f + attackFreq * 0.5f, 0.5f, 2f);
        float trapMult = Mathf.Clamp(0.8f + attackFreq * 0.3f, 0.5f, 2f);
        float stealth = features[2];
        float shelterMult = Mathf.Clamp(0.8f + stealth * 0.1f, 0.5f, 2f);
        return new float[] { trapMult, shelterMult, enemyMult, speedMult };
    }
}