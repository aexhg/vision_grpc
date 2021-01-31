using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Google.Cloud.Storage.V1;
using Google.Apis.Storage.v1.Data;


namespace vision
{
    public sealed class VisionService : Vision.VisionBase
    {
        readonly CloudStorageOptions _options;
        readonly StorageClient _storage;

        private readonly ILogger<VisionService> _logger;
        public VisionService(ILogger<VisionService> logger, IOptions<CloudStorageOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            _storage = StorageClient.Create();
        }

        public override async Task<Response> Load(VisionMessage request, ServerCallContext context)
        {
            _logger.LogDebug("Load message {}", request.Meta);
            await SaveToStorage(request);
            var response = new Response(){Success = true};
            return response;
        }

        private async Task SaveToStorage(VisionMessage request)
        {
            var meta = JsonSerializer.Deserialize<Dictionary<string, string>>(request.Meta);
            var obj = new Google.Apis.Storage.v1.Data.Object()
            {
                Bucket = _options.BucketName,
                Name = Guid.NewGuid().ToString(),
                ContentType = "image/jpeg",
                Metadata = meta
            };
            _logger.LogDebug("Save object {} to {}", obj.Name, _options.BucketName);
            await _storage.UploadObjectAsync(
                obj,
                new MemoryStream(request.Data.ToByteArray())
            );
            // await _storage.UploadObjectAsync(
            // _options.BucketName, Guid.NewGuid().ToString(), "image/jpeg",
            // new MemoryStream(request.Data.ToByteArray()));
        }

    }   
}
