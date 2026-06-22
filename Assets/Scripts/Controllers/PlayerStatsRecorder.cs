using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text;
using UnityEngine;

public class PlayerStatsRecorder : MonoBehaviour
{
    [Header("Interval Settings")]
    [SerializeField] private float intervalDuration = 5f;

    [Header("Idle Threshold")]
    [SerializeField] private float idleSpeedThreshold = 0.1f;

    private Rigidbody2D rb;
    private StealthCoverHandler stealthHandler;
    private bool runFinished = false;

    private float runStartTime;

    private float totalDistance;
    private int dashCount;
    private float stealthTime;
    private float idleTime;
    private int attackCount;
    private int parryCount;
    private int enemiesKilled;
    private int objectsDestroyed;

    private static List<RunRecord> allRuns = new List<RunRecord>();
    private List<IntervalRecord> currentRunIntervals = new List<IntervalRecord>();

    private static RunRecord lastFinishedRun = null;

    public static event Action<IntervalRecord> OnIntervalRecorded;
    public static event Action<RunRecord> OnRunCompleted;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stealthHandler = GetComponent<StealthCoverHandler>();
        ResetCurrentRun();
        runStartTime = Time.time;
    }

    private void Start()
    {
        StartCoroutine(RecordIntervalCoroutine());
    }

    private void Update()
    {
        if (runFinished) return;

        if (rb != null)
        {
            totalDistance += rb.linearVelocity.magnitude * Time.deltaTime;
        }

        if (stealthHandler != null && stealthHandler.IsHidden)
            stealthTime += Time.deltaTime;

        bool isMoving = rb != null && rb.linearVelocity.magnitude > idleSpeedThreshold;
        if (!isMoving)
            idleTime += Time.deltaTime;
    }

    private IEnumerator RecordIntervalCoroutine()
    {
        while (!runFinished)
        {
            yield return new WaitForSeconds(intervalDuration);

            float intervalSpeed = totalDistance / intervalDuration;
            float dashFrequency = dashCount / intervalDuration;
            float attacksFrequency = attackCount / intervalDuration;
            float parryFrequency = parryCount / intervalDuration;

            IntervalRecord record = new IntervalRecord
            {
                timestamp = Time.time,
                speed = intervalSpeed,
                dashFrequency = dashFrequency,
                stealthTime = stealthTime,
                idleTime = idleTime,
                attackFrequency = attacksFrequency,
                parryFrequency = parryFrequency,
                enemiesKilled = enemiesKilled,
                objectsDestroyed = objectsDestroyed
            };

            currentRunIntervals.Add(record);
            OnIntervalRecorded?.Invoke(record);

            totalDistance = 0;
            dashCount = 0;
            stealthTime = 0;
            idleTime = 0;
            attackCount = 0;
            parryCount = 0;
            enemiesKilled = 0;
            objectsDestroyed = 0;
        }
    }

    public void OnDash() => dashCount++;
    public void OnAttack() => attackCount++;
    public void OnParry() => parryCount++;
    public void OnEnemyKilled() => enemiesKilled++;
    public void OnObjectDestroyed() => objectsDestroyed++;

    public void FinishRun()
    {
        if (runFinished) return;
        runFinished = true;

        if (currentRunIntervals.Count == 0)
        {
            currentRunIntervals.Add(new IntervalRecord
            {
                timestamp = Time.time,
                speed = 0,
                dashFrequency = 0,
                stealthTime = stealthTime,
                idleTime = idleTime,
                attackFrequency = 0,
                parryFrequency = 0,
                enemiesKilled = enemiesKilled,
                objectsDestroyed = objectsDestroyed
            });
        }

        RunRecord runRecord = CalculateRunRecord();
        runRecord.totalRunTime = Time.time - runStartTime;
        allRuns.Add(runRecord);

        lastFinishedRun = runRecord;

        OnRunCompleted?.Invoke(runRecord);

        SaveRunToAllRunsCSV(runRecord);

        ResetCurrentRun();
        StopAllCoroutines();
        enabled = false;
    }

    private RunRecord CalculateRunRecord()
    {
        float totalSpeed = 0, totalDashFreq = 0, totalStealthTime = 0, totalIdleTime = 0;
        float totalAttackFreq = 0, totalParryFreq = 0;
        int totalEnemies = 0, totalObjects = 0;

        foreach (var rec in currentRunIntervals)
        {
            totalSpeed += rec.speed;
            totalDashFreq += rec.dashFrequency;
            totalStealthTime += rec.stealthTime;
            totalIdleTime += rec.idleTime;
            totalAttackFreq += rec.attackFrequency;
            totalParryFreq += rec.parryFrequency;
            totalEnemies += rec.enemiesKilled;
            totalObjects += rec.objectsDestroyed;
        }

        int count = currentRunIntervals.Count;
        return new RunRecord
        {
            averageSpeed = totalSpeed / count,
            averageDashFrequency = totalDashFreq / count,
            totalStealthTime = totalStealthTime,
            totalIdleTime = totalIdleTime,
            averageAttackFrequency = totalAttackFreq / count,
            averageParryFrequency = totalParryFreq / count,
            totalEnemiesKilled = totalEnemies,
            totalObjectsDestroyed = totalObjects
        };
    }

    private void ResetCurrentRun()
    {
        currentRunIntervals.Clear();
        totalDistance = 0;
        dashCount = 0;
        stealthTime = 0;
        idleTime = 0;
        attackCount = 0;
        parryCount = 0;
        enemiesKilled = 0;
        objectsDestroyed = 0;
        runStartTime = Time.time;
    }

    public void ResetForNewRun()
    {
        StopAllCoroutines();
        runFinished = false;
        ResetCurrentRun();
        enabled = true;
        StartCoroutine(RecordIntervalCoroutine());
        Debug.Log("PlayerStatsRecorder перезапущен для нового забега.");
    }

    public float[] GetFeatures()
    {
        if (lastFinishedRun == null)
            return new float[9] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };

        RunRecord r = lastFinishedRun;
        return new float[]
        {
            r.averageSpeed,
            r.averageDashFrequency,
            r.totalStealthTime,
            r.totalIdleTime,
            r.averageAttackFrequency,
            r.averageParryFrequency,
            r.totalEnemiesKilled,
            r.totalObjectsDestroyed,
            r.totalRunTime
        };
    }

    private void SaveRunToAllRunsCSV(RunRecord run)
    {
        try
        {
            string folderPath = Path.Combine(Application.dataPath, "Stats");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, "AllRuns.csv");
            bool fileExists = File.Exists(filePath);

            string separator = ";";
            CultureInfo invariant = CultureInfo.InvariantCulture;
            Encoding encoding = new UTF8Encoding(true);

            using (StreamWriter writer = new StreamWriter(filePath, append: true, encoding: encoding))
            {
                if (!fileExists)
                {
                    writer.WriteLine(
                        $"TotalRunTime{separator}AverageSpeed{separator}AverageDashFrequency{separator}" +
                        $"TotalStealthTime{separator}TotalIdleTime{separator}AverageAttackFrequency{separator}" +
                        $"AverageParryFrequency{separator}TotalEnemiesKilled{separator}TotalObjectsDestroyed"
                    );
                }

                writer.WriteLine(
                    $"{run.totalRunTime.ToString(invariant)}{separator}" +
                    $"{run.averageSpeed.ToString(invariant)}{separator}" +
                    $"{run.averageDashFrequency.ToString(invariant)}{separator}" +
                    $"{run.totalStealthTime.ToString(invariant)}{separator}" +
                    $"{run.totalIdleTime.ToString(invariant)}{separator}" +
                    $"{run.averageAttackFrequency.ToString(invariant)}{separator}" +
                    $"{run.averageParryFrequency.ToString(invariant)}{separator}" +
                    $"{run.totalEnemiesKilled}{separator}" +
                    $"{run.totalObjectsDestroyed}"
                );
            }

            Debug.Log($"Run record appended to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save run record: {e.Message}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Portal") && !runFinished)
        {
            FinishRun();
        }
    }

    public static List<RunRecord> GetAllRuns() => allRuns;
    public static void ClearHistory() => allRuns.Clear();

    [System.Serializable]
    public class IntervalRecord
    {
        public float timestamp;
        public float speed;
        public float dashFrequency;
        public float stealthTime;
        public float idleTime;
        public float attackFrequency;
        public float parryFrequency;
        public int enemiesKilled;
        public int objectsDestroyed;
    }

    [System.Serializable]
    public class RunRecord
    {
        public float totalRunTime;
        public float averageSpeed;
        public float averageDashFrequency;
        public float totalStealthTime;
        public float totalIdleTime;
        public float averageAttackFrequency;
        public float averageParryFrequency;
        public int totalEnemiesKilled;
        public int totalObjectsDestroyed;
    }
}