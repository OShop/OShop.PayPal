using Orchard;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using OShop.PayPal.Models;
using OShop.PayPal.Services;
using OShop.Permissions;
using System.Web.Mvc;

namespace OShop.PayPal.Controllers
{
    [Admin]
    public class SettingsController : Controller
    {
        private readonly IPaypalSettingsService _settingsService;

        public SettingsController(
            IPaypalSettingsService settingsService,
            IOrchardServices services) {
            _settingsService = settingsService;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(OShopPermissions.ManageShopSettings, T("Not allowed to manage Shop Settings")))
                return new HttpUnauthorizedResult();

            return View(_settingsService.GetSettings());
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult IndexPost(PaypalSettings model) {
            if (!Services.Authorizer.Authorize(OShopPermissions.ManageShopSettings, T("Not allowed to manage Shop Settings")))
                return new HttpUnauthorizedResult();

            if (TryUpdateModel(model)) {
                _settingsService.SetSettings(model);
                Services.Notifier.Information(T("PayPal Settings saved successfully."));
            }
            else {
                Services.Notifier.Error(T("Could not save PayPal Settings."));
            }

            return Index();
        }

    }
}