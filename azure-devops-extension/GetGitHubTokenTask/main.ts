import * as tl from 'azure-pipelines-task-lib/task';

async function run() {
    try {
        tl.debug("Starting GetGitHubToken task");

        // ---------------------------------------------------------------
        // 1. Read inputs
        // ---------------------------------------------------------------
        const serviceConnection = tl.getInput('gitHubConnection', true)!;
        const variableName = tl.getInput('variableName', true)!;

        tl.debug(`Service connection input: ${serviceConnection}`);
        tl.debug(`Variable name: ${variableName}`);

        // ---------------------------------------------------------------
        // 2. Read service connection info
        // ---------------------------------------------------------------
        const endpointAuth = tl.getEndpointAuthorization(serviceConnection, false);
        if (!endpointAuth) {
            tl.setResult(
                tl.TaskResult.Failed,
                `Could not find service connection: ${serviceConnection}`
            );
            return;
        }

        const scheme = (endpointAuth.scheme || "").toLowerCase();
        tl.debug(`Endpoint scheme: ${scheme}`);

        // DO NOT log endpointAuth.parameters because it contains secrets
        const paramKeys = Object.keys(endpointAuth.parameters || {});
        tl.debug(`Endpoint parameter keys: ${JSON.stringify(paramKeys)}`);

        // ---------------------------------------------------------------
        // 3. Retrieve token according to Microsoft rules
        // ---------------------------------------------------------------
        let token: string | undefined;

        switch (scheme) {

            // -----------------------------------------------------------
            // PAT-based GitHub connections
            // -----------------------------------------------------------
            case "personalaccesstoken":
                tl.debug("Detected PersonalAccessToken scheme");
                token = tl.getEndpointAuthorizationParameter(
                    serviceConnection, 
                    "accessToken", 
                    false
                );
                break;

            // -----------------------------------------------------------
            // OAuth GitHub connections (very old style)
            // -----------------------------------------------------------
            case "oauth":
                tl.debug("Detected OAuth scheme");
                token = tl.getEndpointAuthorizationParameter(
                    serviceConnection,
                    "AccessToken",
                    false
                );
                break;

            // -----------------------------------------------------------
            // GitHub App / InstallationToken connections
            // (This is what Azure Pipelines GitHub App uses)
            // Microsoft tasks read AccessToken here
            // -----------------------------------------------------------
            case "token":
                tl.debug("Detected Token scheme (GitHub App / installation token)");
                token = tl.getEndpointAuthorizationParameter(
                    serviceConnection,
                    "AccessToken",
                    false
                );
                break;

            // -----------------------------------------------------------
            // Fallback for unknown or custom service connections
            // -----------------------------------------------------------
            default:
                tl.debug(`Unknown scheme '${scheme}', trying fallback keys`);
                token =
                    tl.getEndpointAuthorizationParameter(serviceConnection, "AccessToken", true) ||
                    tl.getEndpointAuthorizationParameter(serviceConnection, "accessToken", true) ||
                    tl.getEndpointAuthorizationParameter(serviceConnection, "apitoken", true);
                break;
        }

        // ---------------------------------------------------------------
        // 4. Error if no token could be retrieved
        // ---------------------------------------------------------------
        if (!token) {
            tl.setResult(
                tl.TaskResult.Failed,
                `Could not retrieve token from service connection. Scheme: ${endpointAuth.scheme}`
            );
            return;
        }

        // ---------------------------------------------------------------
        // 5. Set pipeline variable (secret)
        // ---------------------------------------------------------------
        tl.setVariable(variableName, token, true); // secret=true

        tl.debug(`Token retrieved and stored as secret variable '${variableName}'`);
        console.log(`GitHub token successfully retrieved into variable '${variableName}'`);

        tl.setResult(
            tl.TaskResult.Succeeded,
            `GitHub token retrieved from service connection and stored in '${variableName}'`
        );

    } catch (err: any) {
        const message = err?.message || err?.toString() || "Unknown error occurred";
        tl.setResult(tl.TaskResult.Failed, message);
    }
}

run();
