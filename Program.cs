﻿using System;
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
        private const int KEY_SIZE_LIMIT = 300;

        // Specify your bucket region (an example region is shown).
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast1;
        private static IAmazonS3 s3Client;
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            s3Client = new AmazonS3Client( bucketRegion );
            //UploadFileAsync().Wait();
           // FindTaskByTag().Wait();
            DownloadFileFromS3();
        }

        private static async Task UploadFileAsync() {

            // uploading the file to the S3 bucket and added in 2 tags to the object so that the service can down below can query for Skills that the person may have in the resume

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

                // Performs the actual upload to the S3 bucket
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
            // task to retrieve all of the objects in the S3 bucket and performs a search for a user with Linux experience
             try {
                 // retrieve the list of objects
                 ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    MaxKeys = KEY_SIZE_LIMIT
                 };
                ListObjectsV2Response response;
                do
                {
                    response = await s3Client.ListObjectsV2Async(request);

                    // Process the response.

                    // iterates through all of the objects and determines if the object contains any linux experience
                    foreach (S3Object entry in response.S3Objects)
                    {
                        
                        Console.WriteLine("key = {0} size = {1}",
                            entry.Key, entry.Size);
                            GetObjectTaggingRequest getObjectTaggingRequest = new GetObjectTaggingRequest();
                            getObjectTaggingRequest.BucketName = entry.BucketName;
                            getObjectTaggingRequest.Key = entry.Key;

                            // iterates through all of the tags within the object
                            GetObjectTaggingResponse objectTaggingResponse = await s3Client.GetObjectTaggingAsync( getObjectTaggingRequest );

                            Console.WriteLine( "Metadata tag value for " + entry.Key + " are the following." );
                            for( int i = 0; i < objectTaggingResponse.Tagging.Count; i++ ) {
                                Console.WriteLine( "Metadata tag Key: " + objectTaggingResponse.Tagging[ i ].Key + "Metadata Value: " + objectTaggingResponse.Tagging[ i ].Value );
                                
                                if( objectTaggingResponse.Tagging[ i ].Value.Equals( "Linux" ) ) {
                                    Console.WriteLine( "Service found Candidate with Linux Skill Set!" );
                                }
                            }
                    }
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

        private static void DownloadFileFromS3() {
            try {
                TransferUtility fileTransferUtility = new TransferUtility( s3Client );
                Console.WriteLine( "#### DOWNLOADING FILE FROM S3 TO FILE SYSTEM. ####" );

                fileTransferUtility.Download( "file-drop/candidate-tracker-site-test.xml", bucketName, "candidate-tracker-site-test.xml" );
                Console.WriteLine( "File Download is complete" );
            }
            catch ( AmazonS3Exception amazonS3Exception  ) {
                Console.WriteLine("S3 error occurred. Exception: " + amazonS3Exception.ToString());
                Console.ReadKey();
            }
            catch ( Exception e ) {
                Console.WriteLine("Exception: " + e.ToString());
                Console.ReadKey();
            }
        }


    }
}
