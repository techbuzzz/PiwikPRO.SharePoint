using Microsoft.Azure.WebJobs;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using PiwikPRO.SharePoint.Shared;
using PiwikPRO.SharePoint.Shared.Helpers;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace PiwikPRO.SharePoint.WebJob
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        private static string ClientId = "2fd1e765-ca9c-4cab-aaa5-32d960f2764a";
        private static string Thumbprint = "597B0116AB9FB9EAC6D11E1755C4C4218AE91219";
        private static string Authority = "https://login.windows.net/kogifidev3.onmicrosoft.com/";

        public static void ExecuteTimer([TimerTrigger("0 */5 * * * *")]TimerInfo timer, TextWriter log)
        {
            OfficeDevPnP.Core.AuthenticationManager authMan = new OfficeDevPnP.Core.AuthenticationManager();
            using (ClientContext ctx = authMan.GetAppOnlyAuthenticatedContext(ConfigurationManager.AppSettings["PiwikAdminSiteUrl"], ConfigurationManager.AppSettings["PiwikAzureAppKey"], ConfigurationManager.AppSettings["PiwikAzureAppSecret"]))
            {
            //using (ClientContext ctx = new ClientContext(ConfigurationManager.AppSettings["PiwikAdminSiteUrl"]))
            //{
                Functions f = new Functions();
                // Use default authentication mode  
                //ctx.AuthenticationMode = ClientAuthenticationMode.Default;
                //ctx.Credentials = new SharePointOnlineCredentials(f.GetSPOAccountName(), f.GetSPOSecureStringPassword());

                AzureLogger splogger = new AzureLogger();
                splogger.WriteLog(Category.Information, "Piwik PRO Job", "Started");
                Configuration cfg = new Configuration(splogger, ctx);
                PiwikPROJobOperations pbjo = new PiwikPROJobOperations(cfg, splogger);

                pbjo.GetAllNewSitesAndOperate(ctx, ConfigurationManager.AppSettings["PiwikAzureAppKey"], ConfigurationManager.AppSettings["PiwikAzureAppSecret"], ConfigurationManager.AppSettings["PiwikAdminTenantSiteUrl"]);
                pbjo.GetAllDeactivatingSitesAndOperate(ctx, ConfigurationManager.AppSettings["PiwikAzureAppKey"], ConfigurationManager.AppSettings["PiwikAzureAppSecret"], ConfigurationManager.AppSettings["PiwikAdminTenantSiteUrl"]);

                splogger.WriteLog(Category.Information, "Piwik PRO Job", "Finished");
            }
        }

        public SecureString GetSPOSecureStringPassword()
        {
            try
            {
                var secureString = new SecureString();
                foreach (char c in ConfigurationManager.AppSettings["AzureLoginPassword"])
                {
                    secureString.AppendChar(c);
                }
                return secureString;
            }
            catch
            {
                throw;
            }
        }
        public string GetSPOAccountName()
        {
            try
            {
                return ConfigurationManager.AppSettings["AzureLoginName"];
            }
            catch
            {
                throw;
            }
        }
    }
}