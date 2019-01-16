# Training-Azure-DevOps
Using Azure Dev Ops to deploy Infrastructure as Code, Web application, Staging Slots and Docker images.


### Setup
1. Create a resource group in Azure named: Training-Azure-DevOps
2. Create a service principle in Azure named: Training-Azure-DevOps-SP
3. Grant the service principle contributor access to the resource group
4. Create an Azure DevOps project named: Training-Azure-DevOps



Azure DevOps

ARM:
App Service Plan Name: TrainingAzureDevOpsAppServicePlan-DEV
Web App Name: TrainingAzureDevOpsWebApp-DEV
Slot: Pre-Prod


Create an Azure App Service
Create an Azure Web App (Windows)
Import code 
Build code
Publish artifacts
Publish website
cd "O
Add Staging slot
Publish to slot
Slot Swap

Create Azure container registry
Create Azure Linux Web App
Import code
Build Docker
Push to container registry
Publish to Linux App

Create Azure Function gate
Add Staging slot to Linux App
Publish to staging
Call Gate
Swap if Gate is "true"



