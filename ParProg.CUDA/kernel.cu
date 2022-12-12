
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>
#include "Decoder.h"
#include <vector>

cudaError_t count_gt_leq_with_cuda(std::vector<long long int> data, long long int target, int* leq, int* gt);

__global__ void count_gt_leq_kernel(long long int* data, long long int target, int* res_leq, int* res_gt, unsigned int size)
{
    int elem_index = blockDim.x * blockIdx.x + threadIdx.x;
    
    if (elem_index >= size)
        return;

    long long int value = data[elem_index];
    printf("%d | [%d->%d]=%lld\n", threadIdx.x, elem_index, size, value);
    if (abs(value) > target) res_gt[threadIdx.x]++;
    else res_leq[threadIdx.x]++;
}

int main()
{
    auto data = Decode("file2.wav");
    printf("File length is %d", data.size());
    long long int target = 16000;
    int leq = 0; 
    int gt = 0;

    cudaError_t cudaStatus = count_gt_leq_with_cuda(data, target, &leq, &gt);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "count_gt_leq_with_cuda failed! ");
        return 1;
    }

    printf("leq = %d | gt = %d", leq, gt);
    getchar();

    // cudaDeviceReset must be called before exiting in order for profiling and
    // tracing tools such as Nsight and Visual Profiler to show complete traces.
    cudaStatus = cudaDeviceReset();
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaDeviceReset failed!");
        return 1;
    }

    return 0;
}

cudaError_t count_gt_leq_with_cuda(std::vector<long long int> data, long long int target, int* leq, int* gt)
{
    long long int* dev_data = 0;
    int *dev_leq = 0, *dev_gt = 0;
    auto size = data.size();
    int *host_leq = new int[size], *host_gt = new int[size];
    const int THREAD_COUNT = 1024;
    dim3 gridSize = dim3(size / THREAD_COUNT + 1);
    dim3 blockSize = dim3(THREAD_COUNT);

    cudaError_t cudaStatus;

    cudaStatus = cudaSetDevice(0);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaSetDevice failed!  Do you have a CUDA-capable GPU installed?");
        goto Error;
    }

    // Выделяем память на GPU для входных данных
    cudaStatus = cudaMalloc((void**)&dev_data, size * sizeof(long long int));
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }

    // Выделяем память на GPU для выходных данных
    cudaStatus = cudaMalloc((void**)&dev_leq, THREAD_COUNT * sizeof(int));
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }

    cudaStatus = cudaMalloc((void**)&dev_gt, THREAD_COUNT * sizeof(int));
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }
    
    // Копируем входные данные в буффер GPU
    cudaStatus = cudaMemcpy(dev_data, data.data(), size * sizeof(long long int), cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }

    count_gt_leq_kernel<<<gridSize, blockSize>>>(dev_data, target, dev_leq, dev_gt, size);
    
    cudaStatus = cudaGetLastError();
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "count_gt_leq_kernel launch failed: %s\n", cudaGetErrorString(cudaStatus));
        goto Error;
    }

    // Ждем выполнения всех задач на GPU
    cudaStatus = cudaDeviceSynchronize();
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaDeviceSynchronize returned error code %d after launching addKernel!\n", cudaStatus);
        goto Error;
    }

    // Копируем значения из буффера gpu на хост
    cudaStatus = cudaMemcpy(host_leq, dev_leq, THREAD_COUNT * sizeof(int), cudaMemcpyDeviceToHost);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }

    cudaStatus = cudaMemcpy(host_gt, dev_gt, THREAD_COUNT * sizeof(int), cudaMemcpyDeviceToHost);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }

    // Собираем результат с ядер
    for (int i = 0; i < THREAD_COUNT; i++)
    {
        (*gt) += host_gt[i];
        (*leq) += host_leq[i];
    }

Error:
    cudaFree(dev_data);
    cudaFree(dev_leq);
    cudaFree(dev_gt);
    free(host_gt);
    free(host_leq);
    return cudaStatus;
}
