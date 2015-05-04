﻿using Orchard;
using OShop.PayPal.Models;
using OShop.PayPal.Models.Api;
using System.Threading.Tasks;

namespace OShop.PayPal.Services {
    public interface IPaypalApiService : IDependency {
        Task<bool> ValidateCredentialsAsync(PaypalSettings Settings);
        Task<PaymentContext> CreatePayment(Payment Payment, PaypalSettings Settings);
        Task<PaymentContext> ExecutePayment(PaymentContext PaymentCtx, string PayerId);
    }
}