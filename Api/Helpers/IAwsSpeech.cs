using Amazon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MovieProfanityDetector.Models;
using Newtonsoft.Json;

namespace MovieProfanityDetector.Helpers
{
    public interface IAwsSpeech
    {
        public Task<TranscriptionJobResult> TranscribeInputFile(Guid movieGuid, string targetLanguageCode= "en-US");
    }

    public class AwsSpeech : IAwsSpeech, IDisposable
    {
        private readonly AwsSettings _options; 
        public AwsSpeech(IOptions<AwsSettings> option, RegionEndpoint regionEndpoint = null)
        {
             _options = option.Value;

            var credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
            //var config = new AmazonS3Config
            //{
            //    RegionEndpoint = regionEndpoint ?? RegionEndpoint.EUWest2
            //};
            RegionEndpoint = regionEndpoint ?? RegionEndpoint.EUWest2;
            //todo add region endpoint for AWS Face
            //S3Client = new AmazonS3Client(RegionEndpoint);
            S3Client = new AmazonS3Client( credentials,RegionEndpoint);
            TranscribeClient = new AmazonTranscribeServiceClient(credentials,RegionEndpoint);
        }

        private RegionEndpoint RegionEndpoint { get; }

        private AmazonTranscribeServiceClient TranscribeClient { get; }

        private AmazonS3Client S3Client { get; }

        public void Dispose()
        {
            //TODO remember to call
            S3Client.Dispose();
            TranscribeClient.Dispose();
            //TODO dispose for faceClient
            //todo dispose for gcp speech and azure speech
        }

        public async Task<TranscriptionJobResult> TranscribeInputFile(Guid movieGuid, string targetLanguageCode = "en-US")
        {
            var bucketName = _options.BucketName + @"/" + movieGuid;



            var putBucketResponse = await CreateBucket(bucketName);
            if (putBucketResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                var uploadInputFileToS3 = await UploadInputFileToS3(movieGuid, bucketName);
                if (uploadInputFileToS3.HttpStatusCode == HttpStatusCode.OK)
                {
                    var startTranscriptionJobResponse =
                        await TranscribeInputFile(movieGuid, bucketName, targetLanguageCode);

                    return startTranscriptionJobResponse;
                    //todo
                    //todo delete bucket
                }
                else
                {
                    //Logger.WriteLine($"Fail to transcribe {fileName} because cannot upload {fileName} to {bucketName}",
                    //    uploadInputFileToS3);
                }
            }
            else
            {
                //Logger.WriteLine($"Fail to transcribe {fileName} because cannot create bucket {bucketName}",
                //    putBucketResponse);
            }
            throw new BadHttpRequestException("Error Transcribe File");
        }

        private async Task<TranscriptionJobResult> TranscribeInputFile(Guid movieGuid, string bucketName,
            string targetLanguageCode)
        {
            //var objectName = Path.GetFileName(fileName);

            var media = new Media()
            {
                MediaFileUri = $"https://{_options.BucketName}.s3.{RegionEndpoint.SystemName}.amazonaws.com/{movieGuid}/{movieGuid}"
            };

            var transcriptionJobName = $"transcribe-job-{DateTime.Now.ToString("yyMMddHHmmss")}";
            var transcriptionJobRequest = new StartTranscriptionJobRequest()
            {
                LanguageCode = targetLanguageCode,
                Media = media,
                MediaFormat = MediaFormat.Wav,
                TranscriptionJobName = transcriptionJobName,
                OutputBucketName = _options.BucketName
            };

            var startTranscriptionJobResponse = await TranscribeClient.StartTranscriptionJobAsync(transcriptionJobRequest);
            if (startTranscriptionJobResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                return await WaitForTranscriptionJob(startTranscriptionJobResponse.TranscriptionJob, _options.BucketName ,movieGuid);
            }
            else
            {
                //todo
                throw new NotImplementedException();
            }
        }

        private async Task<TranscriptionJobResult> WaitForTranscriptionJob(TranscriptionJob transcriptionJob,
            string bucketName, Guid movieGuid, int delayTime = 16000)
        {
            var transcriptionJobTranscriptionJobStatus = transcriptionJob.TranscriptionJobStatus;
            //Logger.WriteLine($"transcriptionJobTranscriptionJobStatus={transcriptionJobTranscriptionJobStatus}");
            if (transcriptionJobTranscriptionJobStatus ==
                TranscriptionJobStatus.COMPLETED)
            {
                var keyName = $"{transcriptionJob.TranscriptionJobName}.json";
                // Logger.WriteLine($"Downloading {keyName}");
                var result = await GetFileFromS3(keyName, bucketName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Transcriptions");

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                await File.WriteAllTextAsync(Path.Combine(filePath, movieGuid + ".json"), result);

                return JsonConvert.DeserializeObject<TranscriptionJobResult>(result);
                /*using var stringReader = new StringReader(result);
                using var jsonTextReader = new JsonTextReader(stringReader);*/
            }
            else if (transcriptionJobTranscriptionJobStatus == TranscriptionJobStatus.FAILED)
            {
                //TODO
                throw new NotImplementedException();
            }
            else
            {
                await Task.Delay(delayTime);
                var getTranscriptionJobResponse = await TranscribeClient.GetTranscriptionJobAsync(
                    new GetTranscriptionJobRequest()
                    {
                        TranscriptionJobName = transcriptionJob.TranscriptionJobName
                    });

                return await WaitForTranscriptionJob(getTranscriptionJobResponse.TranscriptionJob, bucketName,movieGuid,
                    delayTime * 2);
            }
        }

        public async Task<PutBucketResponse> CreateBucket(string bucketName)
        {
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = bucketName,
            };

            return await S3Client.PutBucketAsync(putBucketRequest);
        }

        public async Task<PutObjectResponse> UploadInputFileToS3(Guid movieGuid, string bucketName)
        {
            //var objectName = Path.GetFileName(fileName);

            var putObjectRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = movieGuid.ToString(),
                ContentType = "audio/wav",
                FilePath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot",
                    Path.Combine("Movies", movieGuid.ToString() , movieGuid + ".wav"))
            };

            return await S3Client.PutObjectAsync(putObjectRequest);
        }

        public async Task<string> GetFileFromS3(string keyName, string bucketName)
        {
            var request = new GetObjectRequest()
            {
                BucketName = bucketName,
                Key = keyName
            };
            using var response = await S3Client.GetObjectAsync(request);
            using var responseStream = response.ResponseStream;
            using var reader = new StreamReader(responseStream);
            /*string title = response.Metadata["x-amz-meta-title"]; // Assume you have "title" as medata added to the object.
            string contentType = response.Headers["Content-Type"];
            Console.WriteLine("Object metadata, Title: {0}", title);
            Console.WriteLine("Content type: {0}", contentType);*/

            return await reader.ReadToEndAsync(); // Now you process the response body.
        }
    }

    //todo move
    public class TranscriptionJobResult
    {
        public string jobName { get; set; }
        public string accountId { get; set; }
        public string status { get; set; }
        public TranscriptionResult results { get; set; }
    }

    public class TranscriptionResult
    {
        public List<Transcript> transcripts { get; set; }
        public List<TranscriptItem> items { get; set; }
    }

    public class Transcript
    {
        public string transcript { get; set; }
    }

    public class TranscriptItem
    {
        public string start_time { get; set; }
        public string end_time { get; set; }
        public List<AlternativeTranscription> alternatives { get; set; }
        public string type { get; set; }
    }

    public class AlternativeTranscription
    {
        public string confidence { get; set; }
        public string content { get; set; }
    }
}