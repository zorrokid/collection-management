using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace ProcessImage.Services;

public interface IImageAnalyzer
{
    Task<IEnumerable<ReadResult>> AnalyzeImageContent(string urlFile);
}

public class ImageAnalyzer : IImageAnalyzer
{
    private readonly ComputerVisionClient client;

    public ImageAnalyzer(ComputerVisionClient client)
    {
        this.client = client;
    }

    public async Task<IEnumerable<ReadResult>> AnalyzeImageContent(string urlFile)
    {
        var textHeaders = await client.ReadAsync(urlFile);
        string operationLocation = textHeaders.OperationLocation;

        const int numberOfCharsInOperationId = 36;
        string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

        // Read back the results from the analysis request
        ReadOperationResult results;
        do
        {
            results = await client.GetReadResultAsync(Guid.Parse(operationId));
        }
        while (results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted);

        return results.AnalyzeResult.ReadResults;
    }
}