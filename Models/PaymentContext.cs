using OShop.PayPal.Models.Api;
using System;

namespace OShop.PayPal.Models {
    public class PaymentContext {
        public bool UseSandbox;
        public AccessToken Token;
        public DateTime ValidUntil;
        public Payment Payment;
    }
}