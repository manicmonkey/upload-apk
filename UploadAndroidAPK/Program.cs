using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using RestSharp;

namespace UploadAndroidAPK
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Required args: <application_id> <version> <client_id> <client_secret> <refresh_token>");
                return;
            }

            Console.WriteLine("Preparing to upload APK");
            var applicationId = args[0];
            var version = int.Parse(args[1]);
            var clientId = args[2];
            var clientSecret = args[3];
            var refreshToken = args[4];
            var accessToken = GetAccessToken(clientId, clientSecret, refreshToken);
            var restClient = new RestClient("https://www.googleapis.com");
            var editId = CreateEdit(restClient, applicationId, accessToken);
            UploadApk(restClient, accessToken, applicationId, editId);
            SetTrack(restClient, accessToken, applicationId, editId, version);
            CommitEdit(restClient, accessToken, applicationId, editId);

            if (Debugger.IsAttached)
                Console.ReadLine();
        }
        
        private static string GetAccessToken(string clientId, string clientSecret, string refreshToken)
        {
            var client = new RestClient("https://accounts.google.com/o/oauth2/token");
            var request = new RestRequest(Method.POST);
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("client_id", clientId);
            request.AddParameter("client_secret", clientSecret);
            request.AddParameter("refresh_token", refreshToken);
            var response = client.Execute<RefreshTokenResponse>(request);
            CheckResponse(response);
            return response.Data.AccessToken;
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

        private static void UploadApk(IRestClient restClient, string accessToken, string applicationId, string editId)
        {
            Console.WriteLine("Uploading apk");
            var request = new RestRequest("/upload/androidpublisher/v2/applications/{applicationId}/edits/{editId}/apks", Method.POST);
            request.AddUrlSegment("applicationId", applicationId);
            request.AddUrlSegment("editId", editId);
            request.AddQueryParameter("access_token", accessToken);
            request.AddFile("file", "Build/amyloids.apk");
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
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Unexpected statusCode: " + response.StatusCode);
            }
        }
    }
}
