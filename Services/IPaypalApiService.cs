using Orchard;
using OShop.PayPal.Models;
using OShop.PayPal.Models.Api;
using System.Threading.Tasks;

namespace OShop.PayPal.Services {
    public interface IPaypalApiService : IDependency {
        Task<bool> ValidateCredentialsAsync(PaypalSettings Settings);
        Task<PaymentContext> CreatePaymentAsync(Payment Payment, PaypalSettings Settings);
        Task<PaymentContext> ExecutePaymentAsync(PaymentContext PaymentCtx, string PayerId);
    }
}
