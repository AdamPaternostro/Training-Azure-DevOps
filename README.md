# Training-Azure-DevOps
This lab/walkthrough shows how to create a build and release pipeline that demostrates:
- Infrastructure as Code
  - Deploys an App Service
  - Deploys a Web App
  - Deploys a Function App (consumption plan using v2)
- Creates a build defination with CI
  - Builds code
  - Zips an Azure Function
  - Publishes code
- Create a release defination with CD
  - Deploys an ARM template
  - Uses Variables
  - Deploys a web app
  - Swap web app slots
  - Uses approvals
  - Uses conditions
  - Uses cloning to quickly duplicate environments
  - Uses gates
- Create a build defination for a Docker Image
  - Creates an Azure Container Registry
  - Builds the Docker image
  - Pushs the Docker image to an Azure Container Registry
- Create a release defination for a Docker Image
  - Creates a Linux App Service
  - Creates a Linux Web App 
  - Releases the Docker image from the ACR


# Setup
### Azure Setup
1. Create a resource group in Azure named: Training-Azure-DevOps
2. Create a resource group in Azure named: Training-Azure-DevOps-Docker
2. Create a service principle in Azure named (web/api): Training-Azure-DevOps-SP
3. Grant the service principle Contributor access to the resource groups


### Azure DevOps Setup
1. Create an Azure DevOps project named: Training-Azure-DevOps


# Web App Build and Release
### Open Azure DevOps
1. Click on Repos | Files
2. Click on Import under "or import a Repository"
3. Enter: https://github.com/AdamPaternostro/Training-Azure-DevOps.git


### Create a build defination
We will be using the visual interface
1. Click on Pipelines | Builds
2. Click on New Pipeline button
3. Click on the small link "Use the visual designer"
4. Click on Azure Repos Git (should all be filled out) | Click Continue
5. Select template "ASP.NET Core"
6. Click Save & Queue (make sure it works)
7. Click on the Artifacts button.  You should see a zip.


### Alter build defination
1. Add a Copy File task (click +)
2. Move it above the Publish Artifact task
2. Set the Source folder: SampleWebApp-ARM-Templates
3. Set the Target folder: $(build.artifactstagingdirectory)
4. Build again
5. View artifacts and you should see arm-template.json in the artifacts


### Create a Release pipeline (Deploy ARM Template)
1. Click on Pipelines | Releases | New Pipeline button
2. Click on Empty Job link (where it says Select a template)
3. Rename Stage 1 to Dev and click the X to close
4. Click on Add an Artifact
5. Select your build
6. Select Default version: Latest
7. Click on Variables at the top
8. Add variables
   - Environment -> DEV  (Note if you are working with several people all sharing a resource group them make this DEV-{yourname})
   - AppServicePlan -> TrainingAzureDevOpsAppServicePlan (you can specify what you like - must be globally unique in Azure)
   - WebApp -> TrainingAzureDevOpsWebApp (you can specify what you like - must be globally unique in Azure)
   - ResourceGroup -> Training-Azure-DevOps
9. Click on "New release pipeline" and change the name to something you like
10. Click Save
11. Click on Dev Stage
12. Add a task (click +)
13. Select "Azure Resource Group Deployment"
14. Click on tasks
    - Click the manage link next to "Azure Subscription"
    - Click on New Service Connection
    - Click on Azure Resource Manager
    - Click on "Use full version of this dialog" (link at bottom)
    - Give it a name
    - Enter the Service Principle Id (Application Id)
    - Enter the Service Principle Key
    - Test the connection
    - If you get an error make sure the service principle exists and has Contributor access to your resource groups
    - Close the tab and return to your pipeline
    - Click the refresh button next to Azure Subscription
    - Select the connection you just setup
    - Enter the resource group name: $(ResourceGroup)  
    - Pick your location (I keep mine the same as my resource group)
    - Pick your template "arm-template.json"
    - Leave parameters blank, we do not have a parameters file
    - In "Override template parameters" enter: 
      ```
      -serverfarms_TrainingAzureDevOpsAppServicePlan_name $(AppServicePlan)-$(Environment) -sites_TrainingAzureDevOpsWebApp_name $(WebApp)-$(Environment) 
      ```
    - Leave Deployment Mode as Incremental (this is very important as Complete will remove unused resources which can do things like delete a storage account)
15. Save
16. Click on Release | Create a Release
   - You can click the Release-1 link to view the release
   - You can view the resources being created in the Azure Portal


### Release pipeline (Deploy Code)
1. Edit the release pipeline
2. Add a task to the Dev stage "Azure App Service Deploy"
3. Edit the settings
   - Set the subscription
   - Set App Service name to $(WebApp)-$(Environment)
   - Set the Package / Folder to the SampleWebApp.zip file (use the selector)
   - Set the App Settings to "-Environment $(Environment)" under Application and Configuration Settings
4. Disable the ARM deployment task (right click and disable). This is a time saver during development.
5. Save and Run a release
6. After the release open the website (e.g. https://trainingazuredevopswebapp-dev.azurewebsites.net/)


### Release pipeline (Deploy to slot)
1. Edit the release pipeline
2. Edit the Web App Deployment
3. Click Deploy to Slot
   - Set resource group: $(ResourceGroup)
   - Set Slot to: Proprod
4. Save and Run a release
5. After the release open the websites 
   - e.g. https://trainingazuredevopswebapp-dev.azurewebsites.net/
   - e.g. https://trainingazuredevopswebapp-dev-PREPROD.azurewebsites.net/
6. You will notice the ServerName is the same (both apps are on the same server)


### Release pipeline (Swap slot)
1. Edit the release pipeline
2. Add a task to the Dev stage "Azure App Service Manage"
3. Edit the settings
   - Set the subscription
   - Set the App Service name: $(WebApp)-$(Environment)
   - Set the Resource Group: $(ResourceGroup)
   - Set the Source Slot: Preprod  
4. Save and Run a release

### Create QA Release
1. Edit the release pipeline
2. Edit variables
3. Change Environment variable Scope to DEV
4. Click on Pipeline
5. Clone the Dev Stage
6. Click on new Stage and rename to QA
7. Click on variables, you will see Environment twice (once for each stage)
8. Change the QA scope to "QA" for the value
9. Click on the QA stage and re-enable the ARM task
10. Save and Run a release
   - You can quickly make new environments onces you have your Dev pipeline working

### Create Production Release
1. Edit the release pipeline
2. Clone QA
3. Rename to Prod
4. Edit variables
5. In QA disable the ARM task
6. Clone Prod
7. Rename cloned Stage to Prod-Swap and Edit
8. Delete the ARM task from Prod-Swap Stage
9. Delete the Web Deploy task from Prod-Swap Stage
10. Edit the Prod stage
11. Delete the Swap Slots task (we only want this task in the Prod-Swap stage)
12. Save and Run a release

### Add conditions
1. Added a variable called DeployARMTemplate and set the value to false
2. For each stage
   - Enable the ARM template
   - Edit the step
   - Under Control Options
   - Set Run this task to: Custom Conditions
   - Set Custom condition: eq(variables['DeployARMTemplate'], 'true')
3. Save and Run a release
   - You should see the ARM template skipped

### Add approvals
1. Edit the release pipeline
2. Click on the person icon on the right side of the Dev stage
3. Enable "Post-deployment approvals"
4. Enter your name
5. Click on the person icon on the right side of the QA stage
6. Enable "Post-deployment approvals"
7. Enter your name
8. Save and Run a release
   You can approve on https://dev.azure.com or click on the email


### Change the code and enable automatic builds/releases
1. Edit the Build defination
2. Click on Triggers
3. Click the "Enable continuous integration" checkbox
4. Save the Build (do not Save and queue)
5. Edit the Release pipeline
6. Click on the Lightning bolt icon on the Artifacts
7. Enable "Continuous deployment trigger"
8. Save the Release
9. Click on Repo | Files
10. Edit the file SampleWebApp/Views/Home/Index.cshtml
11. At the bottom enter
   ```
   <div id="row">
    <div class="col-md-12">
        New Release
    </div>
   </div>
   ```
12. Save the file
   - The build should kick off automatically (click on bulids)
   - The release should kick off automatically (click on releases when build is done)
   - You can verify the new code is moving through environments by viewing the websites (the prod slot will have the changes, the old site will be in the preprod slot)

### Implement a Gate
1. Edit the Build 
2. Clone the Copy Files task SampleWebApp-ARM-Templates
3. Edit the new task and change the folder to AzureFunction-ARM-Templates
4. Add a new task Archive Files
   - Root Folder: AzureFunction-Code
   - Archive File to Create: $(Build.ArtifactStagingDirectory)/AzureFunction.zip
5. Save and Build
6. Edit the Prod stage
7. Clone the ARM task
   - Edit Template path (e.g. $(System.DefaultWorkingDirectory)/_Training-Azure-DevOps-Build/drop/arm-function-template.json)
   - Edit the Override template parameters 
      - NOTE: You might need to change the storage account name (functionappstorage001) to be unique
     ```
     -AzureFunctionPlanName $(AppServicePlan)-Function-$(Environment) -AzureFunctionAppName $(WebApp)-Function-$(Environment)  -AzureFunctionStorageAccountName functionappstorage001
     ```
8. Clone the App Service Deploy task
   - Change App Type to Function App
   - Uncheck deploy to slot
   - Change the Package / Folder path (e.g. $(System.DefaultWorkingDirectory)/_Training-Azure-DevOps-Build/drop/AzureFunction.zip)
9. Change the variable DeployARMTemplate to "true" (since we have a new ARM template). 
   - You can disable the approvals as well to save time.     
10. Save and run to make sure things are working, you should also check to make sure the Function App was deployed      
11. Change the variable DeployARMTemplate to "false" if things worked
12. Go to the Function App in the Azure Portal and copy the URL / security code
13. Click on the Lightning bolt of the Prod Swap stage
    - Enable a gate
    - In the Azure Portal go to your fucntion app, you need to copy the URL for it
    - Enter the function url (e.g. https://trainingazuredevopswebapp-function-prod.azurewebsites.net/api/AzureDevOpsFunctionGate)
    - Enter the code (e.g. kw99xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=)
    - Under advanced for Success Criteria enter "eq(root['status'], 'true')"
    - NOTE: If you have an apporval you can check the box "On successfuly gates, ask for approvals", this means your gate will be evaluated first and must be successful before asking for an approval
    - You can change the evaluation times as well for testing
14. Save and Run a release
    - You can watch the gate perform its test
    

## Notes 
- If your web app gets a DLL locked, then add a task (Azure App Service Manage) to restart the staging slot web app

- Some customers like to tear down the staging slot after deployments.  You can add this part of your pipeline.  I usually wait 1 to 2 hours before tearing down staging since if something goes wrong with the release your can quick swap slots.  You can add a step to perform the delete after a certain amount of time (e.g. Gate or an appoval with a deferred time).  Instead of "deleting" you staging slow, you can deploy an index.html file that is just empty and for Azure Functions you can have an empty function.

- My preprod staging slot is pointing to production resources and production databases

- If you are using Web Apps and use Slot specific variables be-aware that after your slot swap the appliction must recycle to load the values!  This negates most of the benefits of slots.  If your application has a quick warm up time for the first call, you might be alright, but if not, your users can see a delay.

- If your web app is event based, for instance an Azure Function that triggers based upon a blob, then when you deploy to a the preprod staging slot, this function is processing production data!
   - How do you handle this?  For a blob trigger, if your preprod slot picks up the blob, it is not like it can pass the event to the production slot.  You also cannot have the production slot monitoring one container and preprod monitoring a different one.  
   - My perference for all event driven logic, have an Azure function that places items in a queue (or just place the item in the queue to begin with).  Then process the queue only if you are in the production slot.  You can test the URL of the application in some cases (not if you are directing 90% of your traffic the production slot and 10% to staging).  Having an application configuration value will not help since when you swap slots the value will not change.  I worry about missing events - especailly when new code is being deployed for an event based process.

# Docker Build and Release

### Clean Up
1. Disable automatitic builds for pipeline. Since we are using a single repo for this training we want to stop automatic builds.


### Create a build pipeline
1. Create a new Build Defination and select "Docker task"
2. Add variables
   - ACR_Name -> TrainingAzureDevOpsContainerReg
   - AzureContainerRegistryConnection -> {"loginServer":"trainingazuredevopscontainerregdev.azurecr.io", "id" : "/subscriptions/{REPLACE ME}/resourceGroups/Training-Azure-DevOps-Docker/providers/Microsoft.ContainerRegistry/registries/trainingazuredevopscontainerregdev"}
   - Environment -> DEV  (Note if you are working with several people all sharing a resource group them make this DEV-{yourname})
   - ResourceGroup -> Training-Azure-DevOps-Docker
3. Add a task Azure Resource Group Deployment (move to the top of the task list)
   - Set the Azure subscription
   - Set the Resource Group: $(ResourceGroup)
   - Set the Location: East US
   - Set the Override template parameters: 
   ```
   -ACR_Name $(ACR_Name)$(Environment)
   ```   
4. Click on Build Docker task
   - Set the Azure subscription
   - Set the Azure Container Registry: $(AzureContainerRegistryConnection)
5. Click on Push Docker image task
   - Set the Azure subscription
   - Set the Azure Container Registry: $(AzureContainerRegistryConnection)
5. Add a Copy File task (at the bottom)
   - Set the Source folder: Sample-Docker-ARM-Templates
   - Set the Target folder: $(Build.ArtifactStagingDirectory)
5. Add a Publish task  
6. Save and Queue

Check you build artifacts and make sure the Azure Container Registry was created.  We need to create this as part of the build (versus release).


### Create a Release pipeline
1. Create a new release pipeline
2. Select Empty job
3. Link your Artifacts
4. Add variables
   - ACR_Name -> TrainingAzureDevOpsContainerReg
   - ACR_Password -> Get from Azure Portal
   - AppServicePlan -> TrainingAzureDevOpsLinuxPlan (you can specify what you like - must be globally unique in Azure)
   - Environment -> DEV  (Note if you are working with several people all sharing a resource group them make this DEV-{yourname})
   - ResourceGroup -> Training-Azure-DevOps-Docker
   - WebApp -> TrainingAzureDevOpsLinuxApp (you can specify what you like - must be globally unique in Azure)
 5. Add a task Azure Resource Group Deployment
   - Set the AAzure subscription
   - Set the Resource group: $(ResourceGroup)
   - Set Location:  "East US"
   - Set the Template location: $(System.DefaultWorkingDirectory)/_Training-Azure-DevOps-Docker/drop/arm-template-web.json
   - Set Override template parameters
   ```
   -sites_azuretrainingdockerimageapp_name $(WebApp)-$(Environment) -serverfarms_azuretrainingdockerimageappservice_name $(AppServicePlan)-$(Environment) -ACR_Name $(ACR_Name)$(Environment) -ACR_Password $(ACR_Password) -DockerImageName $(Build.Repository.Name) -DockerTag $(Build.BuildId)
   ```
6. Save and then Queue

Verify that the website is working

### Notes
- You have to create your ACR before building/pushing our Docker image.  This is why the ACR ARM template is in the Build pipeline.  I consider this a "build" resource which is why it is created here.  Typically, I place all my ARM templates in my Release pipeline.
- For best Docker build performance use your own agent so you do not need to rebuild every layer (which will happen on a hosted agent)
- You might have noticed we are getting the ACR password by hand.  A better apporach would be to save the password to Azure KeyVault when creating the ACR.  Then reference the KeyVault value with a Variable Group.
- When creating Docker image try to keep your layers < 100 MB.  The layers are stored in Azure Blob storage and what you want is many layers being pulled in parallel.  Having a 1 GB layer will have issues if 10 web servers are trying to pull at the same time.
- If you notice we are not tagging the Docker image with Latest.  We are using specific build numbers.  This helps in Rollback and a lot of people just pull latest, but if are using Slots then this can cause confusion.


# DevOps Best Practices
https://github.com/AdamPaternostro/Training-Azure-DevOps/blob/master/DevOps-Best-Practices.md