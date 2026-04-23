using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace SimpleNotesTests
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string CreatedNoteId;

        private const string BaseUrl = "http://144.91.123.158:5005";

        private const string Email = "Sevdalin.Atanasov.Atanasov1985@softuni.bg";
        private const string Password = "Sevdalin1985";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken(Email, Password);

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);

            var request = new RestRequest("/api/User/Authorization", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            var json = JsonDocument.Parse(response.Content);
            return json.RootElement.GetProperty("accessToken").GetString();
        }

        // ❌ 1.3 Create invalid note
        [Test, Order(1)]
        public void CreateNote_Invalid_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Note/Create", Method.Post);

            request.AddJsonBody(new { });

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        // ✅ 1.4 Create valid note
        [Test, Order(2)]
        public void CreateNote_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Note/Create", Method.Post);

            request.AddJsonBody(new
            {
                title = "My First Note",
                description = "This is a valid description with more than thirty characters.",
                status = "New"
            });

            var response = client.Execute(request);
            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Msg, Is.EqualTo("Note created successfully!"));
        }

        // ✅ 1.5 Get all + extract ID
        [Test, Order(3)]
        public void GetAllNotes_ShouldReturnList_AndExtractId()
        {
            var request = new RestRequest("/api/Note/AllNotes", Method.Get);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var notes = JsonSerializer.Deserialize<List<NoteDto>>(
                JsonDocument.Parse(response.Content)
                .RootElement
                .GetProperty("allNotes")
                .GetRawText()
            );

            Assert.That(notes, Is.Not.Null);
            Assert.That(notes, Is.Not.Empty);

            CreatedNoteId = notes.Last().Id;

            Assert.That(CreatedNoteId, Is.Not.Null.And.Not.Empty);
        }

        // ✏️ 1.6 Edit note
        [Test, Order(4)]
        public void EditNote_ShouldWork()
        {
            var request = new RestRequest($"/api/Note/Edit/{CreatedNoteId}", Method.Put);

            request.AddJsonBody(new
            {
                title = "Updated Note",
                description = "This is an updated description that is definitely longer than thirty characters.",
                status = "Done"
            });

            var response = client.Execute(request);
            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Msg, Is.EqualTo("Note edited successfully!"));
        }

        // ❌ 1.7 Delete note
        [Test, Order(5)]
        public void DeleteNote_ShouldWork()
        {
            var request = new RestRequest($"/api/Note/Delete/{CreatedNoteId}", Method.Delete);

            var response = client.Execute(request);
            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Msg, Is.EqualTo("Note deleted successfully!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            client?.Dispose();
        }
    }
}