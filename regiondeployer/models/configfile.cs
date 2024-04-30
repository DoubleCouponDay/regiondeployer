using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoonMachine.Core.interop.models
{
    public struct configfile //made a struct because it enforces property initialization
    {
        public mmenvironment currentenvironment { get; set; }

        public string ConnectionString { get; private set; }

        public string GithubId { get; private set; }
        public string GithubSecret { get; private set; }

        public string RecaptchaSiteKey { get; private set; }
        public string RecaptchaSecret { get; private set; }

        public string infrastructureapikey { get; private set; }
        public string infrastructureapilink { get; private set; }

        public string TwillioAccountId { get; private set; }
        public string TwillioAccountToken { get; private set; }
        public string TwillioServiceNumber { get; private set; }

        public string AzureClientId { get; private set; }
        public string AzureClientSecret { get; private set; }
        public string AzureTenantId { get; private set; }
        public string AzureSubscriptionId { get; private set; }

        public string IpStackKey { get; private set; }

        public configfile(
                mmenvironment inputmmenvironment,
                string inputConnectionString,

                string inputGithubId,
                string inputGithubSecret,

                string inputRecaptchaSiteKey,
                string inputRecaptchaSecret,

                string inputinfrastructureapikey,
                string inputinfrastructureapiurl,

                string inputTwillioAccountId,
                string inputTwillioAccountToken,
                string inputTwillioServiceNumber,

                string inputAzureClientId,
                string inputAzureClientSecret,
                string inputAzureTenantId,
                string inputAzureSubscriptionId,

                string inputIpStackKey
            )
        {
            currentenvironment = inputmmenvironment;
            ConnectionString = inputConnectionString;

            GithubId = inputGithubId;
            GithubSecret = inputGithubSecret;

            RecaptchaSiteKey = inputRecaptchaSiteKey;
            RecaptchaSecret = inputRecaptchaSecret;

            infrastructureapikey = inputinfrastructureapikey;
            infrastructureapilink = inputinfrastructureapiurl;

            TwillioAccountId = inputTwillioAccountId;
            TwillioAccountToken = inputTwillioAccountToken;
            TwillioServiceNumber = inputTwillioServiceNumber;

            AzureClientId = inputAzureClientId;
            AzureClientSecret = inputAzureClientSecret;
            AzureTenantId = inputAzureTenantId;
            AzureSubscriptionId = inputAzureSubscriptionId;

            IpStackKey = inputIpStackKey;
        }
    }
}
