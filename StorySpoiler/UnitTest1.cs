using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;



namespace StorySpoiler
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string? lastStoryId;

        private static string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";
        private const string token =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJmZmIwMDU0MS01YjBiLTQ3OGEtOTNmZC1iNjdmNzBmNzU5MDAiLCJpYXQiOiIwOC8xNi8yMDI1IDA2OjA4OjQ4IiwiVXNlcklkIjoiYjQ3NTg3N2EtOWI2OC00ZDRjLThkY2YtMDhkZGRiMWExM2YzIiwiRW1haWwiOiJtdnZAZ21haWwuY29tIiwiVXNlck5hbWUiOiJtdnYiLCJleHAiOjE3NTUzNDYxMjgsImlzcyI6IlN0b3J5U3BvaWxfQXBwX1NvZnRVbmkiLCJhdWQiOiJTdG9yeVNwb2lsX1dlYkFQSV9Tb2Z0VW5pIn0.-YLRfr0zbjLWO5h6f3-exRg0ZZDJOtzkCrddqMzoLMU";

        private const string LoginEmail = "mvv@gmail.com";
        private const string LoginPass = "123456";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrEmpty(token))
            {
                jwtToken = token;

            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPass);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string loginEmail, string loginPassword)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/Users/Authentication", Method.Post);

            request.AddJsonBody(new { loginEmail, loginPassword });

            var response = tempClient.Execute(request);

            var content = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return content.GetProperty("accessToken").GetString() ?? string.Empty;

        }
        //All tests here

        [Order(1)]
        [Test]

        public void CreateNewStorySpoilerWhitRequiredFields_ShouldReturnSuccess()
        {
            var story = new StoryDTO
            {
                Title = "Test title",
                Description = "Test description",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = this.client.Execute(request);
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(json, Is.Not.Null);
            Assert.That(json.Msg, Is.EqualTo("Successfully created!"));
            

            

        }

        [Order(2)]
        [Test]
        public void GetAllStorys_ShouldReturnSuccess()
        {
            var reqest = new RestRequest("/api/Story/All", Method.Get);

            var response = this.client.Execute(reqest);
            var json = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json, Is.Not.Empty.Or.Null);

            lastStoryId = json.LastOrDefault()?.Id;

        }



        [Order(3)]
        [Test]
        public void EditTheCreatedStorySpoiler_ShouldReturnSuccess()
        {
            var story = new StoryDTO
            {
                Title = "Edited title",
                Description = "Edited description",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/{lastStoryId}", Method.Put);
            request.AddJsonBody(story);


            var response = client.Execute(request);


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(json.Msg, Is.EqualTo("Successfully edited"));
            
        }

        [Order(4)]
        [Test]
        public void DeleteExistingStory_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Story/Delete/{lastStoryId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var json = JsonSerializer.Deserialize<ApiResponseDTO> (response.Content);
            Assert.That(json.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateStoryWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var story = new StoryDTO
            {
                Title = "",
                Description = "",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            

        }

        [Order(6)]
        [Test]
        public void EditingNonExistingStory_ShouldReturnNotFound()
        {
            var story = new StoryDTO
            {
                Title = "New",

                Description = "new",

                Url = ""
            };
            var request = new RestRequest("/api/Story/Edit/NonExistingQP", Method.Put);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(json.Msg, Is.EqualTo("No spoilers..."));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            var request = new RestRequest($"/api/Story/Delete/NonExistingQP", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(json.Msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }
































        [OneTimeTearDown]
        public void TearDown()
        {
            this.client.Dispose();
        }



    }
}