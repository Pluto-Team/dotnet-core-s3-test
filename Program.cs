using System;
using Amazon.S3.Transfer;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace dotnet_core_s3_test
{
    public class Program
    {
        private const string bucketName = "candidate-tracker-candidate-documents";
        private const string keyName = "20200721_120407.jpg";
        private const string filePath = "/Users/brian.highnam/Downloads/" + keyName;
        // Specify your bucket region (an example region is shown).
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast1;
        private static IAmazonS3 s3Client;
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            s3Client = new AmazonS3Client( bucketRegion );
            UploadFileAsync().Wait();
            FindTaskByTag().Wait();
        }

        private static async Task UploadFileAsync() {

            try {
                var fileTransferUtility =
                    new TransferUtility(s3Client);
                
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    FilePath = filePath,
                    StorageClass = S3StorageClass.Standard,
                    Key = keyName,
                    CannedACL = S3CannedACL.Private,
                    TagSet = new List<Tag>{
                        new Tag { Key = "Skill 1", Value = ".NET"},
                        new Tag { Key = "Skill 2", Value = "Linux" }
                    }
                };

                PutObjectTaggingRequest putObjectTaggingRequest = new PutObjectTaggingRequest();
                putObjectTaggingRequest.BucketName = bucketName;
                putObjectTaggingRequest.Key = keyName;



                await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
                Console.WriteLine("Upload complete");
            }

             catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }

        }

        private static async Task FindTaskByTag() {
             try {
                 ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    MaxKeys = 10
                };
                ListObjectsV2Response response;
                do
                {
                    response = await s3Client.ListObjectsV2Async(request);

                    // Process the response.
                    foreach (S3Object entry in response.S3Objects)
                    {
                        
                        Console.WriteLine("key = {0} size = {1}",
                            entry.Key, entry.Size);
                            GetObjectTaggingRequest getObjectTaggingRequest = new GetObjectTaggingRequest();
                            getObjectTaggingRequest.BucketName = entry.BucketName;
                            getObjectTaggingRequest.Key = entry.Key;

                            GetObjectTaggingResponse objectTaggingResponse = await s3Client.GetObjectTaggingAsync( getObjectTaggingRequest );
                            Console.WriteLine( "Metadata tag value for " + entry.Key + " are the following." );
                            for( int i = 0; i < objectTaggingResponse.Tagging.Count; i++ ) {
                                Console.WriteLine( "Metadata tag Key: " + objectTaggingResponse.Tagging[ i ].Key + "Metadata Value: " + objectTaggingResponse.Tagging[ i ].Value );
                                
                                if( objectTaggingResponse.Tagging[ i ].Value.Equals( "Linux" ) ) {
                                    Console.WriteLine( "Service found Candidate with Linux Skill Set!" );
                                }
                            }
                    }
                    Console.WriteLine("Next Continuation Token: {0}", response.NextContinuationToken);
                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);
             }

             catch (AmazonS3Exception amazonS3Exception)
            {
                Console.WriteLine("S3 error occurred. Exception: " + amazonS3Exception.ToString());
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
                Console.ReadKey();
            }

        }


    }
}
