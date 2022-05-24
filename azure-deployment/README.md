# Id Crypt Agent one-click deployment to Azure

This repository contains the templates used to build the one-click deployment of ID Crypt agent to Azure. This includes an automated pipeline to build the templates, upload them to a storage account, and generate the custom deployment link used to trigger a deployment process on the Azure portal.

## Deployment

### From the CLI

You can deploy the Bicep templates from the CLI following the next steps.

First, create a resource group that will host the resources.

```
az group create -l westeurope -n one-click-deployment-test
```

Then, copy the `main.local.parameters.json` into a new file and update the parameters with your desired configuration.

Finally, you can kick off the deployment with the following command.

```
az deployment group create \
    -g one-click-deployment-test \
    -f azure/templates/main.bicep \
    --parameters @azure/templates/main.test.parameters.json
```

### From the portal

The final state of the portal deployment will be driven from the Azure Marketplace. However, during the development, templates have to be made available from a public source. One way to achieve this is uploading the templates to a storage account and build a custom deployment link like described [here](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-to-azure-button).

We can create a deployment link that contains URL-encoded links to the publicly accessible ARM template and UI definition template. In this case, the link looks like follows.

```
https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fidcryptsovrin.blob.core.windows.net%2Ftemplates%2Fmain.json/createUIDefinitionUri/https%3A%2F%2Fidcryptsovrin.blob.core.windows.net%2Ftemplates%2Fmain.portal.json
```

This link can be used to generate a "Deploy to Azure" button as seen below and kick off the deployment through a customized user experience within the Azure portal.

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FRTGS-OpenSource%2Frtgs-idcrypt-service%2Ffeature%2F3846-deploy-agent%2Fazure-deployment%2Fazure%2Ftemplates%2Fmain.json/createUIDefinitionUri/https%3A%2F%2Fraw.githubusercontent.com%2FRTGS-OpenSource%2Frtgs-idcrypt-service%2Ffeature%2F3846-deploy-agent%2Fazure-deployment%2Fazure%2Ftemplates%2Fmain.portal.json)
