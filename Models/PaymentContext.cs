using OShop.PayPal.Models.Api;

namespace OShop.PayPal.Models {
    public class PaymentContext {
        public bool UseSandbox;
        public AccessToken Token;
        public Payment Payment;
    }
}