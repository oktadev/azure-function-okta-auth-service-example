using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace AuthService
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			var OktaDomain = "https://dev-583903.oktapreview.com";
			var OktaApiToken = "00fWkOjwwg9xiFd-Xfgm_ePATIRxVj850Iblbb1DS_";
			Session session = null;
			User user = null;

			//get username
			string publicKey = req.GetQueryNameValuePairs()
				.FirstOrDefault(q => string.Compare(q.Key, "user", true) == 0)
				.Value;

			if (publicKey == null)
				req.CreateResponse<AuthResponse>(new AuthResponse() { WasSuccessful = false, Message = "Must pass `user` as a query string parameter" });

			//get password
			string privateKey = req.GetQueryNameValuePairs()
				.FirstOrDefault(q => string.Compare(q.Key, "password", true) == 0)
				.Value;

			if (privateKey == null)
				req.CreateResponse<AuthResponse>(new AuthResponse() { WasSuccessful = false, Message = "Must pass `password` as a query string parameter" });

			//generate URL for service call using your configured Okta Domain
			string url = string.Format("{0}/api/v1/authn", OktaDomain);

			//build the package we're going to send to Okta
			var data = new OktaAuthenticationRequest() { username = publicKey, password = privateKey };

			//serialize input as json
			var json = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

			//create HttpClient to communicate with Okta's web service
			using (HttpClient client = new HttpClient())
			{
				//Set the API key
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("SSWS", OktaApiToken);

				//Post the json data to Okta's web service
				using (HttpResponseMessage res = await client.PostAsync(url, json))

				//Get the response from the server
				using (HttpContent content = res.Content)
				{
					//get json string from the response
					var responseJson = await content.ReadAsStringAsync();

					//deserialize json into complex object
					dynamic responseObj = JsonConvert.DeserializeObject(responseJson);

					//determine if the returned status is success
					if (responseObj.status == "SUCCESS")
					{
						//get session data
						session = new Session()
						{
							Token = responseObj.sessionToken,
							ExpiresAt = responseObj.expiresAt
						};

						//get user data
						user = new User()
						{
							Id = responseObj._embedded.user.id,
							Login = responseObj._embedded.user.login,
							Locale = responseObj._embedded.user.locale,
							TimeZone = responseObj._embedded.user.timeZone,
							FirstName = responseObj._embedded.user.firstName,
							LastName = responseObj._embedded.user.lastName
						};
					}
				}
			}

			//response
			var wasSuccess = session != null && user != null;
			return req.CreateResponse<AuthResponse>(new AuthResponse()
			{
				WasSuccessful = wasSuccess,
				Message = wasSuccess ? "Success" : "Invalid username and password",
				Session = session,
				User = user
			});
		}
	}
}
