// See https://aka.ms/new-console-template for more information
using Microsoft.Identity.Client;
using OPI.Client;
using OPI.Core.Models;


OPIClient client = OPIClientFactory.Instance.Create(new Uri("http://localhost:5041"));

PublicClientApplicationOptions options = new PublicClientApplicationOptions()
{
    Instance = "https://login.microsoftonline.com/",
    ClientId = "d0ff64df-c1ee-4200-a971-21d7c8647de2",  // opi-console
    TenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47",
};

string accessToken = await SignInUserAndGetTokenUsingMSAL(options, new[] { "api://5fba0a18-7118-4762-b8c5-f406244e164f/.default" });    // opi-webapi
if (string.IsNullOrEmpty(accessToken))
{
    Console.WriteLine("Sign in is required.");
}
client.UseAccessToken(accessToken);

int count = 0;
foreach (PerfIssueRegisterEntry entry in await client.ListAllRegisteredAsync(default).ConfigureAwait(false))
{
    Console.Write("[{0}]Processing entry: {1} - {2:D} ... ", count++, entry.Title, entry.PermanentId);
    PerfIssueRegisterEntry updateSpec = entry with
    {
        Rationale = @"{value}% of your {issueCategory} was spent in `{symbol}`, We expected this value to be {relation} {criteria}%.",
    };
    await client.UpdateEntryAsync(updateSpec, default);
    Console.WriteLine("Done.");
}

async Task<string> SignInUserAndGetTokenUsingMSAL(PublicClientApplicationOptions configuration, string[] scopes)
{
    // build the AAd authority Url
    string authority = string.Concat(configuration.Instance, configuration.TenantId);

    // Initialize the MSAL library by building a public client application

    Console.WriteLine("For client id: {0}", configuration.ClientId);
    IPublicClientApplication application = PublicClientApplicationBuilder.Create(configuration.ClientId)
                                            .WithAuthority(authority)
                                            .WithRedirectUri("http://localhost")
                                            .Build();
    AuthenticationResult result;

    try
    {
        var accounts = await application.GetAccountsAsync();

        Console.WriteLine("Account number: {0}", accounts.Count());
        result = await application.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
    }
    catch (MsalUiRequiredException)
    {
        result = await application.AcquireTokenInteractive(scopes).ExecuteAsync();
    }
    return result.AccessToken;
}