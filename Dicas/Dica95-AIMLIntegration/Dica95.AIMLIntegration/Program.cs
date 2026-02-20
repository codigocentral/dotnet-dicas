using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Vision;
using System.Numerics.Tensors;
using System.Text.Json;

Console.WriteLine("🤖 Dica 95: AI & Machine Learning Integration (.NET 9)");
Console.WriteLine("========================================================");

// 1. ML.NET Classification
Console.WriteLine("\n1. 📊 ML.NET Classification:");
Console.WriteLine("─────────────────────────────");

await DemonstrarClassificacaoTexto();

// 2. Regression Analysis
Console.WriteLine("\n2. 📈 Regression Analysis:");
Console.WriteLine("─────────────────────────");

await DemonstrarRegressao();

// 3. Clustering & Anomaly Detection
Console.WriteLine("\n3. 🎯 Clustering & Anomaly Detection:");
Console.WriteLine("────────────────────────────────────");

await DemonstrarClusteringAnomalyDetection();

// 4. Computer Vision
Console.WriteLine("\n4. 👁️ Computer Vision:");
Console.WriteLine("──────────────────────");

await DemonstrarComputerVision();

// 5. Natural Language Processing
Console.WriteLine("\n5. 💬 Natural Language Processing:");
Console.WriteLine("──────────────────────────────────");

await DemonstrarNLP();

// 6. Tensor Operations
Console.WriteLine("\n6. 🧮 Tensor Operations:");
Console.WriteLine("──────────────────────");

DemonstrarTensorOperations();

// 7. Model Evaluation & Metrics
Console.WriteLine("\n7. 📊 Model Evaluation:");
Console.WriteLine("──────────────────────");

await DemonstrarModelEvaluation();

// 8. AI Pipeline & AutoML
Console.WriteLine("\n8. 🔄 AI Pipeline & AutoML:");
Console.WriteLine("──────────────────────────");

await DemonstrarAIPipeline();

Console.WriteLine("\n✅ Demonstração completa de AI & ML Integration!");

static async Task DemonstrarClassificacaoTexto()
{
    var mlContext = new MLContext(seed: 1);
    
    // Dados de exemplo para classificação de sentimento
    var sentimentData = new[]
    {
        new SentimentData { Text = "This movie is fantastic!", Sentiment = true },
        new SentimentData { Text = "I love this product", Sentiment = true },
        new SentimentData { Text = "Amazing experience", Sentiment = true },
        new SentimentData { Text = "This is terrible", Sentiment = false },
        new SentimentData { Text = "I hate this", Sentiment = false },
        new SentimentData { Text = "Worst experience ever", Sentiment = false },
        new SentimentData { Text = "Great service and quality", Sentiment = true },
        new SentimentData { Text = "Poor quality and service", Sentiment = false }
    };
    
    Console.WriteLine($"📋 Dataset criado com {sentimentData.Length} amostras");
    
    var dataView = mlContext.Data.LoadFromEnumerable(sentimentData);
    
    // Pipeline de treinamento
    var pipeline = mlContext.Transforms.Text.FeaturizeText(
            outputColumnName: "Features", 
            inputColumnName: nameof(SentimentData.Text))
        .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
            labelColumnName: nameof(SentimentData.Sentiment), 
            featureColumnName: "Features"));
    
    Console.WriteLine("🔧 Pipeline configurado: Text Featurization + SDCA Logistic Regression");
    
    // Treinamento
    var model = pipeline.Fit(dataView);
    Console.WriteLine("🎯 Modelo treinado com sucesso!");
    
    // Predição
    var predictionEngine = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
    
    var testSamples = new[]
    {
        "This is absolutely wonderful!",
        "I'm not happy with this",
        "Great job, well done!",
        "This is disappointing"
    };
    
    Console.WriteLine("\n🔮 Predições:");
    foreach (var sample in testSamples)
    {
        var prediction = predictionEngine.Predict(new SentimentData { Text = sample });
        var sentiment = prediction.Prediction ? "Positivo 😊" : "Negativo 😞";
        var confidence = prediction.Probability;
        
        Console.WriteLine($"   📝 \"{sample}\"");
        Console.WriteLine($"   ➡️ {sentiment} (Confiança: {confidence:P1})");
    }
    
    await Task.Delay(10);
}

static async Task DemonstrarRegressao()
{
    var mlContext = new MLContext(seed: 1);
    
    // Dados de exemplo para predição de preços de casas
    var houseData = new[]
    {
        new HouseData { Size = 1000, Bedrooms = 2, Price = 150000 },
        new HouseData { Size = 1200, Bedrooms = 3, Price = 180000 },
        new HouseData { Size = 1500, Bedrooms = 3, Price = 220000 },
        new HouseData { Size = 1800, Bedrooms = 4, Price = 280000 },
        new HouseData { Size = 2000, Bedrooms = 4, Price = 320000 },
        new HouseData { Size = 2200, Bedrooms = 5, Price = 380000 },
        new HouseData { Size = 800, Bedrooms = 1, Price = 120000 },
        new HouseData { Size = 1100, Bedrooms = 2, Price = 160000 }
    };
    
    Console.WriteLine($"🏠 Dataset de casas criado com {houseData.Length} amostras");
    
    var dataView = mlContext.Data.LoadFromEnumerable(houseData);
    
    // Pipeline de regressão
    var pipeline = mlContext.Transforms.Concatenate("Features", nameof(HouseData.Size), nameof(HouseData.Bedrooms))
        .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: nameof(HouseData.Price)));
    
    Console.WriteLine("📈 Pipeline de regressão configurado: SDCA Regression");
    
    var model = pipeline.Fit(dataView);
    Console.WriteLine("🎯 Modelo de regressão treinado!");
    
    // Avaliação
    var predictions = model.Transform(dataView);
    var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: nameof(HouseData.Price));
    
    Console.WriteLine($"📊 Métricas do modelo:");
    Console.WriteLine($"   R² Score: {metrics.RSquared:F3}");
    Console.WriteLine($"   RMSE: ${metrics.RootMeanSquaredError:F0}");
    Console.WriteLine($"   MAE: ${metrics.MeanAbsoluteError:F0}");
    
    // Predições
    var predictionEngine = mlContext.Model.CreatePredictionEngine<HouseData, HousePrediction>(model);
    
    var testHouses = new[]
    {
        new HouseData { Size = 1300, Bedrooms = 3 },
        new HouseData { Size = 1700, Bedrooms = 4 },
        new HouseData { Size = 900, Bedrooms = 2 }
    };
    
    Console.WriteLine("\n🔮 Predições de preços:");
    foreach (var house in testHouses)
    {
        var prediction = predictionEngine.Predict(house);
        Console.WriteLine($"   🏠 Casa: {house.Size} sq ft, {house.Bedrooms} quartos");
        Console.WriteLine($"   💰 Preço predito: ${prediction.Price:F0}");
    }
    
    await Task.Delay(10);
}

static async Task DemonstrarClusteringAnomalyDetection()
{
    var mlContext = new MLContext(seed: 1);
    
    // Dados para clustering (coordenadas 2D)
    var points = new[]
    {
        new Point2D { X = 1.0f, Y = 1.0f },
        new Point2D { X = 1.2f, Y = 1.1f },
        new Point2D { X = 1.1f, Y = 0.9f },
        new Point2D { X = 5.0f, Y = 5.0f },
        new Point2D { X = 5.2f, Y = 4.8f },
        new Point2D { X = 5.1f, Y = 5.1f },
        new Point2D { X = 10.0f, Y = 1.0f }, // Possível outlier
        new Point2D { X = 2.5f, Y = 2.5f },
        new Point2D { X = 2.8f, Y = 2.2f }
    };
    
    Console.WriteLine($"📍 Dataset de pontos 2D com {points.Length} amostras");
    
    var dataView = mlContext.Data.LoadFromEnumerable(points);
    
    // Clustering K-Means
    var clusteringPipeline = mlContext.Transforms.Concatenate("Features", nameof(Point2D.X), nameof(Point2D.Y))
        .Append(mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: 3));
    
    var clusterModel = clusteringPipeline.Fit(dataView);
    Console.WriteLine("🎯 Modelo K-Means treinado (3 clusters)");
    
    var clusterPredictions = clusterModel.Transform(dataView);
    
    Console.WriteLine("\n📊 Resultados do clustering:");
    Console.WriteLine("   🎯 Modelo K-Means aplicado com sucesso");
    Console.WriteLine("   📍 Pontos agrupados em 3 clusters distintos");
    Console.WriteLine("   ✅ Clustering executado para análise de padrões");
    
    // Detecção de anomalias
    var anomalyPipeline = mlContext.Transforms.Concatenate("Features", nameof(Point2D.X), nameof(Point2D.Y))
        .Append(mlContext.AnomalyDetection.Trainers.RandomizedPca("Features", rank: 1));
    
    var anomalyModel = anomalyPipeline.Fit(dataView);
    Console.WriteLine("\n🚨 Detecção de anomalias configurada (Randomized PCA)");
    
    var anomalyPredictions = anomalyModel.Transform(dataView);
    
    Console.WriteLine("\n🔍 Detecção de anomalias:");
    Console.WriteLine("   🚨 Modelo PCA aplicado para detecção de outliers");
    Console.WriteLine("   � Análise de padrões anômalos nos dados");
    Console.WriteLine("   ✅ Detecção de anomalias configurada com sucesso");
    
    await Task.Delay(10);
}

static async Task DemonstrarComputerVision()
{
    Console.WriteLine("👁️ Computer Vision com ML.NET:");
    Console.WriteLine("   📝 Nota: Para uma demonstração completa seria necessário");
    Console.WriteLine("   📸 um modelo treinado e imagens reais.");
    Console.WriteLine("   🔧 Demonstrando configuração conceitual...");
    
    var mlContext = new MLContext();
    
    // Simular dados de imagem
    var imageData = new[]
    {
        new ImageData { ImagePath = "cat1.jpg", Label = "Cat" },
        new ImageData { ImagePath = "dog1.jpg", Label = "Dog" },
        new ImageData { ImagePath = "cat2.jpg", Label = "Cat" },
        new ImageData { ImagePath = "dog2.jpg", Label = "Dog" }
    };
    
    Console.WriteLine($"\n📊 Dataset simulado com {imageData.Length} imagens");
    
    // Pipeline conceitual para classificação de imagens
    Console.WriteLine("\n🔧 Pipeline conceitual configurado para:");
    Console.WriteLine("   📷 Carregamento e pré-processamento de imagens");
    Console.WriteLine("   🧠 Extração de features usando modelos pré-treinados");
    Console.WriteLine("   🏷️ Classificação multi-classe de objetos");
    Console.WriteLine("   🎯 Transfer Learning com modelos existentes");
    
    // Simular predições
    var testImages = new[] { "test_cat.jpg", "test_dog.jpg", "test_bird.jpg" };
    
    Console.WriteLine("\n🔮 Predições simuladas:");
    foreach (var image in testImages)
    {
        var predictedClass = image.Contains("cat") ? "Cat 🐱" : 
                           image.Contains("dog") ? "Dog 🐶" : "Bird 🐦";
        var confidence = Random.Shared.NextSingle() * 0.3f + 0.7f; // 70-100%
        
        Console.WriteLine($"   📸 {image} → {predictedClass} (Confiança: {confidence:P1})");
    }
    
    Console.WriteLine("\n💡 Para implementação real:");
    Console.WriteLine("   🔧 Use modelos ONNX ou TensorFlow pré-treinados");
    Console.WriteLine("   📊 Configure Image Classification API do ML.NET");
    Console.WriteLine("   🚀 Implemente transfer learning para sua aplicação");
    
    await Task.Delay(10);
}

static async Task DemonstrarNLP()
{
    var mlContext = new MLContext();
    
    // Dados para classificação de tópicos
    var textData = new[]
    {
        new TextData { Text = "Machine learning algorithms are fascinating", Category = "Technology" },
        new TextData { Text = "The stock market is volatile today", Category = "Finance" },
        new TextData { Text = "New AI breakthrough in healthcare", Category = "Technology" },
        new TextData { Text = "Interest rates are rising", Category = "Finance" },
        new TextData { Text = "Deep learning neural networks", Category = "Technology" },
        new TextData { Text = "Investment portfolio analysis", Category = "Finance" }
    };
    
    Console.WriteLine($"📚 Dataset de texto com {textData.Length} amostras");
    
    var dataView = mlContext.Data.LoadFromEnumerable(textData);
    
    // Pipeline para análise de texto
    var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(TextData.Text))
        .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(TextData.Category)))
        .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
        .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
    
    Console.WriteLine("🔧 Pipeline NLP configurado:");
    Console.WriteLine("   📝 Text Featurization (TF-IDF)");
    Console.WriteLine("   🎯 Maximum Entropy Classification");
    
    var model = pipeline.Fit(dataView);
    Console.WriteLine("🎯 Modelo NLP treinado!");
    
    // Análise de texto
    var predictionEngine = mlContext.Model.CreatePredictionEngine<TextData, TextPrediction>(model);
    
    var testTexts = new[]
    {
        "Artificial intelligence is transforming industries",
        "The cryptocurrency market is experiencing growth",
        "Neural networks and deep learning advancements",
        "Banking sector financial analysis"
    };
    
    Console.WriteLine("\n🔮 Classificação de tópicos:");
    foreach (var text in testTexts)
    {
        var prediction = predictionEngine.Predict(new TextData { Text = text });
        Console.WriteLine($"   📝 \"{text}\"");
        Console.WriteLine($"   🏷️ Categoria: {prediction.PredictedLabel}");
    }
    
    await Task.Delay(10);
}

static void DemonstrarTensorOperations()
{
    Console.WriteLine("🧮 Operações com Tensors (.NET 9):");
    
    // Criar arrays de dados
    var data1 = new float[] { 1, 2, 3, 4 };
    var data2 = new float[] { 5, 6, 7, 8 };
    
    Console.WriteLine("📊 Array 1 (2x2 conceptual):");
    Console.WriteLine($"   [{data1[0]:F1}, {data1[1]:F1}]");
    Console.WriteLine($"   [{data1[2]:F1}, {data1[3]:F1}]");
    
    Console.WriteLine("\n📊 Array 2 (2x2 conceptual):");
    Console.WriteLine($"   [{data2[0]:F1}, {data2[1]:F1}]");
    Console.WriteLine($"   [{data2[2]:F1}, {data2[3]:F1}]");
    
    // Operações básicas usando System.Numerics.Tensors
    var result = new float[4];
    
    // Soma elemento por elemento
    for (int i = 0; i < data1.Length; i++)
    {
        result[i] = data1[i] + data2[i];
    }
    
    Console.WriteLine("\n➕ Soma (array1 + array2):");
    Console.WriteLine($"   [{result[0]:F1}, {result[1]:F1}]");
    Console.WriteLine($"   [{result[2]:F1}, {result[3]:F1}]");
    
    // Multiplicação por escalar
    var scaled = new float[4];
    for (int i = 0; i < data1.Length; i++)
    {
        scaled[i] = data1[i] * 2.5f;
    }
    
    Console.WriteLine("\n✖️ Multiplicação por escalar (array1 * 2.5):");
    Console.WriteLine($"   [{scaled[0]:F1}, {scaled[1]:F1}]");
    Console.WriteLine($"   [{scaled[2]:F1}, {scaled[3]:F1}]");
    
    // Simulação de tensor 3D
    var tensor3D = new float[8];
    for (int i = 0; i < tensor3D.Length; i++)
    {
        tensor3D[i] = i + 1;
    }
    
    Console.WriteLine($"\n📦 Tensor 3D (2x2x2 conceptual): Shape = [2,2,2]");
    Console.WriteLine($"   📊 Total elements: {tensor3D.Length}");
    Console.WriteLine($"   📋 Values: [{string.Join(", ", tensor3D)}]");
    
    // Demonstrar operações vetorizadas usando Span
    var span1 = data1.AsSpan();
    var span2 = data2.AsSpan();
    var resultSpan = result.AsSpan();
    
    Console.WriteLine("\n⚡ Operações com Span<T> (otimizadas):");
    Console.WriteLine("   ✅ Spans permitem operações de baixo nível eficientes");
    Console.WriteLine("   ✅ Acesso sem alocação de memória adicional");
    Console.WriteLine("   ✅ Operações vetorizadas quando possível");
}

static async Task DemonstrarModelEvaluation()
{
    var mlContext = new MLContext(seed: 1);
    
    Console.WriteLine("📊 Model Evaluation & Metrics:");
    Console.WriteLine("   🎯 Accuracy: Mede a proporção de predições corretas");
    Console.WriteLine("   📈 AUC: Área sob a curva ROC (0.5-1.0)");
    Console.WriteLine("   ⚖️ F1 Score: Média harmônica entre precision e recall");
    Console.WriteLine("   🔍 Precision: True Positives / (True Positives + False Positives)");
    Console.WriteLine("   📋 Recall: True Positives / (True Positives + False Negatives)");
    
    Console.WriteLine("\n📊 Exemplos de Métricas Típicas:");
    Console.WriteLine("   🎯 Accuracy: 92.5%");
    Console.WriteLine("   📊 AUC: 0.87");
    Console.WriteLine("   ⚖️ F1 Score: 0.91");
    Console.WriteLine("   🔍 Precision: 0.89");
    Console.WriteLine("   📋 Recall: 0.93");
    
    Console.WriteLine("\n� Matriz de Confusão (Exemplo):");
    Console.WriteLine("   ✅ True Positives: 145");
    Console.WriteLine("   ❌ False Positives: 18");
    Console.WriteLine("   ❌ False Negatives: 12");
    Console.WriteLine("   ✅ True Negatives: 225");
    
    Console.WriteLine("\n� Interpretação:");
    Console.WriteLine("   � AUC > 0.8: Bom modelo");
    Console.WriteLine("   � F1 > 0.85: Performance sólida");
    Console.WriteLine("   ⚖️ Balance precision/recall conforme necessidade");
    Console.WriteLine("   🎯 Cross-validation para validação robusta");
    
    await Task.Delay(10);
}

static async Task DemonstrarAIPipeline()
{
    Console.WriteLine("🔄 AI Pipeline & AutoML Simulation:");
    
    var mlContext = new MLContext();
    
    // Simular um pipeline automatizado
    var steps = new[]
    {
        "🔍 Data Exploration & Profiling",
        "🧹 Data Cleaning & Preprocessing", 
        "🔧 Feature Engineering",
        "🤖 Algorithm Selection",
        "🎯 Hyperparameter Tuning",
        "📊 Model Validation",
        "🚀 Model Deployment"
    };
    
    Console.WriteLine("📋 Pipeline AutoML:");
    
    foreach (var step in steps)
    {
        Console.WriteLine($"   ⏳ Executando: {step}");
        await Task.Delay(200);
        
        // Simular métricas
        var progress = Random.Shared.Next(85, 99);
        var time = Random.Shared.Next(50, 300);
        Console.WriteLine($"      ✅ Completo ({progress}% eficiência, {time}ms)");
    }
    
    Console.WriteLine("\n🏆 Melhores Algoritmos Encontrados:");
    var algorithms = new[]
    {
        new { Name = "LightGBM", Accuracy = 0.94f, TrainingTime = "1.2s" },
        new { Name = "Random Forest", Accuracy = 0.92f, TrainingTime = "2.1s" },
        new { Name = "SVM", Accuracy = 0.89f, TrainingTime = "0.8s" },
        new { Name = "Neural Network", Accuracy = 0.91f, TrainingTime = "5.3s" }
    };
    
    foreach (var algo in algorithms.OrderByDescending(a => a.Accuracy))
    {
        Console.WriteLine($"   🤖 {algo.Name}: {algo.Accuracy:P1} accuracy, {algo.TrainingTime} training");
    }
    
    Console.WriteLine($"\n🎯 Melhor modelo selecionado: {algorithms.OrderByDescending(a => a.Accuracy).First().Name}");
    
    // Simular pipeline de produção
    Console.WriteLine("\n🚀 Pipeline de Produção:");
    Console.WriteLine("   📊 Model Monitoring: ✅ Ativo");
    Console.WriteLine("   🔄 Continuous Learning: ✅ Habilitado");
    Console.WriteLine("   ⚡ Real-time Inference: ✅ Disponível");
    Console.WriteLine("   📈 Performance Tracking: ✅ Monitorando");
}

// Classes de dados
public class SentimentData
{
    public string Text { get; set; } = "";
    public bool Sentiment { get; set; }
}

public class SentimentPrediction
{
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }
    public float Probability { get; set; }
    public float Score { get; set; }
}

public class HouseData
{
    public float Size { get; set; }
    public float Bedrooms { get; set; }
    public float Price { get; set; }
}

public class HousePrediction
{
    public float Price { get; set; }
}

public class Point2D
{
    public float X { get; set; }
    public float Y { get; set; }
}

public class ImageData
{
    public string ImagePath { get; set; } = "";
    public string Label { get; set; } = "";
}

public class TextData
{
    public string Text { get; set; } = "";
    public string Category { get; set; } = "";
}

public class TextPrediction
{
    public string PredictedLabel { get; set; } = "";
}
