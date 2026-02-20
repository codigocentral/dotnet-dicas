// Funções JavaScript para métricas de performance

window.getMemoryInfo = () => {
    if (performance.memory) {
        return {
            usedJSHeapSize: performance.memory.usedJSHeapSize,
            totalJSHeapSize: performance.memory.totalJSHeapSize,
            jsHeapSizeLimit: performance.memory.jsHeapSizeLimit
        };
    }
    
    // Fallback para navegadores que não suportam performance.memory
    return {
        usedJSHeapSize: 0,
        totalJSHeapSize: 0,
        jsHeapSizeLimit: 0
    };
};

window.measureRenderTime = (callback) => {
    const start = performance.now();
    requestAnimationFrame(() => {
        const end = performance.now();
        callback(end - start);
    });
};

window.observeElementResize = (element, dotnetHelper) => {
    if (window.ResizeObserver) {
        const observer = new ResizeObserver(entries => {
            for (let entry of entries) {
                dotnetHelper.invokeMethodAsync('OnElementResized', 
                    entry.contentRect.width, 
                    entry.contentRect.height);
            }
        });
        observer.observe(element);
        return observer;
    }
    return null;
};
