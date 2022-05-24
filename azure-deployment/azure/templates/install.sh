#!/usr/bin/env bash
set -euxo pipefail

namespace=default
# helmVersion=3.8.2

# declare env vars used in the script to fail fast if they are not provided
KUBELET_IDENTITY_CLIENT_ID=${KUBELET_IDENTITY_CLIENT_ID}
KEYVAULT_NAME=${KEYVAULT_NAME}
TENANT_ID=${TENANT_ID}

echo "Agent install script"

az version
OS=$(echo `uname`|tr '[:upper:]' '[:lower:]')
ARCH=$(uname -m)
echo "OS=$OS"
echo "ARCH=$ARCH"

echo "Installing kubectl..."
az aks install-cli

echo "Installing Helm..."
# wget -O helm.tgz "https://get.helm.sh/helm-v${helmVersion}-linux-amd64.tar.gz"
# tar -zxf helm.tgz
# mv -f linux-amd64/helm /usr/local/bin/helm
curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3
chmod 700 get_helm.sh
./get_helm.sh
helm version

echo "Logging in to Azure..."
az login --identity

echo "Getting cluster credentials..."
az aks get-credentials --resource-group ${RESOURCE_GROUP_NAME} --name ${CLUSTER_NAME}
kubectl config set-context --current --namespace=$namespace

echo "Obtaining agent configuration from Key Vault..."

function get_secret() {
    az keyvault secret show --name $1 --vault-name $KEYVAULT_NAME --query "value" -o tsv
}

environment=$(get_secret environment)
agentLabel=$(get_secret agentLabel)
agentEndpoint=$(get_secret agentEndpoint)
agentAdminApiKey=$(get_secret agentAdminApiKey)
idcryptApiKey=$(get_secret idcryptApiKey)
idcryptToken=$(get_secret idcryptToken)
idcryptCredentialDefinition=$(get_secret idcryptCredentialDefinition)
idcryptOrganizationId=$(get_secret idcryptOrganizationId)
walletName=$(get_secret walletName)
walletKey=$(get_secret walletKey)
walletSeed=$(get_secret walletSeed)
postgresUrl=$(get_secret postgresUrl)
postgresUser=$(get_secret postgresUser)
postgresPassword=$(get_secret postgresPassword)


echo "Installing ID Crypt agent..."

helm repo add idcryptsovrin https://idcryptsovrin.azurecr.io/helm/v1/repo
helm repo update

cat <<EOF >>values.yaml
ingress:
  # Ingress for the agent's communication protocol (DIDcomm)
  didcomm:
    enabled: true
    className: ""
    annotations:
      kubernetes.io/ingress.class: azure/application-gateway
      appgw.ingress.kubernetes.io/health-probe-port: "4000"
      appgw.ingress.kubernetes.io/health-probe-path: "/"
      appgw.ingress.kubernetes.io/backend-protocol: http
      # appgw.ingress.kubernetes.io/appgw-ssl-certificate: kvsslcert
    hosts:
      - host: "$PUBLIC_FQDN"
        paths:
          - path: /
            pathType: Prefix
  # Ingress for the agent's admin API
  admin:
    enabled: true
    className: ""
    annotations:
      kubernetes.io/ingress.class: azure/application-gateway
      appgw.ingress.kubernetes.io/health-probe-port: "11000"
      appgw.ingress.kubernetes.io/health-probe-path: "/api/doc"
      appgw.ingress.kubernetes.io/backend-protocol: http
      appgw.ingress.kubernetes.io/use-private-ip: "true"
      # App Gateway doesn't support multiple IPs on the same port (e.g. 443)
      # Therefore, we will listen on port 8443 for the Agent API.
      # https://github.com/Azure/application-gateway-kubernetes-ingress/issues/948
      # appgw.ingress.kubernetes.io/override-frontend-port: "8443"
      # appgw.ingress.kubernetes.io/appgw-ssl-certificate: kvsslcert
    hosts:
      - host: "$PUBLIC_FQDN"
        paths:
          - path: /
            pathType: Prefix
EOF


helm upgrade --install agent \
    idcryptsovrin/idcrypt-agent \
    --wait --timeout=4m \
    --namespace default \
    --set-string podAnnotations.date=$(date +'%s') \
    --set replicaCount=3 \
    --set environment="${environment}" \
    --set idcrypt=null \
    --set agent.label="${agentLabel}" \
    --set agent.endpoint="${agentEndpoint}" \
    --set agent.adminApiKey="${agentAdminApiKey}" \
    --set wallet.seed="${walletSeed}" \
    --set wallet.name="${walletName}" \
    --set wallet.key="${walletKey}" \
    --set wallet.autoProvision=true \
    --set storage.type="postgres" \
    --set storage.postgres.url="${postgresUrl}" \
    --set storage.postgres.user="${postgresUser}" \
    --set storage.postgres.password="${postgresPassword}" \
    -f values.yaml
# --set idcrypt.organizationId="${idcryptOrganizationId}" \
# --set idcrypt.token="${idcryptToken}" \
# --set idcrypt.apiKey="${idcryptApiKey}" \
# --set idcrypt.credentialDefinition="${idcryptCredentialDefinition}" \


exit 0
