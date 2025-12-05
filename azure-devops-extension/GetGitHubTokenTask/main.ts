import * as tl from 'azure-pipelines-task-lib/task';
import * as http from 'typed-rest-client/HttpClient';

async function run() {
    try {
        // Get inputs
        const gitHubConnection: string | undefined = tl.getInput('gitHubConnection', true);
        const variableName: string | undefined = tl.getInput('variableName', true);

        if (!gitHubConnection || !variableName) {
            tl.setResult(tl.TaskResult.Failed, 'Missing required inputs');
            return;
        }

        // Get the service connection endpoint
        const endpoint = tl.getEndpointAuthorization(gitHubConnection, false);
        
        if (!endpoint) {
            tl.setResult(tl.TaskResult.Failed, `Could not find service connection: ${gitHubConnection}`);
            return;
        }

        // Get the token from the service connection
        // The token is stored in the endpoint's authorization parameters
        let token: string | undefined;
        
        if (endpoint.scheme === 'PersonalAccessToken') {
            // For PAT-based connections
            token = endpoint.parameters['accessToken'];
        } else if (endpoint.scheme === 'OAuth') {
            // For OAuth-based connections (GitHub App)
            token = endpoint.parameters['AccessToken'];
        } else if (endpoint.scheme === 'Token') {
            // For token-based connections
            token = endpoint.parameters['apitoken'];
        }

        if (!token) {
            tl.setResult(tl.TaskResult.Failed, `Could not retrieve token from service connection. Scheme: ${endpoint.scheme}`);
            return;
        }

        // Set the token as a secret output variable
        tl.setVariable(variableName, token, true); // true = secret
        console.log(`Token successfully retrieved and stored in variable '${variableName}'`);
        
        tl.setResult(tl.TaskResult.Succeeded, `GitHub token retrieved from service connection and stored in ${variableName}`);
    } catch (err: any) {
        tl.setResult(tl.TaskResult.Failed, err.message || 'Unknown error occurred');
    }
}

run();
