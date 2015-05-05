using Orchard;
using Orchard.Localization;
using Orchard.Services;
using OShop.PayPal.Models;
using OShop.PayPal.Models.Api;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OShop.PayPal.Services {
    public class PaypalApiService : IPaypalApiService {
        public const string SandboxEndpoint = "https://api.sandbox.paypal.com/";
        public const string LiveEndpoint = "https://api.paypal.com/";

        private readonly IClock _clock;

        public PaypalApiService(IClock clock) {
            _clock = clock;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public async Task<bool> ValidateCredentialsAsync(PaypalSettings Settings) {
            using (var client = CreateClient(Settings.UseSandbox)) {
                try {
                    return await GetAccessTokenAsync(client, Settings) != null;
                }
                catch {
                    return false;
                }
            }
        }

        public async Task<PaymentContext> CreatePayment(Payment Payment, PaypalSettings Settings) {
            using (var client = CreateClient(Settings.UseSandbox)) {
                try {
                    var token = await GetAccessTokenAsync(client, Settings);
                    var response = await client.PostAsJsonAsync("v1/payments/payment", Payment);
                    if (response.IsSuccessStatusCode) {
                        var createdPayment = await response.Content.ReadAsAsync<Payment>();
                        return new PaymentContext() {
                            UseSandbox = Settings.UseSandbox,
                            Payment = createdPayment,
                            ValidUntil = _clock.UtcNow.AddSeconds(token.ExpiresIn),
                            Token = token
                        };
                    }
                    else {
                        throw new OrchardException(T("Payment creation failed."));
                    }
                }
                catch(Exception exp) {
                    throw new OrchardException(T("Payment creation failed."), exp);
                }
            }
        }

        public async Task<PaymentContext> ExecutePayment(PaymentContext PaymentCtx, string PayerId) {
            using (var client = CreateClient(PaymentCtx.UseSandbox, PaymentCtx.Token)) {
                try {
                    var response = await client.PostAsJsonAsync("v1/payments/payment/" + PaymentCtx.Payment.Id + "/execute", new { payer_id = PayerId });
                    if (response.IsSuccessStatusCode) {
                        var executedPayment = await response.Content.ReadAsAsync<Payment>();
                        return new PaymentContext() {
                            UseSandbox = PaymentCtx.UseSandbox,
                            Payment = executedPayment,
                            Token = PaymentCtx.Token
                        };
                    }
                    else {
                        throw new OrchardException(T("Payment execution failed."));
                    }
                }
                catch (Exception exp) {
                    throw new OrchardException(T("Payment execution failed."), exp);
                }
            }
        }

        #region Private functions
        private HttpClient CreateClient(bool UseSandbox, AccessToken Token = null) {
            var client = new HttpClient();
            client.BaseAddress = new Uri(UseSandbox ? PaypalApiService.SandboxEndpoint : PaypalApiService.LiveEndpoint);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (Token != null) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Token.TokenType, Token.AccessToken);
            }

            return client;
        }

        private async Task<AccessToken> GetAccessTokenAsync(HttpClient Client, PaypalSettings Settings) {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                ASCIIEncoding.ASCII.GetBytes(Settings.ClientId + ":" + Settings.ClientSecret)
            ));

            var response = await Client.PostAsync("v1/oauth2/token", new FormUrlEncodedContent(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            }));

            if (response.IsSuccessStatusCode) {
                var token = await response.Content.ReadAsAsync<AccessToken>();

                if (token != null) {
                    // Set authorization header for further requests
                    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
                }

                return token;
            }
            else {
                throw new OrchardException(T("Unable to obtain Access Token from PayPal API."));
            }
        }

        #endregion
    }
}