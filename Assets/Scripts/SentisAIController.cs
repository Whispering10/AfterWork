using UnityEngine;
using Unity.InferenceEngine; 

public class NeuralNetworkRunner : MonoBehaviour
{
    public ModelAsset modelAsset; 

    private Model runtimeModel;
    private Worker worker; 

    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);

        worker = new Worker(runtimeModel, BackendType.GPUCompute);
    }

    void Update()
    {
        RunInference();
    }

    void RunInference()
    {
     
        Tensor<float> inputTensor = new Tensor<float>(new TensorShape(1, 224, 224, 3));

        worker.Schedule(inputTensor);

        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

        inputTensor.Dispose();
        outputTensor?.Dispose();
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}