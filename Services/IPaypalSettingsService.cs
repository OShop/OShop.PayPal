using Orchard;
using OShop.PayPal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OShop.PayPal.Services {
    public interface IPaypalSettingsService : IDependency {
        PaypalSettings GetSettings();
        void SetSettings(PaypalSettings Settings);
    }
}
