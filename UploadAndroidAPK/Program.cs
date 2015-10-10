using System;
using System.Diagnostics;
using System.Net;
using RestSharp;

namespace UploadAndroidAPK
{
    class Program
    {
        private const int _10_MIN_IN_MILLIS = 600000;

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Required args: <application_id> <version> <file_path>");
                return;
            }

            var applicationId = args[0];
            Console.Out.WriteLine("applicationId = {0}", applicationId);
            var version = int.Parse(args[1]);
            Console.Out.WriteLine("version = {0}", version);
            var apkPath = args[2];
            Console.Out.WriteLine("apkPath = {0}", apkPath);
            var accessToken = Console.ReadLine();
            Console.Out.WriteLine("accessToken = {0}", accessToken);

            Console.WriteLine("Preparing to upload APK");
            
            var restClient = new RestClient("https://www.googleapis.com");
            var editId = CreateEdit(restClient, applicationId, accessToken);
            UploadApk(restClient, accessToken, applicationId, editId, apkPath);
            SetTrack(restClient, accessToken, applicationId, editId, version);
            CommitEdit(restClient, accessToken, applicationId, editId);

            if (Debugger.IsAttached)
                Console.ReadLine();
        }

        private static string CreateEdit(IRestClient restClient, string applicationId, string accessToken)
        {
            Console.WriteLine("Creating edit");
            var request = new RestRequest("/androidpublisher/v2/applications/{applicationId}/edits", Method.POST);
            request.AddUrlSegment("applicationId", applicationId);
            request.AddQueryParameter("access_token", accessToken);
            var response = restClient.Execute<EditResponse>(request);
            CheckResponse(response);
            return response.Data.Id;
        }

        private static void UploadApk(IRestClient restClient, string accessToken, string applicationId, string editId, string apkPath)
        {
            Console.WriteLine("Uploading apk");
            var request = new RestRequest("/upload/androidpublisher/v2/applications/{applicationId}/edits/{editId}/apks", Method.POST);
            request.Timeout = _10_MIN_IN_MILLIS;
            request.AddUrlSegment("applicationId", applicationId);
            request.AddUrlSegment("editId", editId);
            request.AddQueryParameter("access_token", accessToken);
            request.AddFile("file", apkPath);
            var response = restClient.Execute<EditResponse>(request);
            CheckResponse(response);
        }

        private static void SetTrack(IRestClient restClient, string accessToken, string applicationId, string editId, int version)
        {
            Console.WriteLine("Setting new version");
            var request = new RestRequest("/androidpublisher/v2/applications/{applicationId}/edits/{editId}/tracks/alpha", Method.PATCH);
            request.AddUrlSegment("applicationId", applicationId);
            request.AddUrlSegment("editId", editId);
            request.AddQueryParameter("access_token", accessToken);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new { track = "alpha", versionCodes = new[] { version } });
            var response = restClient.Execute<EditResponse>(request);
            CheckResponse(response);
        }

        private static void CommitEdit(IRestClient restClient, string accessToken, string applicationId, string editId)
        {
            Console.WriteLine("Committing edit");
            var request = new RestRequest("/androidpublisher/v2/applications/{applicationId}/edits/{editId}:commit", Method.POST);
            request.AddUrlSegment("applicationId", applicationId);
            request.AddUrlSegment("editId", editId);
            request.AddQueryParameter("access_token", accessToken);
            var response = restClient.Execute<EditResponse>(request);
            CheckResponse(response);
        }

        private static void CheckResponse(IRestResponse response)
        {
            Console.Out.WriteLine("Got status code: " + response.StatusCode);
            Console.Out.WriteLine("Got content: " + response.Content);
            if (response.StatusCode == HttpStatusCode.OK)
                return;
            Console.Out.WriteLine("Got error message: " + response.ErrorMessage);
            Console.Out.WriteLine("Got error exception: " + response.ErrorException);
            throw new Exception("Unexpected statusCode: " + response.StatusCode);
        }
    }
}
