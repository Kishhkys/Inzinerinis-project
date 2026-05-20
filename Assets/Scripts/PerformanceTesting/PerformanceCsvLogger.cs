using System.IO;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceCsvLogger : MonoBehaviour
{
    [SerializeField] private PerformanceEnemySpawner enemySpawner;
    [SerializeField] private string fileName = "performance-stress-test.csv";
    [SerializeField] private bool saveToProjectReportsFolder = true;
    [SerializeField] private string projectRelativeFolder = "Reports";
    [SerializeField] private float sampleInterval = 0.5f;

    private StreamWriter writer;
    private float nextSampleTime;
    private float startTime;
    private ProfilerRecorder mainThreadRecorder;
    private ProfilerRecorder gcAllocatedRecorder;
    private ProfilerRecorder gcReservedRecorder;

    private void OnEnable()
    {
        startTime = Time.realtimeSinceStartup;
        nextSampleTime = 0f;

        mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");
        gcAllocatedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        gcReservedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");

        string path = GetLogFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        writer = new StreamWriter(path, false);
        writer.WriteLine("elapsed_seconds,frame,enemy_count,frame_time_ms,fps,main_thread_ms,gc_allocated_in_frame_bytes,gc_reserved_memory_bytes,total_allocated_memory_bytes");

        Debug.Log("Performance CSV logging started: " + path, this);
    }

    private void Update()
    {
        if (Time.realtimeSinceStartup < nextSampleTime)
        {
            return;
        }

        nextSampleTime = Time.realtimeSinceStartup + sampleInterval;

        float frameTimeMs = Time.unscaledDeltaTime * 1000f;
        float fps = Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f;
        float elapsedSeconds = Time.realtimeSinceStartup - startTime;
        int enemyCount = enemySpawner != null ? enemySpawner.SpawnedCount : 0;
        double mainThreadMs = mainThreadRecorder.Valid ? mainThreadRecorder.LastValue / 1_000_000.0 : 0.0;
        long gcAllocatedInFrame = gcAllocatedRecorder.Valid ? gcAllocatedRecorder.LastValue : 0L;
        long gcReservedMemory = gcReservedRecorder.Valid ? gcReservedRecorder.LastValue : 0L;
        long totalAllocatedMemory = Profiler.GetTotalAllocatedMemoryLong();

        writer.WriteLine(
            $"{elapsedSeconds:F3}," +
            $"{Time.frameCount}," +
            $"{enemyCount}," +
            $"{frameTimeMs:F3}," +
            $"{fps:F2}," +
            $"{mainThreadMs:F3}," +
            $"{gcAllocatedInFrame}," +
            $"{gcReservedMemory}," +
            $"{totalAllocatedMemory}"
        );
    }

    private void OnDisable()
    {
        mainThreadRecorder.Dispose();
        gcAllocatedRecorder.Dispose();
        gcReservedRecorder.Dispose();

        if (writer != null)
        {
            writer.Flush();
            writer.Dispose();
            writer = null;
        }
    }

    private void OnValidate()
    {
        sampleInterval = Mathf.Max(0.05f, sampleInterval);
    }

    private string GetLogFilePath()
    {
        if (!saveToProjectReportsFolder)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        return Path.Combine(projectRoot, projectRelativeFolder, fileName);
    }
}
