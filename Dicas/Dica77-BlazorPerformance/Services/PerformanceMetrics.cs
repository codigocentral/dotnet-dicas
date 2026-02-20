using System.Diagnostics;
using Microsoft.JSInterop;

namespace Dica77.BlazorPerformance;

public class PerformanceMetrics
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Stopwatch _renderStopwatch = new();
    private int _componentCount = 0;
    private int _rerenderCount = 0;
    private int _jsInteropCallCount = 0;
    private double _memoryUsageMB = 0;

    public PerformanceMetrics(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public event Action? OnMetricsUpdated;

    public double LastRenderTime { get; private set; }
    public int ComponentCount => _componentCount;
    public int RerenderCount => _rerenderCount;
    public int JSInteropCallCount => _jsInteropCallCount;
    public double MemoryUsageMB => _memoryUsageMB;

    public void StartRenderMeasurement()
    {
        _renderStopwatch.Restart();
    }

    public void EndRenderMeasurement()
    {
        _renderStopwatch.Stop();
        LastRenderTime = _renderStopwatch.Elapsed.TotalMilliseconds;
        _rerenderCount++;
        OnMetricsUpdated?.Invoke();
    }

    public void IncrementComponentCount()
    {
        Interlocked.Increment(ref _componentCount);
    }

    public void DecrementComponentCount()
    {
        Interlocked.Decrement(ref _componentCount);
    }

    public void IncrementJSInteropCalls()
    {
        Interlocked.Increment(ref _jsInteropCallCount);
    }

    public async Task UpdateMetricsAsync()
    {
        try
        {
            IncrementJSInteropCalls();
            
            // Obtém uso de memória via JavaScript
            var memoryInfo = await _jsRuntime.InvokeAsync<MemoryInfo>("getMemoryInfo");
            _memoryUsageMB = memoryInfo.UsedJSHeapSize / (1024.0 * 1024.0);
            
            OnMetricsUpdated?.Invoke();
        }
        catch (Exception)
        {
            // Fallback se JS Interop falhar
            _memoryUsageMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
        }
    }

    public void Reset()
    {
        _componentCount = 0;
        _rerenderCount = 0;
        _jsInteropCallCount = 0;
        LastRenderTime = 0;
        _memoryUsageMB = 0;
        OnMetricsUpdated?.Invoke();
    }

    public class MemoryInfo
    {
        public long UsedJSHeapSize { get; set; }
        public long TotalJSHeapSize { get; set; }
        public long JSHeapSizeLimit { get; set; }
    }
}
